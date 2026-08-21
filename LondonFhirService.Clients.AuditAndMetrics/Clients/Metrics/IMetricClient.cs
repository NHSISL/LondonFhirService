// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Abstractions.Models.Metrics;

namespace LondonFhirService.Clients.AuditAndMetrics.Clients.Metrics
{
    /// <summary>
    /// The outward facing surface over the metric foundation service. Service exceptions are
    /// re-thrown as client exceptions so that callers depend on the client's own contract rather
    /// than on the service layer's.
    ///
    /// Cancellation is the one thing that is not translated. Every method checks the token before
    /// doing any work and lets an OperationCanceledException travel out untouched, so a caller
    /// that cancels gets the cancellation exception it expects rather than a client exception it
    /// has to unwrap.
    /// </summary>
    public interface IMetricClient
    {
        ValueTask<IMetric> AddMetricAsync(IMetric metric, CancellationToken cancellationToken = default);
        ValueTask AddMetricsAsync(List<IMetric> metrics, CancellationToken cancellationToken = default);
        ValueTask<IQueryable<IMetric>> RetrieveAllMetricsAsync(CancellationToken cancellationToken = default);
        ValueTask<IMetric> RetrieveMetricByIdAsync(Guid metricId, CancellationToken cancellationToken = default);
        ValueTask<IMetric> RemoveMetricByIdAsync(Guid metricId, CancellationToken cancellationToken = default);
        ValueTask<int> PurgeMetricsOlderThanRetentionPeriodAsync(CancellationToken cancellationToken = default);
    }
}
