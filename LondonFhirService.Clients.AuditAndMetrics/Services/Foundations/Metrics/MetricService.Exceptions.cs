// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Clients.AuditAndMetrics.Models.Metrics.Exceptions;
using LondonFhirService.Core.Abstractions.Models.Metrics.Exceptions;
using Xeptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Services.Foundations.Metrics
{
    internal partial class MetricService
    {
        private delegate ValueTask<IMetric> ReturningMetricFunction();
        private delegate ValueTask<IQueryable<IMetric>> ReturningMetricsFunction();
        private delegate ValueTask ReturningNothingFunction();
        private delegate ValueTask<int> ReturningCountFunction();

        /// <summary>
        /// There is no SqlException, DbUpdateException or DuplicateKeyException here any more.
        /// This library carries no ORM or database driver, so it cannot name those types. The
        /// storage broker implementation supplied by the hosting application is responsible for
        /// catching them and re-throwing the four storage exceptions below, which are the
        /// contract between the two.
        ///
        /// Cancellation and timeout are still handled here, because they originate from the
        /// caller's token rather than from storage.
        /// </summary>
        private async ValueTask<IMetric> TryCatch(ReturningMetricFunction returningMetricFunction)
        {
            try
            {
                return await returningMetricFunction();
            }
            catch (NullMetricException nullMetricException)
            {
                throw await CreateAndLogValidationExceptionAsync(nullMetricException);
            }
            catch (InvalidMetricException invalidMetricException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidMetricException);
            }
            catch (NotFoundMetricException notFoundMetricException)
            {
                throw await CreateAndLogValidationExceptionAsync(notFoundMetricException);
            }
            catch (OperationCanceledException operationCanceledException)
                when (operationCanceledException.InnerException is TimeoutException)
            {
                throw await CreateAndLogTimedOutExceptionAsync(operationCanceledException);
            }
            catch (TimeoutException timeoutException)
            {
                throw await CreateAndLogTimedOutExceptionAsync(timeoutException);
            }
            catch (OperationCanceledException operationCanceledException)
            {
                throw await CreateAndLogCancelledExceptionAsync(operationCanceledException);
            }
            catch (AlreadyExistsMetricException alreadyExistsMetricException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(alreadyExistsMetricException);
            }
            catch (InvalidReferenceMetricException invalidReferenceMetricException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(invalidReferenceMetricException);
            }
            catch (LockedMetricException lockedMetricException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(lockedMetricException);
            }
            catch (FailedStorageMetricException failedStorageMetricException)
            {
                throw await CreateAndLogCriticalDependencyExceptionAsync(failedStorageMetricException);
            }
            catch (Exception exception)
            {
                throw await CreateAndLogServiceExceptionAsync(exception);
            }
        }

        private async ValueTask TryCatch(ReturningNothingFunction returningNothingFunction)
        {
            try
            {
                await returningNothingFunction();
            }
            catch (NullMetricException nullMetricException)
            {
                throw await CreateAndLogValidationExceptionAsync(nullMetricException);
            }
            catch (InvalidMetricException invalidMetricException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidMetricException);
            }
            catch (OperationCanceledException operationCanceledException)
                when (operationCanceledException.InnerException is TimeoutException)
            {
                throw await CreateAndLogTimedOutExceptionAsync(operationCanceledException);
            }
            catch (TimeoutException timeoutException)
            {
                throw await CreateAndLogTimedOutExceptionAsync(timeoutException);
            }
            catch (OperationCanceledException operationCanceledException)
            {
                throw await CreateAndLogCancelledExceptionAsync(operationCanceledException);
            }
            catch (AlreadyExistsMetricException alreadyExistsMetricException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(alreadyExistsMetricException);
            }
            catch (LockedMetricException lockedMetricException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(lockedMetricException);
            }
            catch (FailedStorageMetricException failedStorageMetricException)
            {
                throw await CreateAndLogCriticalDependencyExceptionAsync(failedStorageMetricException);
            }
            catch (Exception exception)
            {
                throw await CreateAndLogServiceExceptionAsync(exception);
            }
        }

        private async ValueTask<IQueryable<IMetric>> TryCatch(ReturningMetricsFunction returningMetricsFunction)
        {
            try
            {
                return await returningMetricsFunction();
            }
            catch (OperationCanceledException operationCanceledException)
                when (operationCanceledException.InnerException is TimeoutException)
            {
                throw await CreateAndLogTimedOutExceptionAsync(operationCanceledException);
            }
            catch (TimeoutException timeoutException)
            {
                throw await CreateAndLogTimedOutExceptionAsync(timeoutException);
            }
            catch (OperationCanceledException operationCanceledException)
            {
                throw await CreateAndLogCancelledExceptionAsync(operationCanceledException);
            }
            catch (FailedStorageMetricException failedStorageMetricException)
            {
                throw await CreateAndLogCriticalDependencyExceptionAsync(failedStorageMetricException);
            }
            catch (Exception exception)
            {
                throw await CreateAndLogServiceExceptionAsync(exception);
            }
        }

        private async ValueTask<int> TryCatch(ReturningCountFunction returningCountFunction)
        {
            try
            {
                return await returningCountFunction();
            }
            catch (InvalidMetricException invalidMetricException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidMetricException);
            }
            catch (OperationCanceledException operationCanceledException)
                when (operationCanceledException.InnerException is TimeoutException)
            {
                throw await CreateAndLogTimedOutExceptionAsync(operationCanceledException);
            }
            catch (TimeoutException timeoutException)
            {
                throw await CreateAndLogTimedOutExceptionAsync(timeoutException);
            }
            catch (OperationCanceledException operationCanceledException)
            {
                throw await CreateAndLogCancelledExceptionAsync(operationCanceledException);
            }
            catch (LockedMetricException lockedMetricException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(lockedMetricException);
            }
            catch (FailedStorageMetricException failedStorageMetricException)
            {
                throw await CreateAndLogCriticalDependencyExceptionAsync(failedStorageMetricException);
            }
            catch (Exception exception)
            {
                throw await CreateAndLogServiceExceptionAsync(exception);
            }
        }

        private async ValueTask<MetricDependencyException> CreateAndLogTimedOutExceptionAsync(Exception exception)
        {
            var timedOutMetricServiceException =
                new TimedOutMetricServiceException(
                    message: "Metric request timed out, please try again.",
                    innerException: exception,
                    data: exception.Data);

            return await CreateAndLogDependencyExceptionAsync(timedOutMetricServiceException);
        }

        private async ValueTask<MetricDependencyException> CreateAndLogCancelledExceptionAsync(Exception exception)
        {
            var cancelledMetricServiceException =
                new CancelledMetricServiceException(
                    message: "Metric request was cancelled, please try again.",
                    innerException: exception,
                    data: exception.Data);

            return await CreateAndLogDependencyExceptionAsync(cancelledMetricServiceException);
        }

        private async ValueTask<MetricServiceException> CreateAndLogServiceExceptionAsync(Exception exception)
        {
            var failedMetricServiceException =
                new FailedMetricServiceException(
                    message: "Failed metric service occurred, please contact support.",
                    innerException: exception,
                    data: exception.Data);

            var metricServiceException =
                new MetricServiceException(
                    message: "Metric service error occurred, contact support.",
                    innerException: failedMetricServiceException);

            await this.loggingBroker.LogErrorAsync(metricServiceException);

            return metricServiceException;
        }

        private async ValueTask<MetricValidationException> CreateAndLogValidationExceptionAsync(Xeption exception)
        {
            var metricValidationException =
                new MetricValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(metricValidationException);

            return metricValidationException;
        }

        private async ValueTask<MetricDependencyException> CreateAndLogCriticalDependencyExceptionAsync(
            Xeption exception)
        {
            var metricDependencyException =
                new MetricDependencyException(
                    message: "Metric dependency error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogCriticalAsync(metricDependencyException);

            return metricDependencyException;
        }

        private async ValueTask<MetricDependencyValidationException> CreateAndLogDependencyValidationExceptionAsync(
            Xeption exception)
        {
            var metricDependencyValidationException =
                new MetricDependencyValidationException(
                    message: "Metric dependency validation occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(metricDependencyValidationException);

            return metricDependencyValidationException;
        }

        private async ValueTask<MetricDependencyException> CreateAndLogDependencyExceptionAsync(Xeption exception)
        {
            var metricDependencyException =
                new MetricDependencyException(
                    message: "Metric dependency error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(metricDependencyException);

            return metricDependencyException;
        }
    }
}
