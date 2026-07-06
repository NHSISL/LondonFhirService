// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.ConsumerAccesses
{
    public partial class ConsumerAccessService
    {
        private delegate ValueTask<ConsumerAccess> ReturningConsumerAccessFunction();

        private async ValueTask<ConsumerAccess> TryCatch(ReturningConsumerAccessFunction returningConsumerAccessFunction)
        {
            try
            {
                return await returningConsumerAccessFunction();
            }
            catch (InvalidConsumerAccessServiceException invalidConsumerAccessServiceException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidConsumerAccessServiceException);
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

        private async ValueTask<ConsumerAccessServiceDependencyValidationException> CreateAndLogDependencyValidationExceptionAsync(
            Xeption exception)
        {
            var consumerAccessDependencyValidationException = new ConsumerAccessServiceDependencyValidationException(
                message: "ConsumerAccess dependency validation error occurred, fix errors and try again.",
                innerException: exception);

            await this.loggingBroker.LogErrorAsync(consumerAccessDependencyValidationException);

            return consumerAccessDependencyValidationException;
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
