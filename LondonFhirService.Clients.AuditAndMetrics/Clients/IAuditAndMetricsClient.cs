// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Clients.AuditAndMetrics.Clients.Audits;
using LondonFhirService.Clients.AuditAndMetrics.Clients.Metrics;

namespace LondonFhirService.Clients.AuditAndMetrics.Clients
{
    /// <summary>
    /// The library's entry point. Public rather than internal - a consuming application has to be
    /// able to name this type to register and resolve it.
    /// </summary>
    public interface IAuditAndMetricsClient
    {
        IAuditClient AuditClient { get; }
        IMetricClient MetricClient { get; }
    }
}
