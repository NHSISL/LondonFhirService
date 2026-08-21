// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Metrics;

namespace LondonFhirService.Core.Services.Foundations.Metrics
{
    public interface IMetricService
    {
        /// <summary>
        /// Records a single span to storage and to telemetry. A no-op returning the input
        /// unchanged when recording is disabled by configuration.
        /// </summary>
        ValueTask<Metric> AddMetricAsync(Metric metric, CancellationToken cancellationToken = default);

        /// <summary>
        /// Records a batch of spans in one round trip. This is the flush the instrumented code
        /// uses at the end of a request, so that no storage write happens inside the work being
        /// measured. A no-op when recording is disabled by configuration.
        /// </summary>
        ValueTask AddMetricsAsync(List<Metric> metrics, CancellationToken cancellationToken = default);

        ValueTask<IQueryable<Metric>> RetrieveAllMetricsAsync(CancellationToken cancellationToken = default);
        ValueTask<Metric> RetrieveMetricByIdAsync(Guid metricId, CancellationToken cancellationToken = default);
        ValueTask<Metric> RemoveMetricByIdAsync(Guid metricId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes every metric older than the configured retention period and returns how many
        /// were selected for deletion. That is a count of candidates, not of rows the database
        /// confirmed it removed - the bulk delete reports no affected-row count, so if two purges
        /// run at once the second can select rows the first has already taken. Returns zero
        /// without deleting when purging is not allowed by configuration.
        /// </summary>
        ValueTask<int> PurgeMetricsOlderThanRetentionPeriodAsync(CancellationToken cancellationToken = default);
    }
}
