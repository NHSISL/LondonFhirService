// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.JsonElements.Exceptions;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.Exceptions;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.GuidIgnoreRules.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Processings.JsonIgnoreRules
{
    public partial class GuidIgnoreProcessingRule
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
                var failedGuidIgnoreProcessingException =
                    new FailedGuidIgnoreProcessingException(
                        message:
                            "Failed guid ignore processing exception occurred, please contact support",

                        innerException: exception,
                        data: exception.Data);

                throw await CreateAndLogServiceExceptionAsync(failedGuidIgnoreProcessingException);
            }
        }

        private async ValueTask<GuidIgnoreProcessingValidationException> CreateAndLogValidationExceptionAsync(
            Xeption exception)
        {
            var arrayOrderIgnoreProcessingValidationException =
                new GuidIgnoreProcessingValidationException(
                    message: "GuidIgnoreProcessing validation error occurred, please fix errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(arrayOrderIgnoreProcessingValidationException);

            return arrayOrderIgnoreProcessingValidationException;
        }

        private async ValueTask<GuidIgnoreProcessingDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var arrayOrderIgnoreProcessingDependencyValidationException =
                new GuidIgnoreProcessingDependencyValidationException(
                    message: "GuidIgnoreProcessing dependency validation occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(arrayOrderIgnoreProcessingDependencyValidationException);
            return arrayOrderIgnoreProcessingDependencyValidationException;
        }

        private async ValueTask<GuidIgnoreProcessingDependencyException> CreateAndLogDependencyExceptionAsync(
            Xeption exception)
        {
            var arrayOrderIgnoreProcessingDependencyException =
                new GuidIgnoreProcessingDependencyException(
                    message: "GuidIgnoreProcessing dependency error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(arrayOrderIgnoreProcessingDependencyException);

            return arrayOrderIgnoreProcessingDependencyException;
        }

        private async ValueTask<GuidIgnoreProcessingServiceException> CreateAndLogServiceExceptionAsync(
            Xeption exception)
        {
            var odsDataServiceException =
                new GuidIgnoreProcessingServiceException(
                    message: "GuidIgnoreProcessing service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(odsDataServiceException);

            return odsDataServiceException;
        }
    }
}
