// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Medications.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers.Medications
{
    public partial class MedicationMatcherService : IResourceMatcherService
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
                var failedMedicationMatcherServiceException =
                    new FailedMedicationMatcherServiceException(
                        message: "Failed medication matcher service occurred, please contact support",
                        innerException: exception);

                throw await CreateAndLogServiceException(failedMedicationMatcherServiceException);
            }
        }

        private async ValueTask<MedicationMatcherServiceValidationException> CreateAndLogValidationException(
            Xeption exception)
        {
            var medicationMatcherServiceValidationException =
                new MedicationMatcherServiceValidationException(
                    message: "Medication matcher validation errors occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(medicationMatcherServiceValidationException);

            return medicationMatcherServiceValidationException;
        }

        private async ValueTask<MedicationMatcherServiceException> CreateAndLogServiceException(
            Xeption exception)
        {
            var medicationMatcherServiceException =
                new MedicationMatcherServiceException(
                    message: "Medication matcher service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(medicationMatcherServiceException);

            return medicationMatcherServiceException;
        }
    }
}
