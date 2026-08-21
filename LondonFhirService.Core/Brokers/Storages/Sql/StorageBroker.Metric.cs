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

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial class StorageBroker
    {
        public DbSet<Metric> Metrics { get; set; }

        public virtual async ValueTask BulkInsertMetricsAsync(
            List<Metric> metrics,
            CancellationToken cancellationToken = default) =>
            await BulkInsertAsync(metrics, cancellationToken);

        public virtual async ValueTask BulkDeleteMetricsAsync(
            List<Metric> metrics,
            CancellationToken cancellationToken = default) =>
            await BulkDeleteAsync(metrics, cancellationToken);

        public virtual async ValueTask<Metric> InsertMetricAsync(
            Metric metric,
            CancellationToken cancellationToken = default) =>
            await InsertAsync(metric, cancellationToken);

        public virtual async ValueTask<IQueryable<Metric>> SelectAllMetricsAsync(
            CancellationToken cancellationToken = default) =>
            await SelectAllAsync<Metric>(cancellationToken);

        public virtual async ValueTask<Metric> SelectMetricByIdAsync(
            Guid metricId,
            CancellationToken cancellationToken = default) =>
            await SelectAsync<Metric>(cancellationToken, metricId);

        public virtual async ValueTask<Metric> DeleteMetricAsync(
            Metric metric,
            CancellationToken cancellationToken = default) =>
            await DeleteAsync(metric, cancellationToken);
    }
}
