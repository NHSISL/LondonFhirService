// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Net.Http;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.ConsumerAccesses
{
    internal partial class ConsumerAccessService
    {
        private delegate ValueTask<ConsumerAccess> ReturningConsumerAccessFunction();

        private async ValueTask<ConsumerAccess> TryCatch(
            ReturningConsumerAccessFunction returningConsumerAccessFunction)
        {
            try
            {
                return await returningConsumerAccessFunction();
            }
            catch (NullConsumerAccessServiceException nullConsumerAccessServiceException)
            {
                throw await CreateAndLogValidationExceptionAsync(nullConsumerAccessServiceException);
            }
            catch (InvalidConsumerAccessServiceException invalidConsumerAccessServiceException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidConsumerAccessServiceException);
            }
            catch (OperationCanceledException operationCanceledException)
                when (operationCanceledException.InnerException is TimeoutException)
            {
                var timedOutConsumerAccessServiceException =
                    new TimedOutConsumerAccessServiceException(
                        message: "Consumer access request timed out, please try again.",
                        innerException: operationCanceledException,
                        data: operationCanceledException.Data);

                throw await CreateAndLogDependencyExceptionAsync(timedOutConsumerAccessServiceException);
            }
            catch (TimeoutException timeoutException)
            {
                var timedOutConsumerAccessServiceException =
                    new TimedOutConsumerAccessServiceException(
                        message: "Consumer access request timed out, please try again.",
                        innerException: timeoutException,
                        data: timeoutException.Data);

                throw await CreateAndLogDependencyExceptionAsync(timedOutConsumerAccessServiceException);
            }
            catch (OperationCanceledException operationCanceledException)
            {
                var cancelledConsumerAccessServiceException =
                    new CancelledConsumerAccessServiceException(
                        message: "Consumer access request was cancelled, please try again.",
                        innerException: operationCanceledException,
                        data: operationCanceledException.Data);

                throw await CreateAndLogDependencyExceptionAsync(cancelledConsumerAccessServiceException);
            }
            catch (HttpRequestException httpRequestException)
            {
                var failedConsumerAccessDependencyException =
                    new FailedConsumerAccessDependencyException(
                        message: "Failed consumer access dependency error occurred, contact support.",
                        innerException: httpRequestException,
                        data: httpRequestException.Data);

                throw await CreateAndLogCriticalDependencyExceptionAsync(failedConsumerAccessDependencyException);
            }
            catch (Exception exception)
            {
                var failedConsumerAccessServiceException =
                    new FailedConsumerAccessServiceException(
                        message: "Failed service consumer access error occurred, contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedConsumerAccessServiceException);
            }
        }

        private async ValueTask<ConsumerAccessServiceValidationException> CreateAndLogValidationExceptionAsync(
            Xeption exception)
        {
            var consumerAccessServiceValidationException = new ConsumerAccessServiceValidationException(
                message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                innerException: exception);

            await this.loggingBroker.LogErrorAsync(consumerAccessServiceValidationException);

            return consumerAccessServiceValidationException;
        }

        private async ValueTask<ConsumerAccessServiceDependencyException> CreateAndLogCriticalDependencyExceptionAsync(
            Xeption exception)
        {
            var consumerAccessServiceDependencyException = new ConsumerAccessServiceDependencyException(
                message: "ConsumerAccess dependency error occurred, contact support.",
                innerException: exception);

            await this.loggingBroker.LogCriticalAsync(consumerAccessServiceDependencyException);

            return consumerAccessServiceDependencyException;
        }

        private async ValueTask<ConsumerAccessServiceDependencyException> CreateAndLogDependencyExceptionAsync(
            Xeption exception)
        {
            var consumerAccessServiceDependencyException = new ConsumerAccessServiceDependencyException(
                message: "ConsumerAccess dependency error occurred, contact support.",
                innerException: exception);

            await this.loggingBroker.LogErrorAsync(consumerAccessServiceDependencyException);

            return consumerAccessServiceDependencyException;
        }

        private async ValueTask<ConsumerAccessServiceException> CreateAndLogServiceExceptionAsync(
           Xeption exception)
        {
            var consumerAccessServiceException = new ConsumerAccessServiceException(
                message: "Service error occurred, contact support.",
                innerException: exception);

            await this.loggingBroker.LogErrorAsync(consumerAccessServiceException);

            return consumerAccessServiceException;
        }
    }
}
