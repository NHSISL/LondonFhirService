// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers.Appointments
{
    public partial class AppointmentMatcherService
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
        }

        private async ValueTask<ResourceMatcherServiceValidationException> CreateAndLogValidationException(
            Xeption exception)
        {
            var resourceMatcherServiceValidationException =
                new ResourceMatcherServiceValidationException(
                    message: "Appointment matcher validation errors occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(resourceMatcherServiceValidationException);

            return resourceMatcherServiceValidationException;
        }
    }
}
