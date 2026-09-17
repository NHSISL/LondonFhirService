// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.Identifiers;
using Microsoft.AspNetCore.Http;

namespace LondonFhirService.Core.Brokers.Correlations
{
    /// <summary>
    /// The one correlation id belonging to the request in flight, and the span id of the request
    /// itself. It used to be drawn inside the coordination service, which meant a request that
    /// failed before it got there - rejected by authentication, refused by authorization,
    /// abandoned on timeout - had no id at all, and the consumer had nothing to quote when they
    /// rang up about it.
    ///
    /// Where the id comes from is W3C Trace Context, not a header of our own invention. A caller
    /// or gateway that sends "traceparent" is already naming the trace this request belongs to;
    /// ASP.NET parses that header and hangs the trace id off Activity.Current before any of our
    /// code runs, and Application Insights correlates on the same value. Taking the correlation
    /// id from there joins the audit trail and the metrics table to the trace the telemetry
    /// already uses, instead of inventing a second id for the same request. A caller that sends
    /// nothing gets the trace id ASP.NET generated for them, which is equally valid - it is still
    /// the id every hop downstream will see.
    ///
    /// The trace id is carried as a Guid because that is what IMetric.CorrelationId and the
    /// indexed column behind it already are. The two are the same 128 bits and the same 32 hex
    /// characters: Guid.ToString("N") of a correlation id IS the W3C trace id, so an id read off
    /// a response header or an audit row can be pasted straight into the telemetry viewer.
    ///
    /// Both values are resolved together, once, and held as a SINGLE object - on HttpContext.Items
    /// for a request, and in a field of this scoped instance when there is no request behind the
    /// work. One object rather than two keys is what makes "both values or neither" structural:
    /// the earlier two-key form let a read of the span id re-resolve and overwrite an already
    /// settled correlation id, so the header promised one value and the audit rows recorded
    /// another. The field is what makes a background worker's repeated reads agree with each
    /// other; before, every call there minted a fresh id and split one operation across two traces.
    ///
    /// Resolution is expected to happen in CorrelationMiddleware, at the very front of the
    /// pipeline, where Activity.Current is still the request's own activity. Resolving later
    /// would work for the correlation id, which is trace wide, but could capture a child
    /// activity's span id rather than the request's.
    ///
    /// With no W3C activity to read, a fresh correlation id is drawn and there is no request span
    /// to name. Guid.Empty would validate away at the next layer and stamp every row written
    /// under it with the same meaningless value.
    /// </summary>
    public class CorrelationBroker : ICorrelationBroker
    {
        public const string CorrelationIdHeaderName = "X-Correlation-Id";

        internal const string RequestCorrelationItemKey = "LondonFhirService.RequestCorrelation";

        private const string EmptyTraceIdHexString = "00000000000000000000000000000000";
        private const string EmptySpanIdHexString = "0000000000000000";

        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IIdentifierBroker identifierBroker;

        /// <summary>
        /// Only ever used when there is no HttpContext to hang the answer off. This broker is
        /// registered scoped, so the field lives exactly as long as the operation does.
        /// </summary>
        private RequestCorrelation resolvedWithoutRequest;

        public CorrelationBroker(
            IHttpContextAccessor httpContextAccessor,
            IIdentifierBroker identifierBroker)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.identifierBroker = identifierBroker;
        }

        public async ValueTask<Guid> GetCorrelationIdAsync() =>
            (await ResolveAsync()).CorrelationId;

        public async ValueTask<string> GetRequestSpanIdAsync() =>
            (await ResolveAsync()).RequestSpanId;

        /// <summary>
        /// Resolves once and reuses the answer thereafter, whichever of the two accessors asks
        /// first. Nothing here overwrites a value that has already been settled.
        /// </summary>
        private async ValueTask<RequestCorrelation> ResolveAsync()
        {
            HttpContext httpContext = this.httpContextAccessor.HttpContext;

            if (httpContext is null)
            {
                return this.resolvedWithoutRequest ??= await CreateAsync(Activity.Current);
            }

            if (httpContext.Items.TryGetValue(RequestCorrelationItemKey, out object storedValue)
                && storedValue is RequestCorrelation storedCorrelation)
            {
                return storedCorrelation;
            }

            RequestCorrelation requestCorrelation = await CreateAsync(Activity.Current);
            httpContext.Items[RequestCorrelationItemKey] = requestCorrelation;

            return requestCorrelation;
        }

        private async ValueTask<RequestCorrelation> CreateAsync(Activity activity) =>
            new RequestCorrelation(
                CorrelationId: ReadTraceId(activity)
                    ?? await this.identifierBroker.GetIdentifierAsync(),
                RequestSpanId: ReadSpanId(activity));

        /// <summary>
        /// Null for anything that is not a usable W3C trace id, which sends the caller back to a
        /// freshly minted one. Activity.Current is null when nothing is listening for activities,
        /// and an activity created before Activity.DefaultIdFormat was W3C carries a hierarchical
        /// id with no trace id to read.
        ///
        /// Converted through the hex string rather than the raw bytes on purpose. Guid's byte
        /// order is little endian for its first three fields while a trace id's is not, so going
        /// through bytes would silently scramble the value; the hex form round trips exactly, in
        /// both directions, which is what lets MetricBroker rebuild the caller's trace id from a
        /// stored correlation id.
        /// </summary>
        internal static Guid? ReadTraceId(Activity activity)
        {
            if (activity is null || activity.IdFormat != ActivityIdFormat.W3C)
            {
                return null;
            }

            string traceIdHexString = activity.TraceId.ToHexString();

            if (string.IsNullOrEmpty(traceIdHexString)
                || traceIdHexString == EmptyTraceIdHexString)
            {
                return null;
            }

            return Guid.ParseExact(traceIdHexString, "N");
        }

        /// <summary>
        /// The activity's own span id, which is the request's when this is resolved from the
        /// front of the pipeline. Null rather than an all-zero span id, so a caller can tell
        /// "no request span" from one that happens to be zeroes - MetricBroker relies on that
        /// distinction to choose between anchoring and its derived fallback.
        /// </summary>
        internal static string ReadSpanId(Activity activity)
        {
            if (activity is null || activity.IdFormat != ActivityIdFormat.W3C)
            {
                return null;
            }

            string spanIdHexString = activity.SpanId.ToHexString();

            if (string.IsNullOrEmpty(spanIdHexString)
                || spanIdHexString == EmptySpanIdHexString)
            {
                return null;
            }

            return spanIdHexString;
        }

        /// <summary>
        /// The pair, kept together so the two can never describe different traces. Internal
        /// rather than private so the broker's tests can seed an already-settled request.
        /// </summary>
        internal sealed record RequestCorrelation(Guid CorrelationId, string RequestSpanId);
    }
}
