// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.JsonElements.Exceptions;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.ArrayOrderIgnoreRules.Exceptions;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Processings.JsonIgnoreRules
{
    public partial class ArrayOrderIgnoreProcessingRule
    {
        private delegate ValueTask<T> ReturningFunction<T>();

        private async ValueTask<T> TryCatch<T>(ReturningFunction<T> returningFunction)
        {
            try
            {
                return await returningFunction();
            }
            catch (InvalidJsonIgnoreRulesProcessingException invalidJsonIgnoreProcessingException)
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
                var failedArrayOrderIgnoreProcessingException =
                    new FailedArrayOrderIgnoreProcessingException(
                        message:
                            "Failed array order ignore processing exception occurred, please contact support",

                        innerException: exception,
                        data: exception.Data);

                throw await CreateAndLogServiceExceptionAsync(failedArrayOrderIgnoreProcessingException);
            }
        }

        private async ValueTask<JsonIgnoreRulesProcessingValidationException> CreateAndLogValidationExceptionAsync(
            Xeption exception)
        {
            var arrayOrderIgnoreProcessingValidationException =
                new ArrayOrderIgnoreProcessingValidationException(
                    message: "Array order ignore processing validation error occurred, please fix errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(arrayOrderIgnoreProcessingValidationException);

            return arrayOrderIgnoreProcessingValidationException;
        }

        private async ValueTask<JsonIgnoreRulesProcessingDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var arrayOrderIgnoreProcessingDependencyValidationException =
                new ArrayOrderIgnoreProcessingDependencyValidationException(
                    message: "Array order ignore processing dependency validation error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(arrayOrderIgnoreProcessingDependencyValidationException);
 
            return arrayOrderIgnoreProcessingDependencyValidationException;
        }

        private async ValueTask<JsonIgnoreRulesProcessingDependencyException> CreateAndLogDependencyExceptionAsync(
            Xeption exception)
        {
            var arrayOrderIgnoreProcessingDependencyException =
                new ArrayOrderIgnoreProcessingDependencyException(
                    message: "Array order ignore processing dependency error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(arrayOrderIgnoreProcessingDependencyException);

            return arrayOrderIgnoreProcessingDependencyException;
        }

        private async ValueTask<JsonIgnoreRulesProcessingServiceException> CreateAndLogServiceExceptionAsync(
            Xeption exception)
        {
            var odsDataServiceException =
                new ArrayOrderIgnoreProcessingServiceException(
                    message: "Array order ignore processing service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(odsDataServiceException);

            return odsDataServiceException;
        }
    }
}
