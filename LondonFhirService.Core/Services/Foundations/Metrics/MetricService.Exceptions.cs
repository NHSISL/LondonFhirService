// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
// Only the client exceptions are imported. A blanket using would collide with Core's own
// MetricServiceException, since both layers name their categories the same way.
using AbstractionExceptions = LondonFhirService.Core.Abstractions.Models.Metrics.Exceptions;
using ClientExceptions = LondonFhirService.Clients.AuditAndMetrics.Models.Metrics.Exceptions;
using LondonFhirService.Core.Models.Foundations.Metrics.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.Metrics
{
    internal partial class MetricService
    {
        private delegate ValueTask<T> ReturningGenericFunction<T>();
        private delegate ValueTask ReturningNothingFunction();

        /// <summary>
        /// The client has already categorised the failure; this localises its exceptions into
        /// this service's own so callers keep depending on Core's contract rather than the
        /// library's. Cancellation is not translated - it travels out as cancellation.
        /// </summary>
        private async ValueTask<T> TryCatch<T>(ReturningGenericFunction<T> returningFunction)
        {
            try
            {
                return await returningFunction();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (ClientExceptions.MetricClientNotFoundException metricClientNotFoundException)
            {
                throw await CreateAndLogNotFoundExceptionAsync(metricClientNotFoundException);
            }
            catch (ClientExceptions.MetricClientValidationException metricClientValidationException)
            {
                throw await CreateAndLogValidationExceptionAsync(metricClientValidationException);
            }
            catch (ClientExceptions.MetricClientDependencyException metricClientDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(metricClientDependencyException);
            }
            catch (ClientExceptions.MetricClientServiceException metricClientServiceException)
            {
                throw await CreateAndLogServiceExceptionAsync(metricClientServiceException);
            }
            catch (Exception exception)
            {
                throw await CreateAndLogServiceExceptionAsync(exception);
            }
        }

        private async ValueTask TryCatch(ReturningNothingFunction returningNothingFunction) =>
            await TryCatch<bool>(async () =>
            {
                await returningNothingFunction();

                return true;
            });

        /// <summary>
        /// Translated into this application's own category, because the controllers dispatch on
        /// Core's types to choose a status code. Without this a missing span is carried in a type
        /// the controller cannot name, and every not-found collapses into a 400.
        /// </summary>
        private async ValueTask<MetricServiceValidationException> CreateAndLogNotFoundExceptionAsync(
            Xeption exception)
        {
            var notFoundMetricServiceException =
                new NotFoundMetricServiceException(
                    message: exception.InnerException?.Message ?? "Metric not found.");

            var metricServiceValidationException =
                new MetricServiceValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: notFoundMetricServiceException);

            await this.loggingBroker.LogErrorAsync(metricServiceValidationException);

            return metricServiceValidationException;
        }

        /// <summary>
        /// The metric client collapses plain validation and dependency validation into one
        /// MetricClientValidationException, unlike the audit client which keeps them apart. The
        /// two still have to arrive at callers as different categories - a duplicate or locked
        /// row is not the same answer as a malformed span - so the inner exception decides which
        /// one this is.
        /// </summary>
        private async ValueTask<Xeption> CreateAndLogValidationExceptionAsync(Xeption exception)
        {
            if (exception.InnerException is ClientExceptions.MetricDependencyValidationException
                metricDependencyValidationException)
            {
                return await CreateAndLogDependencyValidationExceptionAsync(
                    metricDependencyValidationException);
            }

            var metricServiceValidationException =
                new MetricServiceValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: exception.InnerException as Xeption ?? exception);

            await this.loggingBroker.LogErrorAsync(metricServiceValidationException);

            return metricServiceValidationException;
        }

        private async ValueTask<MetricServiceDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            Xeption categorised = exception.InnerException switch
            {
                AbstractionExceptions.AlreadyExistsMetricException alreadyExists =>
                    new AlreadyExistsMetricServiceException(
                        message: "Metric with the same id already exists.",
                        innerException: alreadyExists,
                        data: alreadyExists.Data),

                AbstractionExceptions.InvalidReferenceMetricException invalidReference =>
                    new InvalidReferenceMetricServiceException(
                        message: "Invalid metric reference error occurred.",
                        innerException: invalidReference),

                AbstractionExceptions.LockedMetricException locked =>
                    new LockedMetricServiceException(
                        message: "Locked metric record exception, please try again later.",
                        innerException: locked),

                Xeption inner => inner,
                _ => exception,
            };

            var metricServiceDependencyValidationException =
                new MetricServiceDependencyValidationException(
                    message: "Metric dependency validation occurred, please try again.",
                    innerException: categorised);

            await this.loggingBroker.LogErrorAsync(metricServiceDependencyValidationException);

            return metricServiceDependencyValidationException;
        }

        private async ValueTask<MetricServiceDependencyException> CreateAndLogDependencyExceptionAsync(
            Xeption exception)
        {
            var metricServiceDependencyException =
                new MetricServiceDependencyException(
                    message: "Metric dependency error occurred, please contact support.",
                    innerException: exception.InnerException as Xeption ?? exception);

            await this.loggingBroker.LogCriticalAsync(metricServiceDependencyException);

            return metricServiceDependencyException;
        }

        private async ValueTask<MetricServiceException> CreateAndLogServiceExceptionAsync(Exception exception)
        {
            var failedMetricServiceException =
                new FailedMetricServiceException(
                    message: "Failed metric service error occurred, please contact support.",
                    innerException: exception);

            var metricServiceException =
                new MetricServiceException(
                    message: "Metric service error occurred, please contact support.",
                    innerException: failedMetricServiceException);

            await this.loggingBroker.LogErrorAsync(metricServiceException);

            return metricServiceException;
        }
    }
}
