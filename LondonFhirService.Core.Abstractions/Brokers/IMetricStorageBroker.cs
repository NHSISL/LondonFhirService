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
    /// <summary>
    /// The metric persistence the audit and metrics library needs. See IAuditStorageBroker for
    /// why the port is declared here rather than consumed from the hosting application, and why
    /// audits and metrics are two ports rather than one.
    ///
    /// Everything is expressed in terms of IMetric; the library never sees the concrete entity or
    /// the ORM behind it.
    ///
    /// Implementations are also responsible for classifying storage failures, re-throwing the
    /// storage exceptions in Models.Metrics.Exceptions. Cancellation and timeout must pass
    /// through untranslated; the library handles those.
    /// </summary>
    public interface IMetricStorageBroker
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
