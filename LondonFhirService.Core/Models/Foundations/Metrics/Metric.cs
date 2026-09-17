// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Text.Json.Serialization;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Core.Abstractions.Models;

namespace LondonFhirService.Core.Models.Foundations.Metrics
{
    /// <summary>
    /// A single measured span of work. Spans sharing a <see cref="CorrelationId"/> form one
    /// request; <see cref="ParentId"/> links them into a tree whose root is the span with no
    /// parent.
    ///
    /// This model must never carry patient identifiable data - no NHS number, date of birth or
    /// any other patient identifier. The correlation id is the join key back to the audit trail
    /// when that detail is needed. Keeping this table free of PII is what allows it to be
    /// retained, aggregated and reported on independently of the audit retention rules.
    /// </summary>
    public class Metric : IKey, IMetric
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Who the request that produced this span belonged to, or empty when it was produced
        /// outside a user request - a background worker or the retention sweep.
        ///
        /// An opaque account identifier only. Like every other column here it must never carry
        /// patient identifiable data.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>The enclosing span, or null for the root span of a request.</summary>
        public Guid? ParentId { get; set; }

        /// <summary>Ties every span of a single request together.</summary>
        public Guid CorrelationId { get; set; }

        /// <summary>
        /// The span id of the HTTP request this span belongs to. Transport only and not mapped to
        /// a column - see IMetric.RequestSpanId. It rides from the request that produced the span
        /// to the background worker that replays it into telemetry, and is not part of the record.
        ///
        /// JsonIgnore because Metric is also the entity the Manage host exposes, and a field that
        /// is neither stored nor meaningful to a caller should not appear on that API in either
        /// direction. Without it the convention binder would accept a client-supplied value on
        /// POST and echo it back in the response, while a later GET returned null - a field the
        /// API appears to accept and then silently drops. The transport this property exists for
        /// is in-process: the metric client passes IMetric objects, never JSON, so nothing that
        /// needs the value goes through a serializer.
        /// </summary>
        [JsonIgnore]
        public string RequestSpanId { get; set; }

        /// <summary>
        /// The operation being measured, matching the audit type string used for the same
        /// operation so that metrics and audit rows line up - for example
        /// "STU3-Patient-GetStructuredRecordSerialised". The FHIR version is part of this
        /// string, so STU3 and R4 timings never merge.
        /// </summary>
        public string Method { get; set; }

        public MetricType Type { get; set; }

        /// <summary>The display name of what was measured, such as a provider friendly name.</summary>
        public string Name { get; set; }

        /// <summary>
        /// The stable identifier behind <see cref="Name"/>, such as a provider's fully qualified
        /// name. Survives a rename, so historic trends stay joined up.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Wall clock start. Derived from a single monotonic timestamp taken once per request,
        /// so sibling spans are directly comparable and never appear to start before their parent.
        /// </summary>
        public DateTimeOffset Started { get; set; }

        /// <summary>Always <see cref="Started"/> plus <see cref="DurationMs"/>, by construction.</summary>
        public DateTimeOffset Completed { get; set; }

        /// <summary>
        /// Elapsed milliseconds, measured with <see cref="System.Diagnostics.Stopwatch"/> rather
        /// than by subtracting two clock readings. Held as a double because the faster spans are
        /// sub-millisecond and would otherwise record as zero.
        /// </summary>
        public double DurationMs { get; set; }

        public MetricStatus Status { get; set; }

        /// <summary>A short classification of a failure, never an exception message.</summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Size of the payload the span produced. Provider durations are not comparable without
        /// it - a provider returning a large bundle slowly is not necessarily the slower provider.
        /// </summary>
        public long? PayloadBytes { get; set; }

        /// <summary>
        /// The calling consumer, where one was resolved. Lets a slow provider be attributed to a
        /// particular consumer's request pattern rather than the provider itself.
        /// </summary>
        public string Consumer { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// When the row was written. Deliberately separate from <see cref="Started"/>, which is
        /// when the work began, and indexed to support the retention sweep.
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; }
    }
}
