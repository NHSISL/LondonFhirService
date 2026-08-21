// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Metrics;
using Microsoft.EntityFrameworkCore;
using IMetric = LondonFhirService.Core.Abstractions.Models.Metrics.IMetric;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial class StorageBroker
    {
        public DbSet<Metric> Metrics { get; set; }

        public virtual async ValueTask BulkInsertMetricsAsync(
            List<IMetric> metrics,
            CancellationToken cancellationToken = default) =>
            await BulkInsertAsync(metrics.Cast<Metric>().ToList(), cancellationToken);

        public virtual async ValueTask<IMetric> InsertMetricAsync(
            IMetric metric,
            CancellationToken cancellationToken = default) =>
            await InsertAsync((Metric)metric, cancellationToken);

        public virtual async ValueTask<IQueryable<IMetric>> SelectAllMetricsAsync(
            CancellationToken cancellationToken = default) =>
            (await SelectAllAsync<Metric>(cancellationToken)).Cast<IMetric>();

        public virtual async ValueTask<IMetric> SelectMetricByIdAsync(
            Guid metricId,
            CancellationToken cancellationToken = default) =>
            await SelectAsync<Metric>(new object[] { metricId }, cancellationToken);

        public virtual async ValueTask<IMetric> DeleteMetricAsync(
            IMetric metric,
            CancellationToken cancellationToken = default) =>
            await DeleteAsync((Metric)metric, cancellationToken);

        /// <summary>
        /// The predicate runs in SQL and no entity is materialised, so the cost of a purge does
        /// not grow with the size of the retention window.
        /// </summary>
        public virtual async ValueTask<int> DeleteMetricsOlderThanAsync(
            DateTimeOffset cutOffDate,
            int batchSize,
            CancellationToken cancellationToken = default) =>
            await Metrics
                .Where(metric => metric.CreatedDate < cutOffDate)
                .OrderBy(metric => metric.CreatedDate)
                .Take(batchSize)
                .ExecuteDeleteAsync(cancellationToken);
    }
}
