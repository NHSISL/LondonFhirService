// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Metrics;

namespace LondonFhirService.Core.Brokers.Metrics
{
    /// <summary>
    /// Emits completed metric spans to the telemetry pipeline. This is the second sink for the
    /// same spans the storage broker persists - one set of instrumentation call sites, two
    /// destinations, fanned out in a single place by the metric service. Recording is best
    /// effort and never affects the measured request.
    /// </summary>
    public interface IMetricBroker
    {
        ValueTask RecordAsync(Metric metric, CancellationToken cancellationToken = default);
        ValueTask RecordAsync(List<Metric> metrics, CancellationToken cancellationToken = default);
    }
}
