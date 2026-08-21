// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Metrics;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.Metrics;

namespace LondonFhirService.Core.Services.Foundations.Metrics
{
    public partial class MetricService : IMetricService
    {
        private readonly IStorageBroker storageBroker;
        private readonly IMetricBroker metricBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ILoggingBroker loggingBroker;
        private readonly MetricServiceConfigurations metricServiceConfigurations;

        public MetricService(
            IStorageBroker storageBroker,
            IMetricBroker metricBroker,
            IDateTimeBroker dateTimeBroker,
            ILoggingBroker loggingBroker,
            MetricServiceConfigurations metricServiceConfigurations)
        {
            this.storageBroker = storageBroker;
            this.metricBroker = metricBroker;
            this.dateTimeBroker = dateTimeBroker;
            this.loggingBroker = loggingBroker;
            this.metricServiceConfigurations = metricServiceConfigurations;
        }

        public ValueTask<Metric> AddMetricAsync(Metric metric, CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            if (this.metricServiceConfigurations.IsEnabled is false)
            {
                return metric;
            }

            cancellationToken.ThrowIfCancellationRequested();
            ValidateMetricIsNotNull(metric);
            metric.CreatedDate = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();
            ValidateMetricOnAdd(metric);
            Metric addedMetric = await this.storageBroker.InsertMetricAsync(metric, cancellationToken);
            await this.metricBroker.RecordAsync(addedMetric, cancellationToken);

            return addedMetric;
        });

        public ValueTask AddMetricsAsync(List<Metric> metrics, CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            if (this.metricServiceConfigurations.IsEnabled is false)
            {
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            ValidateMetricsIsNotNull(metrics);

            if (metrics.Count == 0)
            {
                return;
            }

            DateTimeOffset currentDateTime = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

            foreach (Metric metric in metrics)
            {
                ValidateMetricIsNotNull(metric);
                metric.CreatedDate = currentDateTime;
                ValidateMetricOnAdd(metric);
            }

            await this.storageBroker.BulkInsertMetricsAsync(metrics, cancellationToken);
            await this.metricBroker.RecordAsync(metrics, cancellationToken);
        });

        public ValueTask<IQueryable<Metric>> RetrieveAllMetricsAsync(CancellationToken cancellationToken = default) =>
            TryCatch(async () => await this.storageBroker.SelectAllMetricsAsync(cancellationToken));

        public ValueTask<Metric> RetrieveMetricByIdAsync(
            Guid metricId,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            ValidateMetricId(metricId);
            Metric maybeMetric = await this.storageBroker.SelectMetricByIdAsync(metricId, cancellationToken);
            ValidateStorageMetric(maybeMetric, metricId);

            return maybeMetric;
        });

        public ValueTask<Metric> RemoveMetricByIdAsync(
            Guid metricId,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            ValidateMetricId(metricId);
            Metric maybeMetric = await this.storageBroker.SelectMetricByIdAsync(metricId, cancellationToken);
            ValidateStorageMetric(maybeMetric, metricId);

            return await this.storageBroker.DeleteMetricAsync(maybeMetric, cancellationToken);
        });

        public ValueTask<int> PurgeMetricsOlderThanRetentionPeriodAsync(
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            if (this.metricServiceConfigurations.IsPurgingAllowed is false)
            {
                return 0;
            }

            // Guarded rather than obeyed. A zero or negative retention period would make the
            // cut off date the present or the future and delete the entire table.
            ValidateRetentionPeriod(this.metricServiceConfigurations.RetentionPeriodInDays);

            DateTimeOffset currentDateTime = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

            DateTimeOffset cutOffDate =
                currentDateTime.AddDays(-this.metricServiceConfigurations.RetentionPeriodInDays);

            IQueryable<Metric> allMetrics = await this.storageBroker.SelectAllMetricsAsync(cancellationToken);

            List<Metric> expiredMetrics = allMetrics
                .Where(metric => metric.CreatedDate < cutOffDate)
                .ToList();

            if (expiredMetrics.Count == 0)
            {
                return 0;
            }

            await this.storageBroker.BulkDeleteMetricsAsync(expiredMetrics, cancellationToken);

            await this.loggingBroker.LogInformationAsync(
                $"Purged {expiredMetrics.Count} metric(s) created before {cutOffDate}.");

            return expiredMetrics.Count;
        });
    }
}
