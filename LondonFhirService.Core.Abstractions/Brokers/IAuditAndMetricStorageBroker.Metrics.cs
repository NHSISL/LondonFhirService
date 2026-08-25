// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Abstractions.Models.Metrics;

namespace LondonFhirService.Core.Abstractions.Brokers
{
    public partial interface IAuditAndMetricStorageBroker
    {
        ValueTask<IMetric> InsertMetricAsync(IMetric metric, CancellationToken cancellationToken = default);
        ValueTask BulkInsertMetricsAsync(List<IMetric> metrics, CancellationToken cancellationToken = default);
        ValueTask<IQueryable<IMetric>> SelectAllMetricsAsync(CancellationToken cancellationToken = default);
        ValueTask<IMetric> SelectMetricByIdAsync(Guid metricId, CancellationToken cancellationToken = default);
        ValueTask<IMetric> DeleteMetricAsync(IMetric metric, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes at most <paramref name="batchSize"/> metrics created before
        /// <paramref name="cutOffDate"/> and returns how many rows were removed. Expected to run
        /// the predicate in the data store without materialising the candidates.
        /// </summary>
        ValueTask<int> DeleteMetricsOlderThanAsync(
            DateTimeOffset cutOffDate,
            int batchSize,
            CancellationToken cancellationToken = default);
    }
}
