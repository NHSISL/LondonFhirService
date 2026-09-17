// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Clients.AuditAndMetrics.Models.Configurations;
using LondonFhirService.Core.Abstractions.Models.Metrics;

namespace LondonFhirService.Clients.AuditAndMetrics.Brokers.Metrics
{
    internal class MetricBroker : IMetricBroker
    {
        /// <summary>
        /// Cached by name and shared across every instance. An ActivitySource registers itself
        /// with the diagnostics subsystem for its lifetime, and this broker is transient inside a
        /// client that is scoped per request - creating one per instance would leak a listener
        /// registration per request. There is one distinct name in practice, so the dictionary
        /// holds one entry.
        /// </summary>
        private static readonly ConcurrentDictionary<string, ActivitySource> ActivitySources = new();

        /// <summary>
        /// 16 valid hex characters that nonetheless name no span. CorrelationBroker.ReadSpanId
        /// rejects it at the producing end; this rejects it at the consuming end, because the
        /// library takes span ids from any host that satisfies IRequestTraceBroker.
        /// </summary>
        private const string EmptySpanIdHexString = "0000000000000000";

        private readonly ActivitySource activitySource;

        public MetricBroker(AuditAndMetricsConfigurations configurations) =>
            this.activitySource = ActivitySources.GetOrAdd(
                configurations.ActivitySourceName,
                name => new ActivitySource(name));

        public async ValueTask RecordAsync(
            List<IMetric> metrics,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (IMetric metric in metrics)
            {
                await RecordAsync(metric, cancellationToken);
            }
        }

        public async ValueTask RecordAsync(IMetric metric, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Captured BEFORE StartActivity. StartActivity makes the replay span current, so
            // reading Activity.Current afterwards hands back the replay span itself - restoring
            // that after Stop() would push a stopped activity back in, which the setter refuses,
            // leaving Activity.Current null exactly as if nothing had been restored.
            Activity ambientActivity = Activity.Current;

            // Positional rather than named: the name-first and name-last overloads are both
            // applicable once the arguments are named, and the call becomes ambiguous.
            Activity activity = this.activitySource.StartActivity(
                $"{metric.Method}/{metric.Name}",
                ToActivityKind(metric.Type),
                CreateTraceContext(metric.CorrelationId, metric.RequestSpanId),
                null,
                null,
                metric.Started);

            // Null whenever nothing is listening, which is the normal state when telemetry
            // collection is switched off. Spans are still persisted by the storage broker.
            if (activity is null)
            {
                return;
            }

            activity.SetTag("metric.id", metric.Id.ToString());
            activity.SetTag("metric.parentId", metric.ParentId?.ToString());
            activity.SetTag("metric.correlationId", metric.CorrelationId.ToString());
            activity.SetTag("metric.method", metric.Method);
            activity.SetTag("metric.type", metric.Type.ToString());
            activity.SetTag("metric.name", metric.Name);
            activity.SetTag("metric.target", metric.Target);
            activity.SetTag("metric.durationMs", metric.DurationMs);
            activity.SetTag("metric.status", metric.Status.ToString());
            activity.SetTag("metric.errorCode", metric.ErrorCode);
            activity.SetTag("metric.payloadBytes", metric.PayloadBytes);
            activity.SetTag("metric.consumer", metric.Consumer);

            activity.SetStatus(
                code: metric.Status == MetricStatus.Succeeded
                    ? ActivityStatusCode.Ok
                    : ActivityStatusCode.Error,
                description: metric.ErrorCode);

            // Set before Stop, otherwise Stop stamps the end time as now and the replayed span
            // reports the time since it was recorded rather than the time it took.
            activity.SetEndTime(metric.Completed.UtcDateTime);

            // Stop() restores Activity.Current to activity.Parent, and this activity was started
            // from an explicit ActivityContext rather than a parent Activity - so Parent is null
            // and Stop would leave Activity.Current null rather than as it found it. On a host
            // that records a metric inline on the request thread, everything logged afterwards
            // would then reach Application Insights with no operation id. Harmless when the
            // replay runs on the drain worker; not harmless everywhere, so it is put back.
            activity.Stop();
            Activity.Current = ambientActivity;
        }

        /// <summary>
        /// Derives a trace id from the correlation id so every span of one request lands under a
        /// single operation in the telemetry viewer.
        ///
        /// Every span of a request is given the SAME parent, so they replay as siblings rather
        /// than as the tree they actually form. That is deliberate, not a shortcut: reproducing
        /// the real nesting made the metric view noisy enough to be hard to read, and the point
        /// of the telemetry copy is a scannable overview. The exact tree is carried in the
        /// metric.id and metric.parentId tags and in the metrics table, which stays the
        /// authoritative store and is where nesting should be reconstructed from. Do not "fix"
        /// this into a faithful hierarchy without agreeing the UI change that goes with it.
        ///
        /// Built from the correlation id's hex rather than its bytes. A correlation id now IS a
        /// W3C trace id - the host takes it from the request's Activity, which carries whatever
        /// the caller sent in "traceparent" - and Guid's byte order is little endian for its first
        /// three fields while a trace id's is not. Going through ToByteArray therefore rebuilt a
        /// byte-shuffled trace id, so replayed spans landed under an operation that had nothing
        /// else in it rather than under the caller's own trace. The hex form round trips exactly.
        /// Spans grouped per request either way; what this fixes is which trace they join.
        ///
        /// The shared parent is the request's own span when the host could name it, which is what
        /// anchors the flat group underneath the incoming request in the transaction view instead
        /// of leaving it floating beside it. Flattening and anchoring are independent: the group
        /// stays flat either way, it just now hangs from something real.
        ///
        /// Without a request span id - a background worker, or a host that establishes none - it
        /// falls back to a parent derived from the correlation id. That id is synthetic, no span
        /// is ever emitted under it, and the siblings then float at the top of the trace. Correct
        /// grouping, weaker placement, which is the best available when there is no request.
        /// </summary>
        private static ActivityContext CreateTraceContext(Guid correlationId, string requestSpanId)
        {
            if (correlationId == Guid.Empty)
            {
                return default;
            }

            ActivitySpanId parentSpanId =
                CreateSpanId(requestSpanId)
                    ?? ActivitySpanId.CreateFromBytes(
                        correlationId.ToByteArray().AsSpan(start: 0, length: 8));

            return new ActivityContext(
                traceId: ActivityTraceId.CreateFromString(correlationId.ToString("N")),
                spanId: parentSpanId,
                traceFlags: ActivityTraceFlags.Recorded);
        }

        /// <summary>
        /// Null for anything that is not a usable W3C span id, so it falls back to the derived
        /// parent rather than throwing - a telemetry copy must never take down the write path it
        /// rides along with.
        ///
        /// The rule is the same one CorrelationBroker.ReadSpanId applies when it produces the
        /// value, and it has to be: an all-zero span id is 16 valid hex characters, so a length
        /// check alone accepted it and anchored every span to a parent of zeroes - worse placement
        /// than no request span at all, because it skipped the fallback. Lower-cased first because
        /// CreateFromString rejects uppercase hex, which would otherwise discard a perfectly good
        /// span id from any producer that happened to upper-case it.
        /// </summary>
        private static ActivitySpanId? CreateSpanId(string requestSpanId)
        {
            bool isUsable =
                string.IsNullOrWhiteSpace(requestSpanId) is false
                    && requestSpanId.Length == 16
                    && requestSpanId != EmptySpanIdHexString
                    && IsHexadecimal(requestSpanId);

            return isUsable
                ? ActivitySpanId.CreateFromString(requestSpanId.ToLowerInvariant())
                : null;
        }

        private static bool IsHexadecimal(string value)
        {
            foreach (char character in value)
            {
                if (Uri.IsHexDigit(character) is false)
                {
                    return false;
                }
            }

            return true;
        }

        private static ActivityKind ToActivityKind(MetricType metricType) =>
            metricType switch
            {
                MetricType.Request => ActivityKind.Server,
                MetricType.AccessCheck => ActivityKind.Client,
                MetricType.Provider => ActivityKind.Client,
                MetricType.ProviderCall => ActivityKind.Client,
                MetricType.Persist => ActivityKind.Client,
                _ => ActivityKind.Internal
            };
    }
}
