// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Clients.MetricClient.Exceptions;
using LondonFhirService.Core.Models.Foundations.Metrics;
using LondonFhirService.Core.Models.Foundations.Metrics.Exceptions;
using LondonFhirService.Core.Services.Foundations.Metrics;
using Xeptions;

namespace LondonFhirService.Core.Clients.Metrics
{
    public class MetricClient : IMetricClient
    {
        private readonly IMetricService metricService;

        public MetricClient(IMetricService metricService) =>
            this.metricService = metricService;

        public async ValueTask<Metric> AddMetricAsync(
            Metric metric,
            CancellationToken cancellationToken = default) =>
            await TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                return await this.metricService.AddMetricAsync(metric, cancellationToken);
            });

        public async ValueTask AddMetricsAsync(
            List<Metric> metrics,
            CancellationToken cancellationToken = default) =>
            await TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                await this.metricService.AddMetricsAsync(metrics, cancellationToken);
            });

        public async ValueTask<IQueryable<Metric>> RetrieveAllMetricsAsync(
            CancellationToken cancellationToken = default) =>
            await TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                return await this.metricService.RetrieveAllMetricsAsync(cancellationToken);
            });

        public async ValueTask<Metric> RetrieveMetricByIdAsync(
            Guid metricId,
            CancellationToken cancellationToken = default) =>
            await TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                return await this.metricService.RetrieveMetricByIdAsync(metricId, cancellationToken);
            });

        public async ValueTask<Metric> RemoveMetricByIdAsync(
            Guid metricId,
            CancellationToken cancellationToken = default) =>
            await TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                return await this.metricService.RemoveMetricByIdAsync(metricId, cancellationToken);
            });

        public async ValueTask<int> PurgeMetricsOlderThanRetentionPeriodAsync(
            CancellationToken cancellationToken = default) =>
            await TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                return await this.metricService.PurgeMetricsOlderThanRetentionPeriodAsync(cancellationToken);
            });

        private delegate ValueTask<T> ReturningGenericFunction<T>();
        private delegate ValueTask ReturningNothingFunction();

        /// <summary>
        /// The AuditClient sample repeats these catch blocks per method. With six methods that is
        /// four times the duplication for no added clarity, so the mapping is written once here,
        /// in the same delegate and TryCatch shape the foundation services in this codebase use.
        /// </summary>
        private static async ValueTask<T> TryCatch<T>(ReturningGenericFunction<T> returningGenericFunction)
        {
            try
            {
                return await returningGenericFunction();
            }
            catch (OperationCanceledException)
            {
                // Deliberately not translated. A cancelled caller gets the cancellation it asked
                // for rather than a client exception wrapping it.
                throw;
            }
            catch (MetricValidationException metricValidationException)
            {
                throw CreateValidationException(metricValidationException);
            }
            catch (MetricDependencyValidationException metricDependencyValidationException)
            {
                throw CreateValidationException(metricDependencyValidationException);
            }
            catch (MetricDependencyException metricDependencyException)
            {
                throw CreateDependencyException(metricDependencyException);
            }
            catch (MetricServiceException metricServiceException)
            {
                throw CreateServiceException(metricServiceException);
            }
        }

        private static async ValueTask TryCatch(ReturningNothingFunction returningNothingFunction)
        {
            try
            {
                await returningNothingFunction();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (MetricValidationException metricValidationException)
            {
                throw CreateValidationException(metricValidationException);
            }
            catch (MetricDependencyValidationException metricDependencyValidationException)
            {
                throw CreateValidationException(metricDependencyValidationException);
            }
            catch (MetricDependencyException metricDependencyException)
            {
                throw CreateDependencyException(metricDependencyException);
            }
            catch (MetricServiceException metricServiceException)
            {
                throw CreateServiceException(metricServiceException);
            }
        }

        private static MetricClientValidationException CreateValidationException(Xeption exception) =>
            new MetricClientValidationException(
                message: "Metric client validation error occurred, fix errors and try again.",
                innerException: exception.InnerException as Xeption);

        private static MetricClientDependencyException CreateDependencyException(Xeption exception) =>
            new MetricClientDependencyException(
                message: "Metric client dependency error occurred, please contact support.",
                innerException: exception.InnerException as Xeption);

        private static MetricClientServiceException CreateServiceException(Xeption exception) =>
            new MetricClientServiceException(
                message: "Metric client service error occurred, fix errors and try again.",
                innerException: exception.InnerException as Xeption);
    }
}
