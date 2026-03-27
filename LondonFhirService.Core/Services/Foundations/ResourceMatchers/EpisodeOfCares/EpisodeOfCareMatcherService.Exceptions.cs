// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.EpisodeOfCares.Exceptions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers.EpisodeOfCares
{
    public partial class EpisodeOfCareMatcherService
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
                var failedEpisodeOfCareMatcherServiceException =
                    new FailedEpisodeOfCareMatcherServiceException(
                        message: "Failed episode of care matcher service occurred, please contact support",
                        innerException: exception);

                throw await CreateAndLogServiceException(failedEpisodeOfCareMatcherServiceException);
            }
        }

        private async ValueTask<EpisodeOfCareMatcherServiceValidationException> CreateAndLogValidationException(
            Xeption exception)
        {
            var episodeOfCareMatcherServiceValidationException =
                new EpisodeOfCareMatcherServiceValidationException(
                    message: "Episode of care matcher validation errors occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(episodeOfCareMatcherServiceValidationException);

            return episodeOfCareMatcherServiceValidationException;
        }

        private async ValueTask<EpisodeOfCareMatcherServiceException> CreateAndLogServiceException(
            Xeption exception)
        {
            var episodeOfCareMatcherServiceException =
                new EpisodeOfCareMatcherServiceException(
                    message: "Episode of care matcher service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(episodeOfCareMatcherServiceException);

            return episodeOfCareMatcherServiceException;
        }
    }
}
