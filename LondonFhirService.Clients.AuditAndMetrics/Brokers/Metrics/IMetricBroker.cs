// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Abstractions.Models.Metrics;

namespace LondonFhirService.Clients.AuditAndMetrics.Brokers.Metrics
{
    /// <summary>
    /// Emits completed metric spans to the telemetry pipeline. This is the second sink for the
    /// same spans the storage broker persists - one set of instrumentation call sites, two
    /// destinations, fanned out in a single place by the metric service. Recording is best
    /// effort and never affects the measured request.
    /// </summary>
    internal interface IMetricBroker
    {
        ValueTask RecordAsync(IMetric metric, CancellationToken cancellationToken = default);
        ValueTask RecordAsync(List<IMetric> metrics, CancellationToken cancellationToken = default);
    }
}
