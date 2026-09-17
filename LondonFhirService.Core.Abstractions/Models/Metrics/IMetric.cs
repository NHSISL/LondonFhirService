// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;

namespace LondonFhirService.Core.Abstractions.Models.Metrics
{
    /// <summary>
    /// The contract this library works against. The concrete entity lives in the consuming
    /// application and derives from this, so the library never needs a reference back to it -
    /// which is what keeps the dependency one way and the reference acyclic.
    /// </summary>
    public interface IMetric
    {
        Guid Id { get; set; }

        /// <summary>
        /// Who the request that produced this span belonged to, or empty when it was produced
        /// outside a user request - a background worker or the retention sweep.
        ///
        /// This is an opaque account identifier, not a name, an email or anything else that
        /// identifies a patient. It is here so a slow or failing span can be attributed to the
        /// caller who triggered it without joining back to the audit trail first.
        /// </summary>
        string UserId { get; set; }

        Guid? ParentId { get; set; }
        Guid CorrelationId { get; set; }

        /// <summary>
        /// The span id of the HTTP request this span belongs to, as 16 hex characters, or null
        /// when there was no request behind it. It anchors the replayed telemetry span under that
        /// request instead of leaving it floating beside it.
        ///
        /// Transport only - deliberately NOT persisted. The metrics table is the authoritative
        /// store and this adds nothing to it; the value exists to survive the hop from the request
        /// that produced the span to the background worker that replays it.
        ///
        /// Stamped by the metric service when the caller leaves it null. A caller that is already
        /// running deferred - the Persist span is queued before it is recorded - must set it
        /// itself, while the request is still alive, because by the time the service sees it there
        /// is no request left to ask.
        /// </summary>
        string RequestSpanId { get; set; }
        string Method { get; set; }
        MetricType Type { get; set; }
        string Name { get; set; }
        string Target { get; set; }
        DateTimeOffset Started { get; set; }
        DateTimeOffset Completed { get; set; }
        double DurationMs { get; set; }
        MetricStatus Status { get; set; }
        string ErrorCode { get; set; }
        long? PayloadBytes { get; set; }
        string Consumer { get; set; }

        /// <summary>
        /// Free text for a human reading a dashboard - what this span was actually doing, in
        /// words the structured columns cannot carry.
        ///
        /// It must never contain patient identifiable data. Metrics are written on every span,
        /// surfaced on dashboards and purged on a retention timer sized for capacity rather than
        /// for information governance. Anything that identifies a patient belongs in an audit
        /// entry, which is retained and controlled for exactly that purpose.
        /// </summary>
        string Description { get; set; }
        DateTimeOffset CreatedDate { get; set; }
    }
}
