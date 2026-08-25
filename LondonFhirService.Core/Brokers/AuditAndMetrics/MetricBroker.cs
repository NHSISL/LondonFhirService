// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Abstractions.Brokers;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.Metrics;

namespace LondonFhirService.Core.Brokers.AuditAndMetrics
{
    /// <summary>
    /// Satisfies the metric storage port the audit and metrics library declares. See
    /// AuditBroker for why a port adapter is a broker rather than a foundation service,
    /// why it classifies storage failures, why writes take a context from the factory while reads
    /// keep the scoped broker, and why the two entities are no longer served by one class.
    /// </summary>
    public partial class MetricBroker : IMetricBroker
    {
        private readonly IStorageBrokerFactory storageBrokerFactory;
        private readonly IStorageBroker storageBroker;

        public MetricBroker(
            IStorageBrokerFactory storageBrokerFactory,
            IStorageBroker storageBroker)
        {
            this.storageBrokerFactory = storageBrokerFactory;
            this.storageBroker = storageBroker;
        }

        /// <summary>
        /// See AuditBroker.AsAuditEntity: the port accepts any IMetric, storage only
        /// knows the Metric entity, and a cast would throw on a legitimate call.
        /// </summary>
        private static Metric AsMetricEntity(IMetric metric)
        {
            if (metric is Metric metricEntity)
            {
                return metricEntity;
            }

            return new Metric
            {
                Id = metric.Id,
                ParentId = metric.ParentId,
                CorrelationId = metric.CorrelationId,
                Method = metric.Method,
                Type = metric.Type,
                Name = metric.Name,
                Target = metric.Target,
                Started = metric.Started,
                Completed = metric.Completed,
                DurationMs = metric.DurationMs,
                Status = metric.Status,
                ErrorCode = metric.ErrorCode,
                PayloadBytes = metric.PayloadBytes,
                Consumer = metric.Consumer,
                Description = metric.Description,
                CreatedDate = metric.CreatedDate
            };
        }

        public ValueTask<IMetric> InsertMetricAsync(
            IMetric metric,
            CancellationToken cancellationToken = default) =>
            TryCatchAsync(async () =>
            {
                await using IStorageBroker broker = await this.storageBrokerFactory.CreateStorageBrokerAsync();

                return await broker.InsertMetricAsync(AsMetricEntity(metric), cancellationToken);
            });

        public ValueTask BulkInsertMetricsAsync(
            List<IMetric> metrics,
            CancellationToken cancellationToken = default) =>
            TryCatchAsync(async () =>
            {
                await using IStorageBroker broker = await this.storageBrokerFactory.CreateStorageBrokerAsync();
                await broker.BulkInsertMetricsAsync(
                    metrics.Select(AsMetricEntity).Cast<IMetric>().ToList(),
                    cancellationToken);
            });

        // Reads keep the scoped broker: they hand back an IQueryable the caller enumerates, and a
        // disposed context underneath it would kill the query.
        public ValueTask<IQueryable<IMetric>> SelectAllMetricsAsync(
            CancellationToken cancellationToken = default) =>
            TryCatchAsync(async () =>
                await this.storageBroker.SelectAllMetricsAsync(cancellationToken));

        public ValueTask<IMetric> SelectMetricByIdAsync(
            Guid metricId,
            CancellationToken cancellationToken = default) =>
            TryCatchAsync(async () =>
                await this.storageBroker.SelectMetricByIdAsync(metricId, cancellationToken));

        public ValueTask<IMetric> DeleteMetricAsync(
            IMetric metric,
            CancellationToken cancellationToken = default) =>
            TryCatchAsync(async () =>
            {
                await using IStorageBroker broker = await this.storageBrokerFactory.CreateStorageBrokerAsync();

                return await broker.DeleteMetricAsync(AsMetricEntity(metric), cancellationToken);
            });

        public ValueTask<int> DeleteMetricsOlderThanAsync(
            DateTimeOffset cutOffDate,
            int batchSize,
            CancellationToken cancellationToken = default) =>
            TryCatchAsync(async () =>
            {
                await using IStorageBroker broker = await this.storageBrokerFactory.CreateStorageBrokerAsync();

                return await broker.DeleteMetricsOlderThanAsync(cutOffDate, batchSize, cancellationToken);
            });
    }
}
