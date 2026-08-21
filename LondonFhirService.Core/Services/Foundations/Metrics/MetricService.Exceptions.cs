// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
using LondonFhirService.Core.Models.Foundations.Metrics;
using LondonFhirService.Core.Models.Foundations.Metrics.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.Metrics
{
    public partial class MetricService
    {
        private delegate ValueTask<Metric> ReturningMetricFunction();
        private delegate ValueTask<IQueryable<Metric>> ReturningMetricsFunction();
        private delegate ValueTask ReturningNothingFunction();
        private delegate ValueTask<int> ReturningCountFunction();

        private async ValueTask<Metric> TryCatch(ReturningMetricFunction returningMetricFunction)
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
            catch (SqlException sqlException)
            {
                var failedStorageMetricException =
                    new FailedStorageMetricException(
                        message: "Failed metric storage error occurred, contact support.",
                        innerException: sqlException);

                throw await CreateAndLogCriticalDependencyExceptionAsync(failedStorageMetricException);
            }
            catch (OperationCanceledException operationCanceledException)
                when (operationCanceledException.InnerException is TimeoutException)
            {
                var timedOutMetricServiceException =
                    new TimedOutMetricServiceException(
                        message: "Metric request timed out, please try again.",
                        innerException: operationCanceledException,
                        data: operationCanceledException.Data);

                throw await CreateAndLogDependencyExceptionAsync(timedOutMetricServiceException);
            }
            catch (TimeoutException timeoutException)
            {
                var timedOutMetricServiceException =
                    new TimedOutMetricServiceException(
                        message: "Metric request timed out, please try again.",
                        innerException: timeoutException,
                        data: timeoutException.Data);

                throw await CreateAndLogDependencyExceptionAsync(timedOutMetricServiceException);
            }
            catch (OperationCanceledException operationCanceledException)
            {
                var cancelledMetricServiceException =
                    new CancelledMetricServiceException(
                        message: "Metric request was cancelled, please try again.",
                        innerException: operationCanceledException,
                        data: operationCanceledException.Data);

                throw await CreateAndLogDependencyExceptionAsync(cancelledMetricServiceException);
            }
            catch (DuplicateKeyException duplicateKeyException)
            {
                var alreadyExistsMetricException =
                    new AlreadyExistsMetricException(
                        message: "Metric with the same Id already exists.",
                        innerException: duplicateKeyException);

                throw await CreateAndLogDependencyValidationExceptionAsync(alreadyExistsMetricException);
            }
            catch (ForeignKeyConstraintConflictException foreignKeyConstraintConflictException)
            {
                var invalidReferenceMetricException =
                    new InvalidReferenceMetricException(
                        message: "Invalid metric reference error occurred.",
                        innerException: foreignKeyConstraintConflictException);

                throw await CreateAndLogDependencyValidationExceptionAsync(invalidReferenceMetricException);
            }
            catch (DbUpdateConcurrencyException dbUpdateConcurrencyException)
            {
                var lockedMetricException =
                    new LockedMetricException(
                        message: "Locked metric record exception, please try again later.",
                        innerException: dbUpdateConcurrencyException);

                throw await CreateAndLogDependencyValidationExceptionAsync(lockedMetricException);
            }
            catch (DbUpdateException databaseUpdateException)
            {
                var failedStorageMetricException =
                    new FailedStorageMetricException(
                        message: "Failed metric storage error occurred, contact support.",
                        innerException: databaseUpdateException);

                throw await CreateAndLogDependencyExceptionAsync(failedStorageMetricException);
            }
            catch (Exception exception)
            {
                var failedMetricServiceException =
                    new FailedMetricServiceException(
                        message: "Failed metric service occurred, please contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedMetricServiceException);
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
            catch (SqlException sqlException)
            {
                var failedStorageMetricException =
                    new FailedStorageMetricException(
                        message: "Failed metric storage error occurred, contact support.",
                        innerException: sqlException);

                throw await CreateAndLogCriticalDependencyExceptionAsync(failedStorageMetricException);
            }
            catch (OperationCanceledException operationCanceledException)
                when (operationCanceledException.InnerException is TimeoutException)
            {
                var timedOutMetricServiceException =
                    new TimedOutMetricServiceException(
                        message: "Metric request timed out, please try again.",
                        innerException: operationCanceledException,
                        data: operationCanceledException.Data);

                throw await CreateAndLogDependencyExceptionAsync(timedOutMetricServiceException);
            }
            catch (TimeoutException timeoutException)
            {
                var timedOutMetricServiceException =
                    new TimedOutMetricServiceException(
                        message: "Metric request timed out, please try again.",
                        innerException: timeoutException,
                        data: timeoutException.Data);

                throw await CreateAndLogDependencyExceptionAsync(timedOutMetricServiceException);
            }
            catch (OperationCanceledException operationCanceledException)
            {
                var cancelledMetricServiceException =
                    new CancelledMetricServiceException(
                        message: "Metric request was cancelled, please try again.",
                        innerException: operationCanceledException,
                        data: operationCanceledException.Data);

                throw await CreateAndLogDependencyExceptionAsync(cancelledMetricServiceException);
            }
            catch (DuplicateKeyException duplicateKeyException)
            {
                var alreadyExistsMetricException =
                    new AlreadyExistsMetricException(
                        message: "Metric with the same Id already exists.",
                        innerException: duplicateKeyException);

                throw await CreateAndLogDependencyValidationExceptionAsync(alreadyExistsMetricException);
            }
            catch (DbUpdateConcurrencyException dbUpdateConcurrencyException)
            {
                var lockedMetricException =
                    new LockedMetricException(
                        message: "Locked metric record exception, please try again later.",
                        innerException: dbUpdateConcurrencyException);

                throw await CreateAndLogDependencyValidationExceptionAsync(lockedMetricException);
            }
            catch (DbUpdateException databaseUpdateException)
            {
                var failedStorageMetricException =
                    new FailedStorageMetricException(
                        message: "Failed metric storage error occurred, contact support.",
                        innerException: databaseUpdateException);

                throw await CreateAndLogDependencyExceptionAsync(failedStorageMetricException);
            }
            catch (Exception exception)
            {
                var failedMetricServiceException =
                    new FailedMetricServiceException(
                        message: "Failed metric service occurred, please contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedMetricServiceException);
            }
        }

        private async ValueTask<IQueryable<Metric>> TryCatch(ReturningMetricsFunction returningMetricsFunction)
        {
            try
            {
                return await returningMetricsFunction();
            }
            catch (SqlException sqlException)
            {
                var failedStorageMetricException =
                    new FailedStorageMetricException(
                        message: "Failed metric storage error occurred, contact support.",
                        innerException: sqlException);

                throw await CreateAndLogCriticalDependencyExceptionAsync(failedStorageMetricException);
            }
            catch (OperationCanceledException operationCanceledException)
                when (operationCanceledException.InnerException is TimeoutException)
            {
                var timedOutMetricServiceException =
                    new TimedOutMetricServiceException(
                        message: "Metric request timed out, please try again.",
                        innerException: operationCanceledException,
                        data: operationCanceledException.Data);

                throw await CreateAndLogDependencyExceptionAsync(timedOutMetricServiceException);
            }
            catch (TimeoutException timeoutException)
            {
                var timedOutMetricServiceException =
                    new TimedOutMetricServiceException(
                        message: "Metric request timed out, please try again.",
                        innerException: timeoutException,
                        data: timeoutException.Data);

                throw await CreateAndLogDependencyExceptionAsync(timedOutMetricServiceException);
            }
            catch (OperationCanceledException operationCanceledException)
            {
                var cancelledMetricServiceException =
                    new CancelledMetricServiceException(
                        message: "Metric request was cancelled, please try again.",
                        innerException: operationCanceledException,
                        data: operationCanceledException.Data);

                throw await CreateAndLogDependencyExceptionAsync(cancelledMetricServiceException);
            }
            catch (Exception exception)
            {
                var failedMetricServiceException =
                    new FailedMetricServiceException(
                        message: "Failed metric service occurred, please contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedMetricServiceException);
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
            catch (SqlException sqlException)
            {
                var failedStorageMetricException =
                    new FailedStorageMetricException(
                        message: "Failed metric storage error occurred, contact support.",
                        innerException: sqlException);

                throw await CreateAndLogCriticalDependencyExceptionAsync(failedStorageMetricException);
            }
            catch (OperationCanceledException operationCanceledException)
                when (operationCanceledException.InnerException is TimeoutException)
            {
                var timedOutMetricServiceException =
                    new TimedOutMetricServiceException(
                        message: "Metric request timed out, please try again.",
                        innerException: operationCanceledException,
                        data: operationCanceledException.Data);

                throw await CreateAndLogDependencyExceptionAsync(timedOutMetricServiceException);
            }
            catch (TimeoutException timeoutException)
            {
                var timedOutMetricServiceException =
                    new TimedOutMetricServiceException(
                        message: "Metric request timed out, please try again.",
                        innerException: timeoutException,
                        data: timeoutException.Data);

                throw await CreateAndLogDependencyExceptionAsync(timedOutMetricServiceException);
            }
            catch (OperationCanceledException operationCanceledException)
            {
                var cancelledMetricServiceException =
                    new CancelledMetricServiceException(
                        message: "Metric request was cancelled, please try again.",
                        innerException: operationCanceledException,
                        data: operationCanceledException.Data);

                throw await CreateAndLogDependencyExceptionAsync(cancelledMetricServiceException);
            }
            catch (DbUpdateConcurrencyException dbUpdateConcurrencyException)
            {
                var lockedMetricException =
                    new LockedMetricException(
                        message: "Locked metric record exception, please try again later.",
                        innerException: dbUpdateConcurrencyException);

                throw await CreateAndLogDependencyValidationExceptionAsync(lockedMetricException);
            }
            catch (DbUpdateException databaseUpdateException)
            {
                var failedStorageMetricException =
                    new FailedStorageMetricException(
                        message: "Failed metric storage error occurred, contact support.",
                        innerException: databaseUpdateException);

                throw await CreateAndLogDependencyExceptionAsync(failedStorageMetricException);
            }
            catch (Exception exception)
            {
                var failedMetricServiceException =
                    new FailedMetricServiceException(
                        message: "Failed metric service occurred, please contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedMetricServiceException);
            }
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

        private async ValueTask<MetricServiceException> CreateAndLogServiceExceptionAsync(Xeption exception)
        {
            var metricServiceException =
                new MetricServiceException(
                    message: "Metric service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(metricServiceException);

            return metricServiceException;
        }
    }
}
