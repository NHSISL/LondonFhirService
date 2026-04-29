// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers.FamilyMemberHistories
{
    public partial class FamilyMemberHistoryMatcherService
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
                var failedResourceMatcherServiceException =
                    new FailedResourceMatcherServiceException(
                        message: "Failed family member history matcher service occurred, please contact support",
                        innerException: exception);

                throw await CreateAndLogServiceException(failedResourceMatcherServiceException);
            }
        }

        private async ValueTask<ResourceMatcherServiceValidationException> CreateAndLogValidationException(
            Xeption exception)
        {
            var resourceMatcherServiceValidationException =
                new ResourceMatcherServiceValidationException(
                    message: "Family member history matcher validation errors occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(resourceMatcherServiceValidationException);

            return resourceMatcherServiceValidationException;
        }

        private async ValueTask<ResourceMatcherServiceException> CreateAndLogServiceException(
            Xeption exception)
        {
            var resourceMatcherServiceException =
                new ResourceMatcherServiceException(
                    message: "Family member history matcher service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(resourceMatcherServiceException);

            return resourceMatcherServiceException;
        }
    }
}
