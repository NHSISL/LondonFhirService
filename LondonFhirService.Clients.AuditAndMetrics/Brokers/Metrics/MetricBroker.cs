// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Abstractions.Models.Metrics;

namespace LondonFhirService.Clients.AuditAndMetrics.Brokers.Metrics
{
    internal class MetricBroker : IMetricBroker
    {
        public const string ActivitySourceName = "LondonFhirService.Metrics";

        private static readonly ActivitySource activitySource =
            new ActivitySource(ActivitySourceName);

        public async ValueTask RecordAsync(List<IMetric> metrics, CancellationToken cancellationToken = default)
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

            // Positional rather than named: the name-first and name-last overloads are both
            // applicable once the arguments are named, and the call becomes ambiguous.
            Activity activity = activitySource.StartActivity(
                $"{metric.Method}/{metric.Name}",
                ToActivityKind(metric.Type),
                CreateTraceContext(metric.CorrelationId),
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
            activity.Stop();
        }

        /// <summary>
        /// Derives a trace id from the correlation id so every span of one request lands under a
        /// single operation in the telemetry viewer. Exact parent and child nesting is carried in
        /// the tags and reconstructed from the metrics table, which stays the authoritative store.
        /// </summary>
        private static ActivityContext CreateTraceContext(Guid correlationId)
        {
            if (correlationId == Guid.Empty)
            {
                return default;
            }

            byte[] correlationBytes = correlationId.ToByteArray();

            return new ActivityContext(
                traceId: ActivityTraceId.CreateFromBytes(correlationBytes),
                spanId: ActivitySpanId.CreateFromBytes(correlationBytes.AsSpan(start: 0, length: 8)),
                traceFlags: ActivityTraceFlags.Recorded);
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
