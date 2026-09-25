// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Metrics.Exceptions;
using LondonFhirService.Core.Models.Processings.Metrics.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Processings.Metrics
{
    internal partial class MetricProcessingService
    {
        private delegate ValueTask<T> ReturningFunction<T>();

        private async ValueTask<T> TryCatch<T>(ReturningFunction<T> returningFunction)
        {
            try
            {
                return await returningFunction();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (InvalidArgumentMetricProcessingException invalidArgumentMetricProcessingException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidArgumentMetricProcessingException);
            }
            catch (MetricServiceValidationException metricServiceValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(metricServiceValidationException);
            }
            catch (MetricServiceDependencyValidationException metricServiceDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    metricServiceDependencyValidationException);
            }
            catch (MetricServiceDependencyException metricServiceDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(metricServiceDependencyException);
            }
            catch (MetricServiceException metricServiceException)
            {
                throw await CreateAndLogDependencyExceptionAsync(metricServiceException);
            }
            catch (Exception exception)
            {
                var failedMetricProcessingServiceException =
                    new FailedMetricProcessingServiceException(
                        message: "Failed metric processing service error occurred, please contact support.",
                        innerException: exception,
                        data: exception.Data);

                throw await CreateAndLogServiceExceptionAsync(failedMetricProcessingServiceException);
            }
        }

        private async ValueTask<MetricProcessingValidationException> CreateAndLogValidationExceptionAsync(
            Xeption exception)
        {
            var metricProcessingValidationException =
                new MetricProcessingValidationException(
                    message: "Metric processing validation error occurred, please fix errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(metricProcessingValidationException);

            return metricProcessingValidationException;
        }

        private async ValueTask<MetricProcessingDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var metricProcessingDependencyValidationException =
                new MetricProcessingDependencyValidationException(
                    message: "Metric processing dependency validation error occurred, " +
                        "please fix errors and try again.",
                    innerException: exception.InnerException as Xeption);

            await this.loggingBroker.LogErrorAsync(metricProcessingDependencyValidationException);

            return metricProcessingDependencyValidationException;
        }

        private async ValueTask<MetricProcessingDependencyException> CreateAndLogDependencyExceptionAsync(
            Xeption exception)
        {
            var metricProcessingDependencyException =
                new MetricProcessingDependencyException(
                    message: "Metric processing dependency error occurred, please contact support.",
                    innerException: exception.InnerException as Xeption);

            await this.loggingBroker.LogErrorAsync(metricProcessingDependencyException);

            return metricProcessingDependencyException;
        }

        private async ValueTask<MetricProcessingServiceException> CreateAndLogServiceExceptionAsync(
            Xeption exception)
        {
            var metricProcessingServiceException =
                new MetricProcessingServiceException(
                    message: "Metric processing service error occurred, please contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(metricProcessingServiceException);

            return metricProcessingServiceException;
        }
    }
}
