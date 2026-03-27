// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Patients.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers.Patients
{
    public partial class PatientMatcherService : ResourceMatcherServiceBase, IResourceMatcherService
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
                var failedPatientMatcherServiceException =
                    new FailedPatientMatcherServiceException(
                        message: "Failed patient matcher service occurred, please contact support",
                        innerException: exception);

                throw await CreateAndLogServiceException(failedPatientMatcherServiceException);
            }
        }

        private async ValueTask<PatientMatcherServiceValidationException> CreateAndLogValidationException(
            Xeption exception)
        {
            var patientMatcherServiceValidationException =
                new PatientMatcherServiceValidationException(
                    message: "Patient matcher validation errors occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(patientMatcherServiceValidationException);

            return patientMatcherServiceValidationException;
        }

        private async ValueTask<PatientMatcherServiceException> CreateAndLogServiceException(
            Xeption exception)
        {
            var patientMatcherServiceException =
                new PatientMatcherServiceException(
                    message: "Patient matcher service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(patientMatcherServiceException);

            return patientMatcherServiceException;
        }
    }
}
