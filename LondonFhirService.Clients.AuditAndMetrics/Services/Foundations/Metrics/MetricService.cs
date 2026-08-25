// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Clients.AuditAndMetrics.Brokers.DateTimes;
using LondonFhirService.Clients.AuditAndMetrics.Brokers.Loggings;
using LondonFhirService.Clients.AuditAndMetrics.Brokers.Metrics;
using LondonFhirService.Core.Abstractions.Brokers;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Clients.AuditAndMetrics.Models.Configurations;

using LondonFhirService.Clients.AuditAndMetrics.Models.Metrics.Exceptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Services.Foundations.Metrics
{
    internal partial class MetricService : IMetricService
    {
        private readonly IAuditAndMetricStorageBroker storageBroker;
        private readonly IMetricBroker metricBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ILoggingBroker loggingBroker;
        private readonly AuditAndMetricsConfigurations metricServiceConfigurations;
        private readonly IAuditAndMetricsDispatcher dispatcher;

        public MetricService(
            IAuditAndMetricStorageBroker storageBroker,
            IMetricBroker metricBroker,
            IDateTimeBroker dateTimeBroker,
            ILoggingBroker loggingBroker,
            AuditAndMetricsConfigurations metricServiceConfigurations,
            IAuditAndMetricsDispatcher dispatcher)
        {
            this.storageBroker = storageBroker;
            this.metricBroker = metricBroker;
            this.dateTimeBroker = dateTimeBroker;
            this.loggingBroker = loggingBroker;
            this.metricServiceConfigurations = metricServiceConfigurations;
            this.dispatcher = dispatcher;
        }

        public ValueTask<IMetric> AddMetricAsync(IMetric metric, CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (this.metricServiceConfigurations.IsEnabled is false)
            {
                return metric;
            }

            ValidateMetricIsNotNull(metric);
            metric.CreatedDate = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();
            ValidateMetricOnAdd(metric);
            IMetric addedMetric = await this.storageBroker.InsertMetricAsync(metric, cancellationToken);
            await this.metricBroker.RecordAsync(addedMetric, cancellationToken);

            return addedMetric;
        });

        public ValueTask AddMetricsAsync(List<IMetric> metrics, CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (this.metricServiceConfigurations.IsEnabled is false)
            {
                return;
            }

            ValidateMetricsIsNotNull(metrics);

            if (metrics.Count == 0)
            {
                return;
            }

            DateTimeOffset currentDateTime = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

            foreach (IMetric metric in metrics)
            {
                ValidateMetricIsNotNull(metric);
                metric.CreatedDate = currentDateTime;
                ValidateMetricOnAdd(metric);
            }

            await this.storageBroker.BulkInsertMetricsAsync(metrics, cancellationToken);
            await this.metricBroker.RecordAsync(metrics, cancellationToken);
        });

        public ValueTask LogMetricAsync(IMetric metric, CancellationToken cancellationToken = default) =>
        SwallowAsync(() => TryCatch(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (this.metricServiceConfigurations.IsEnabled is false)
            {
                return;
            }

            ValidateMetricIsNotNull(metric);
            metric.CreatedDate = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();
            ValidateMetricOnAdd(metric);

            Dispatch(async token =>
            {
                IMetric addedMetric = await this.storageBroker.InsertMetricAsync(metric, token);
                await this.metricBroker.RecordAsync(addedMetric, token);
            });
        }));

        public ValueTask LogMetricsAsync(
            List<IMetric> metrics,
            CancellationToken cancellationToken = default) =>
        SwallowAsync(() => TryCatch(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (this.metricServiceConfigurations.IsEnabled is false)
            {
                return;
            }

            ValidateMetricsIsNotNull(metrics);

            if (metrics.Count == 0)
            {
                return;
            }

            DateTimeOffset currentDateTime = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

            foreach (IMetric metric in metrics)
            {
                ValidateMetricIsNotNull(metric);
                metric.CreatedDate = currentDateTime;
                ValidateMetricOnAdd(metric);
            }

            // Snapshotted, so a caller that keeps mutating its list after handing it over cannot
            // change what the deferred write persists. The audit bulk path already does this.
            List<IMetric> snapshot = metrics.ToList();

            Dispatch(async token =>
            {
                await this.storageBroker.BulkInsertMetricsAsync(snapshot, token);
                await this.metricBroker.RecordAsync(snapshot, token);
            });
        }));

        /// <summary>
        /// A dispatched write must never fail the work it is measuring. These spans and entries
        /// are recorded from inside the very request they describe, so a malformed one, or
        /// storage being down, has to cost that request nothing.
        ///
        /// The broker used to provide this by wrapping the whole call in Task.Run with a catch.
        /// Moving the deferral into this service removed it accidentally: validation now runs on
        /// the caller thread, and without this its exception would surface in the patient request
        /// instead of in a log line.
        /// </summary>
        private async ValueTask SwallowAsync(Func<ValueTask> work)
        {
            try
            {
                await work();
            }
            catch (OperationCanceledException)
            {
                // Expected. The caller gave up, or the host is stopping.
            }
            catch (Exception)
            {
                // Swallowed, not logged. TryCatch has already localised this and written it to
                // the logging broker on the way past; logging it again here would put two lines
                // in the log for one failure and double the noise of a storage outage.
            }
        }

        /// <summary>
        /// See AuditService.Dispatch - a rejected write is logged, never thrown.
        /// </summary>
        private void Dispatch(Func<CancellationToken, ValueTask> work)
        {
            if (this.dispatcher.TryDispatch(work) is false)
            {
                _ = this.loggingBroker.LogWarningAsync(
                    "A metric write was dropped because the dispatch queue was full. The span is "
                        + "lost; the request it belongs to was not affected.");
            }
        }

        public ValueTask<IQueryable<IMetric>> RetrieveAllMetricsAsync(CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await this.storageBroker.SelectAllMetricsAsync(cancellationToken);
        });

        public ValueTask<IMetric> RetrieveMetricByIdAsync(
            Guid metricId,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidateMetricId(metricId);
            IMetric maybeMetric = await this.storageBroker.SelectMetricByIdAsync(metricId, cancellationToken);
            ValidateStorageMetric(maybeMetric, metricId);

            return maybeMetric;
        });

        public ValueTask<IMetric> RemoveMetricByIdAsync(
            Guid metricId,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidateMetricId(metricId);
            IMetric maybeMetric = await this.storageBroker.SelectMetricByIdAsync(metricId, cancellationToken);
            ValidateStorageMetric(maybeMetric, metricId);

            return await this.storageBroker.DeleteMetricAsync(maybeMetric, cancellationToken);
        });

        public ValueTask<int> PurgeMetricsOlderThanRetentionPeriodAsync(
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (this.metricServiceConfigurations.IsPurgingAllowed is false)
            {
                return 0;
            }

            // Guarded rather than obeyed. A zero or negative retention period would make the
            // cut off date the present or the future and delete the entire table.
            ValidateRetentionPeriod(this.metricServiceConfigurations.RetentionPeriodInDays);
            int batchSize = this.metricServiceConfigurations.PurgeBatchSize;
            ValidatePurgeBatchSize(batchSize);

            DateTimeOffset currentDateTime = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

            DateTimeOffset cutOffDate =
                currentDateTime.AddDays(-this.metricServiceConfigurations.RetentionPeriodInDays);

            // Deleted in bounded batches, in the database. Selecting the expired rows into memory
            // first would size the cost of a purge by the size of the retention window, which on
            // this table is exactly the case that must not fall over - the first purge after a
            // period of not purging at all.
            int totalDeleted = 0;
            int deletedInBatch;

            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                deletedInBatch = await this.storageBroker.DeleteMetricsOlderThanAsync(
                    cutOffDate,
                    batchSize,
                    cancellationToken);

                totalDeleted += deletedInBatch;
            }
            while (deletedInBatch == batchSize);

            if (totalDeleted == 0)
            {
                return 0;
            }

            await this.loggingBroker.LogInformationAsync(
                $"Purged {totalDeleted} metric(s) created before {cutOffDate}.");

            return totalDeleted;
        });
    }
}
