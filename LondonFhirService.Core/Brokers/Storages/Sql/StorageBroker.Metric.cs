// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Metrics;
using Microsoft.EntityFrameworkCore;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial class StorageBroker
    {
        public DbSet<Metric> Metrics { get; set; }

        public virtual async ValueTask BulkInsertMetricsAsync(List<Metric> metrics) =>
            await BulkInsertAsync(metrics);

        public virtual async ValueTask<Metric> InsertMetricAsync(Metric metric) =>
            await InsertAsync(metric);

        public virtual async ValueTask<IQueryable<Metric>> SelectAllMetricsAsync() =>
            await SelectAllAsync<Metric>();

        public virtual async ValueTask<Metric> SelectMetricByIdAsync(Guid metricId) =>
            await SelectAsync<Metric>(metricId);

        public virtual async ValueTask<Metric> DeleteMetricAsync(Metric metric) =>
            await DeleteAsync(metric);
    }
}
