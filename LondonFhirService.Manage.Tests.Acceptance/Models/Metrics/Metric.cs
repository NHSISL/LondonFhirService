// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Core.Abstractions.Models.Metrics;

namespace LondonFhirService.Manage.Tests.Acceptance.Models.Metrics
{
    /// <summary>
    /// Type and Status are the real enums, not strings. EF persists them as text so the reporting
    /// queries stay readable, but that is the database's representation - the host registers no
    /// JsonStringEnumConverter, so on the wire they are ordinals. Sending "Provider" here failed
    /// model binding and every seeded test with it.
    /// </summary>
    public class Metric
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public Guid CorrelationId { get; set; }
        public string Method { get; set; }
        public MetricType Type { get; set; }
        public string Name { get; set; }
        public string Target { get; set; }
        public DateTimeOffset Started { get; set; }
        public DateTimeOffset Completed { get; set; }
        public double DurationMs { get; set; }
        public MetricStatus Status { get; set; }
        public string ErrorCode { get; set; }
        public long? PayloadBytes { get; set; }
        public string Consumer { get; set; }
        public string Description { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
