// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.MedicationStatements.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers.MedicationStatements
{
    public partial class MedicationStatementMatcherService : IResourceMatcherService
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
                var failedMedicationStatementMatcherServiceException =
                    new FailedMedicationStatementMatcherServiceException(
                        message: "Failed medication statement matcher service occurred, please contact support",
                        innerException: exception);

                throw await CreateAndLogServiceException(failedMedicationStatementMatcherServiceException);
            }
        }

        private async ValueTask<MedicationStatementMatcherServiceValidationException> CreateAndLogValidationException(
            Xeption exception)
        {
            var medicationStatementMatcherServiceValidationException =
                new MedicationStatementMatcherServiceValidationException(
                    message: "Medication statement matcher validation errors occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(medicationStatementMatcherServiceValidationException);

            return medicationStatementMatcherServiceValidationException;
        }

        private async ValueTask<MedicationStatementMatcherServiceException> CreateAndLogServiceException(
            Xeption exception)
        {
            var medicationStatementMatcherServiceException =
                new MedicationStatementMatcherServiceException(
                    message: "Medication statement matcher service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(medicationStatementMatcherServiceException);

            return medicationStatementMatcherServiceException;
        }
    }
}
