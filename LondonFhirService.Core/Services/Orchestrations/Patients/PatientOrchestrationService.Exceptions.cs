// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Orchestrations.Patients.Exceptions;

namespace LondonFhirService.Core.Services.Orchestrations.Patients
{
    public partial class PatientOrchestrationService
    {
        private delegate ValueTask<Bundle> ReturningBundleFunction();

        private async ValueTask<Bundle> TryCatch(ReturningBundleFunction returningBundleFunction)
        {
            try
            {
                return await returningBundleFunction();
            }
            catch (InvalidArgumentPatientOrchestrationException invalidArgumentPatientOrchestrationException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidArgumentPatientOrchestrationException);
            }
        }

        private async ValueTask<PatientOrchestrationValidationException> CreateAndLogValidationExceptionAsync(
            InvalidArgumentPatientOrchestrationException invalidArgumentPatientOrchestrationException)
        {
            var patientOrchestrationValidationException =
                new PatientOrchestrationValidationException(
                    message: "Patient orchestration validation error occurred, please try again.",
                    innerException: invalidArgumentPatientOrchestrationException);

            await this.loggingBroker.LogErrorAsync(patientOrchestrationValidationException);

            return patientOrchestrationValidationException;
        }
    }
}
