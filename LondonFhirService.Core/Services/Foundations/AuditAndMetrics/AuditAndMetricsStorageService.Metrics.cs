// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Core.Brokers.Storages.Sql;

namespace LondonFhirService.Core.Services.Foundations.AuditAndMetrics
{
    internal partial class AuditAndMetricsStorageService
    {
        public ValueTask<IMetric> InsertMetricAsync(
            IMetric metric,
            CancellationToken cancellationToken = default) =>
            TryCatchMetricAsync(async () =>
            {
                await using IStorageBroker broker = await this.storageBrokerFactory.CreateStorageBrokerAsync();

                return await broker.InsertMetricAsync(metric, cancellationToken);
            });

        public ValueTask BulkInsertMetricsAsync(
            List<IMetric> metrics,
            CancellationToken cancellationToken = default) =>
            TryCatchMetricAsync(async () =>
            {
                await using IStorageBroker broker = await this.storageBrokerFactory.CreateStorageBrokerAsync();
                await broker.BulkInsertMetricsAsync(metrics, cancellationToken);
            });

        public ValueTask<IQueryable<IMetric>> SelectAllMetricsAsync(
            CancellationToken cancellationToken = default) =>
            TryCatchMetricAsync(async () =>
                await this.storageBroker.SelectAllMetricsAsync(cancellationToken));

        public ValueTask<IMetric> SelectMetricByIdAsync(
            Guid metricId,
            CancellationToken cancellationToken = default) =>
            TryCatchMetricAsync(async () =>
                await this.storageBroker.SelectMetricByIdAsync(metricId, cancellationToken));

        public ValueTask<IMetric> DeleteMetricAsync(
            IMetric metric,
            CancellationToken cancellationToken = default) =>
            TryCatchMetricAsync(async () =>
            {
                await using IStorageBroker broker = await this.storageBrokerFactory.CreateStorageBrokerAsync();

                return await broker.DeleteMetricAsync(metric, cancellationToken);
            });

        public ValueTask<int> DeleteMetricsOlderThanAsync(
            DateTimeOffset cutOffDate,
            int batchSize,
            CancellationToken cancellationToken = default) =>
            TryCatchMetricAsync(async () =>
            {
                await using IStorageBroker broker = await this.storageBrokerFactory.CreateStorageBrokerAsync();

                return await broker.DeleteMetricsOlderThanAsync(cutOffDate, batchSize, cancellationToken);
            });
    }
}
