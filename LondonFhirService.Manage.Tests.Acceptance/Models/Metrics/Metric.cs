// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;

namespace LondonFhirService.Manage.Tests.Acceptance.Models.Metrics
{
    /// <summary>
    /// Type and Status are strings rather than the enums Core uses. The broker persists both as
    /// text so the reporting queries stay readable, and the acceptance suite talks to the wire
    /// format rather than to Core's types.
    /// </summary>
    public class Metric
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public Guid CorrelationId { get; set; }
        public string Method { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Target { get; set; }
        public DateTimeOffset Started { get; set; }
        public DateTimeOffset Completed { get; set; }
        public double DurationMs { get; set; }
        public string Status { get; set; }
        public string ErrorCode { get; set; }
        public long? PayloadBytes { get; set; }
        public string Consumer { get; set; }
        public string Description { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
