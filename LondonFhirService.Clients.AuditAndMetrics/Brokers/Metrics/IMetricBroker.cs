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
        /// <summary>
        /// Each metric carries its own RequestSpanId, which anchors the replayed span under the
        /// HTTP request it belongs to. It travels on the metric rather than as a parameter because
        /// the replay runs on a background worker long after that request has gone, and because a
        /// batch is not guaranteed to belong to one request - a single parameter would stamp one
        /// request's span id onto spans from another trace.
        /// </summary>
        ValueTask RecordAsync(IMetric metric, CancellationToken cancellationToken = default);
        ValueTask RecordAsync(List<IMetric> metrics, CancellationToken cancellationToken = default);
    }
}
