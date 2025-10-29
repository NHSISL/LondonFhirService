// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Coordinations.Patients.Exceptions;

namespace LondonFhirService.Core.Services.Coordinations.Patients
{
    public partial class PatientCoordinationService
    {
        private delegate ValueTask<Bundle> ReturningBundleFunction();

        private async ValueTask<Bundle> TryCatch(ReturningBundleFunction returningBundleFunction)
        {
            try
            {
                return await returningBundleFunction();
            }
            catch (InvalidArgumentPatientCoordinationException invalidArgumentPatientCoordinationException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidArgumentPatientCoordinationException);
            }
        }

        private async ValueTask<PatentCoordinationValidationException> CreateAndLogValidationExceptionAsync(
            InvalidArgumentPatientCoordinationException invalidArgumentPatientCoordinationException)
        {
            var patientCoordinationValidationException =
                new PatentCoordinationValidationException(
                    message: "Patient coordination validation error occurred, please try again.",
                    innerException: invalidArgumentPatientCoordinationException);

            await this.loggingBroker.LogErrorAsync(patientCoordinationValidationException);

            return patientCoordinationValidationException;
        }
    }
}
