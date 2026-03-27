// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Lists.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers.Lists
{
    public partial class ListMatcherService
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
                var failedListMatcherServiceException =
                    new FailedListMatcherServiceException(
                        message: "Failed list matcher service occurred, please contact support",
                        innerException: exception);

                throw await CreateAndLogServiceException(failedListMatcherServiceException);
            }
        }

        private async ValueTask<ListMatcherServiceValidationException> CreateAndLogValidationException(
            Xeption exception)
        {
            var listMatcherServiceValidationException =
                new ListMatcherServiceValidationException(
                    message: "List matcher validation errors occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(listMatcherServiceValidationException);

            return listMatcherServiceValidationException;
        }

        private async ValueTask<ListMatcherServiceException> CreateAndLogServiceException(
            Xeption exception)
        {
            var listMatcherServiceException =
                new ListMatcherServiceException(
                    message: "List matcher service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(listMatcherServiceException);

            return listMatcherServiceException;
        }
    }
}
