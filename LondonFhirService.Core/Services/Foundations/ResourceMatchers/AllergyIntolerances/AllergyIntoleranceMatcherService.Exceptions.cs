// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.AllergyIntolerances.Exceptions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers.AllergyIntolerances
{
    public partial class AllergyIntoleranceMatcherService
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
                var failedAllergyIntoleranceMatcherServiceException =
                    new FailedAllergyIntoleranceMatcherServiceException(
                        message: "Failed allergy intolerance matcher service occurred, please contact support",
                        innerException: exception);

                throw await CreateAndLogServiceException(failedAllergyIntoleranceMatcherServiceException);
            }
        }

        private async ValueTask<AllergyIntoleranceMatcherServiceValidationException> CreateAndLogValidationException(
            Xeption exception)
        {
            var allergyIntoleranceMatcherServiceValidationException =
                new AllergyIntoleranceMatcherServiceValidationException(
                    message: "Allergy intolerance matcher validation errors occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(allergyIntoleranceMatcherServiceValidationException);

            return allergyIntoleranceMatcherServiceValidationException;
        }

        private async ValueTask<AllergyIntoleranceMatcherServiceException> CreateAndLogServiceException(
            Xeption exception)
        {
            var allergyIntoleranceMatcherServiceException =
                new AllergyIntoleranceMatcherServiceException(
                    message: "Allergy intolerance matcher service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(allergyIntoleranceMatcherServiceException);

            return allergyIntoleranceMatcherServiceException;
        }
    }
}
