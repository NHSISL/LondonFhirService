// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.JsonElements.Exceptions;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.Exceptions;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.MetaIgnoreRules.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Processings.JsonIgnoreRules
{
    public partial class MetaIgnoreProcessingRule
    {
        private delegate ValueTask<T> ReturningFunction<T>();

        private async ValueTask<T> TryCatch<T>(ReturningFunction<T> returningFunction)
        {
            try
            {
                return await returningFunction();
            }
            catch (InvalidJsonIgnoreProcessingException invalidJsonIgnoreProcessingException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidJsonIgnoreProcessingException);
            }
            catch (JsonElementServiceValidationException jsonElementServiceValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    jsonElementServiceValidationException);
            }
            catch (JsonElementServiceDependencyException jsonElementServiceDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(
                    jsonElementServiceDependencyException);
            }
            catch (JsonElementServiceDependencyValidationException jsonElementServiceDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    jsonElementServiceDependencyValidationException);
            }
            catch (JsonElementServiceException jsonElementServiceException)
            {
                throw await CreateAndLogDependencyExceptionAsync(
                    jsonElementServiceException);
            }
            catch (Exception exception)
            {
                var failedMetaIgnoreProcessingException =
                    new FailedMetaIgnoreProcessingException(
                        message:
                            "Failed meta ignore processing exception occurred, please contact support",

                        innerException: exception,
                        data: exception.Data);

                throw await CreateAndLogServiceExceptionAsync(failedMetaIgnoreProcessingException);
            }
        }

        private async ValueTask<MetaIgnoreProcessingValidationException> CreateAndLogValidationExceptionAsync(
            Xeption exception)
        {
            var metaIgnoreProcessingValidationException =
                new MetaIgnoreProcessingValidationException(
                    message: "Meta ignore processing validation error occurred, please fix errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(metaIgnoreProcessingValidationException);

            return metaIgnoreProcessingValidationException;
        }

        private async ValueTask<MetaIgnoreProcessingDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var metaIgnoreProcessingDependencyValidationException =
                new MetaIgnoreProcessingDependencyValidationException(
                    message: "Meta ignore processing dependency validation occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(metaIgnoreProcessingDependencyValidationException);

            return metaIgnoreProcessingDependencyValidationException;
        }

        private async ValueTask<MetaIgnoreProcessingDependencyException> CreateAndLogDependencyExceptionAsync(
            Xeption exception)
        {
            var metaIgnoreProcessingDependencyException =
                new MetaIgnoreProcessingDependencyException(
                    message: "Meta ignore processing dependency error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(metaIgnoreProcessingDependencyException);

            return metaIgnoreProcessingDependencyException;
        }

        private async ValueTask<MetaIgnoreProcessingServiceException> CreateAndLogServiceExceptionAsync(
            Xeption exception)
        {
            var metaIgnoreProcessingServiceException =
                new MetaIgnoreProcessingServiceException(
                    message: "Meta ignore processing service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(metaIgnoreProcessingServiceException);

            return metaIgnoreProcessingServiceException;
        }
    }
}
