// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.JsonElements.Exceptions;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.Exceptions;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.IdIgnoreRules.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Processings.JsonIgnoreRules
{
    public partial class IdIgnoreProcessingRule : JsonIgnoreProcessingRuleBase, IJsonIgnoreProcessingRule
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
                var failedIdIgnoreProcessingException =
                    new FailedIdIgnoreProcessingException(
                        message:
                            "Failed id ignore processing exception occurred, please contact support",

                        innerException: exception,
                        data: exception.Data);

                throw await CreateAndLogServiceExceptionAsync(failedIdIgnoreProcessingException);
            }
        }

        private async ValueTask<IdIgnoreProcessingValidationException> CreateAndLogValidationExceptionAsync(
            Xeption exception)
        {
            var arrayOrderIgnoreProcessingValidationException =
                new IdIgnoreProcessingValidationException(
                    message: "IdIgnoreProcessing validation error occurred, please fix errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(arrayOrderIgnoreProcessingValidationException);

            return arrayOrderIgnoreProcessingValidationException;
        }

        private async ValueTask<IdIgnoreProcessingDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var arrayOrderIgnoreProcessingDependencyValidationException =
                new IdIgnoreProcessingDependencyValidationException(
                    message: "IdIgnoreProcessing dependency validation occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(arrayOrderIgnoreProcessingDependencyValidationException);
            return arrayOrderIgnoreProcessingDependencyValidationException;
        }

        private async ValueTask<IdIgnoreProcessingDependencyException> CreateAndLogDependencyExceptionAsync(
            Xeption exception)
        {
            var arrayOrderIgnoreProcessingDependencyException =
                new IdIgnoreProcessingDependencyException(
                    message: "IdIgnoreProcessing dependency error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(arrayOrderIgnoreProcessingDependencyException);

            return arrayOrderIgnoreProcessingDependencyException;
        }

        private async ValueTask<IdIgnoreProcessingServiceException> CreateAndLogServiceExceptionAsync(
            Xeption exception)
        {
            var odsDataServiceException =
                new IdIgnoreProcessingServiceException(
                    message: "IdIgnoreProcessing service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(odsDataServiceException);

            return odsDataServiceException;
        }
    }
}
