// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Metrics;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial interface IStorageBroker
    {
        ValueTask BulkInsertMetricsAsync(List<Metric> metrics, CancellationToken cancellationToken = default);
        /// <summary>
        /// Deletes at most <paramref name="batchSize"/> metrics created before
        /// <paramref name="cutOffDate"/> and returns how many rows the database removed.
        /// The predicate runs in SQL and no entity is materialised, so the cost does not
        /// grow with the size of the retention window.
        /// </summary>
        ValueTask<int> DeleteMetricsOlderThanAsync(
            DateTimeOffset cutOffDate,
            int batchSize,
            CancellationToken cancellationToken = default);
        ValueTask<Metric> InsertMetricAsync(Metric metric, CancellationToken cancellationToken = default);
        ValueTask<IQueryable<Metric>> SelectAllMetricsAsync(CancellationToken cancellationToken = default);
        ValueTask<Metric> SelectMetricByIdAsync(Guid metricId, CancellationToken cancellationToken = default);
        ValueTask<Metric> DeleteMetricAsync(Metric metric, CancellationToken cancellationToken = default);
    }
}
