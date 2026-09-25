// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Orchestrations.Metrics.Exceptions;
using LondonFhirService.Core.Models.Processings.Metrics.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Orchestrations.Metrics
{
    internal partial class MetricOrchestrationService
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
            catch (MetricProcessingValidationException metricProcessingValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    metricProcessingValidationException);
            }
            catch (MetricProcessingDependencyValidationException metricProcessingDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    metricProcessingDependencyValidationException);
            }
            catch (MetricProcessingDependencyException metricProcessingDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(metricProcessingDependencyException);
            }
            catch (MetricProcessingServiceException metricProcessingServiceException)
            {
                throw await CreateAndLogDependencyExceptionAsync(metricProcessingServiceException);
            }
            catch (Exception exception)
            {
                var failedMetricOrchestrationServiceException =
                    new FailedMetricOrchestrationServiceException(
                        message: "Failed metric orchestration service error occurred, please contact support.",
                        innerException: exception,
                        data: exception.Data);

                throw await CreateAndLogServiceExceptionAsync(failedMetricOrchestrationServiceException);
            }
        }

        private async ValueTask<MetricOrchestrationDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var metricOrchestrationDependencyValidationException =
                new MetricOrchestrationDependencyValidationException(
                    message: "Metric orchestration dependency validation error occurred, " +
                        "please fix errors and try again.",
                    innerException: exception.InnerException as Xeption);

            await this.loggingBroker.LogErrorAsync(metricOrchestrationDependencyValidationException);

            return metricOrchestrationDependencyValidationException;
        }

        private async ValueTask<MetricOrchestrationDependencyException> CreateAndLogDependencyExceptionAsync(
            Xeption exception)
        {
            var metricOrchestrationDependencyException =
                new MetricOrchestrationDependencyException(
                    message: "Metric orchestration dependency error occurred, please contact support.",
                    innerException: exception.InnerException as Xeption);

            await this.loggingBroker.LogErrorAsync(metricOrchestrationDependencyException);

            return metricOrchestrationDependencyException;
        }

        private async ValueTask<MetricOrchestrationServiceException> CreateAndLogServiceExceptionAsync(
            Xeption exception)
        {
            var metricOrchestrationServiceException =
                new MetricOrchestrationServiceException(
                    message: "Metric orchestration service error occurred, please contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(metricOrchestrationServiceException);

            return metricOrchestrationServiceException;
        }
    }
}
