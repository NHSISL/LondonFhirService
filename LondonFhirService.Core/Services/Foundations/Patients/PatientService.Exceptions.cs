// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Foundations.Patients.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.Patients
{
    public partial class PatientService
    {
        private delegate ValueTask<List<Bundle>> ReturningBundleListFunction();

        private async ValueTask<List<Bundle>> TryCatch(
            ReturningBundleListFunction returningBundleListFunction)
        {
            try
            {
                return await returningBundleListFunction();
            }
            catch (InvalidArgumentsPatientServiceException invalidArgumentsPatientServiceException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidArgumentsPatientServiceException);
            }
            catch (Exception exception)
            {
                var failedPatientServiceException =
                    new FailedPatientServiceException(
                        message: "Failed patient service error occurred, please contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedPatientServiceException);
            }
        }

        private async ValueTask<PatientServiceValidationException>
            CreateAndLogValidationExceptionAsync(Xeption exception)
        {
            var patientServiceValidationException =
                new PatientServiceValidationException(
                    message: "Patient service validation error occurred, " +
                        "please fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(patientServiceValidationException);

            return patientServiceValidationException;
        }

        private async ValueTask<PatientServiceException> CreateAndLogServiceExceptionAsync(
           Xeption exception)
        {
            var patientServiceException = new PatientServiceException(
                message: "Patient service error occurred, contact support.",
                innerException: exception);

            await this.loggingBroker.LogErrorAsync(patientServiceException);

            return patientServiceException;
        }
    }
}
