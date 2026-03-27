// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Conditions.Exceptions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers.Conditions
{
    public partial class ConditionMatcherService
    {
        private delegate ValueTask<T> ReturningFunction<T>();

        private async ValueTask<T> TryCatch<T>(ReturningFunction<T> returningFunction)
        {
            try
            {
                return await returningFunction();
            }
            catch (InvalidArgumentResourceMatcherException invalidArgumentResourceMatcherException)
            {
                throw await CreateAndLogValidationException(invalidArgumentResourceMatcherException);
            }
            catch (Exception exception)
            {
                var failedConditionMatcherServiceException =
                    new FailedConditionMatcherServiceException(
                        message: "Failed condition matcher service occurred, please contact support",
                        innerException: exception);

                throw await CreateAndLogServiceException(failedConditionMatcherServiceException);
            }
        }

        private async ValueTask<ConditionMatcherServiceValidationException> CreateAndLogValidationException(
            Xeption exception)
        {
            var conditionMatcherServiceValidationException =
                new ConditionMatcherServiceValidationException(
                    message: "Condition matcher validation errors occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(conditionMatcherServiceValidationException);

            return conditionMatcherServiceValidationException;
        }

        private async ValueTask<ConditionMatcherServiceException> CreateAndLogServiceException(
            Xeption exception)
        {
            var conditionMatcherServiceException =
                new ConditionMatcherServiceException(
                    message: "Condition matcher service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(conditionMatcherServiceException);

            return conditionMatcherServiceException;
        }
    }
}
