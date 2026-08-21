// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Metrics;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial interface IStorageBroker
    {
        ValueTask BulkInsertMetricsAsync(List<Metric> metrics);
        ValueTask BulkDeleteMetricsAsync(List<Metric> metrics);
        ValueTask<Metric> InsertMetricAsync(Metric metric);
        ValueTask<IQueryable<Metric>> SelectAllMetricsAsync();
        ValueTask<Metric> SelectMetricByIdAsync(Guid metricId);
        ValueTask<Metric> DeleteMetricAsync(Metric metric);
    }
}
