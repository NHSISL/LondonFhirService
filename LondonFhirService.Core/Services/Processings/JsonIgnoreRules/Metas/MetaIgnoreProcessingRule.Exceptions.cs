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
    public partial class MetaIgnoreProcessingRule
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
                var failedJsonIgnoreRulesProcessingException =
                    new FailedJsonIgnoreRulesProcessingException(
                        message:
                            "Failed meta ignore processing exception occurred, please contact support",

                        innerException: exception,
                        data: exception.Data);

                throw await CreateAndLogServiceExceptionAsync(failedJsonIgnoreRulesProcessingException);
            }
        }

        private async ValueTask<JsonIgnoreRulesProcessingValidationException> CreateAndLogValidationExceptionAsync(
            Xeption exception)
        {
            var jsonIgnoreRulesProcessingValidationException =
                new JsonIgnoreRulesProcessingValidationException(
                    message: "Meta ignore processing validation error occurred, please fix errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(jsonIgnoreRulesProcessingValidationException);

            return jsonIgnoreRulesProcessingValidationException;
        }

        private async ValueTask<JsonIgnoreRulesProcessingDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var jsonIgnoreRulesProcessingDependencyValidationException =
                new JsonIgnoreRulesProcessingDependencyValidationException(
                    message: "Meta ignore processing dependency validation error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(jsonIgnoreRulesProcessingDependencyValidationException);

            return jsonIgnoreRulesProcessingDependencyValidationException;
        }

        private async ValueTask<JsonIgnoreRulesProcessingDependencyException> CreateAndLogDependencyExceptionAsync(
            Xeption exception)
        {
            var jsonIgnoreRulesProcessingDependencyException =
                new JsonIgnoreRulesProcessingDependencyException(
                    message: "Meta ignore processing dependency error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(jsonIgnoreRulesProcessingDependencyException);

            return jsonIgnoreRulesProcessingDependencyException;
        }

        private async ValueTask<JsonIgnoreRulesProcessingServiceException> CreateAndLogServiceExceptionAsync(
            Xeption exception)
        {
            var jsonIgnoreRulesProcessingServiceException =
                new JsonIgnoreRulesProcessingServiceException(
                    message: "Meta ignore processing service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(jsonIgnoreRulesProcessingServiceException);

            return jsonIgnoreRulesProcessingServiceException;
        }
    }
}
