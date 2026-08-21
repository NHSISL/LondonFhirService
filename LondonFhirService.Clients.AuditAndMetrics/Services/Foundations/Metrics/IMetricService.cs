// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Abstractions.Models.Metrics;

using LondonFhirService.Core.Abstractions.Brokers;

using LondonFhirService.Clients.AuditAndMetrics.Models.Metrics.Exceptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Services.Foundations.Metrics
{
    internal interface IMetricService
    {
        /// <summary>
        /// Records a single span to storage and to telemetry. A no-op returning the input
        /// unchanged when recording is disabled by configuration.
        /// </summary>
        ValueTask<IMetric> AddMetricAsync(IMetric metric, CancellationToken cancellationToken = default);

        /// <summary>
        /// Records a batch of spans in one round trip. This is the flush the instrumented code
        /// uses at the end of a request, so that no storage write happens inside the work being
        /// measured. A no-op when recording is disabled by configuration.
        /// </summary>
        ValueTask AddMetricsAsync(List<IMetric> metrics, CancellationToken cancellationToken = default);

        ValueTask<IQueryable<IMetric>> RetrieveAllMetricsAsync(CancellationToken cancellationToken = default);
        ValueTask<IMetric> RetrieveMetricByIdAsync(Guid metricId, CancellationToken cancellationToken = default);
        ValueTask<IMetric> RemoveMetricByIdAsync(Guid metricId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes every metric older than the configured retention period and returns how many
        /// rows the database removed. The delete runs in SQL in bounded batches, so nothing is
        /// materialised and the count is the real affected-row total rather than a count of
        /// candidates. Returns zero without deleting when purging is not allowed by
        /// configuration.
        /// </summary>
        ValueTask<int> PurgeMetricsOlderThanRetentionPeriodAsync(CancellationToken cancellationToken = default);
    }
}
