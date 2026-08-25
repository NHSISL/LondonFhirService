// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Core.Abstractions.Models.Metrics;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Models.Metrics
{
    /// <summary>
    /// Stands in for the concrete entity the hosting application supplies. The library holds no
    /// implementation of IMetric by design - that is what keeps the reference one way - so the
    /// tests bring their own.
    /// </summary>
    internal class TestMetric : IMetric
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
