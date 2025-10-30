// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Foundations.FhirReconciliations.Exceptions;
using LondonFhirService.Core.Models.Foundations.Patients.Exceptions;
using LondonFhirService.Core.Models.Foundations.Providers.Exceptions;
using LondonFhirService.Core.Models.Orchestrations.Patients.Exceptions;
using Xeptions;

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
            catch (InvalidPrimaryProviderPatientOrchestrationException
                   invalidPrimaryProviderPatientOrchestrationException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidPrimaryProviderPatientOrchestrationException);
            }
            catch (ProviderServiceValidationException providerServiceValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(providerServiceValidationException);
            }
            catch (ProviderServiceDependencyValidationException providerServiceDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    providerServiceDependencyValidationException);
            }
            catch (PatientServiceValidationException patientServiceValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(patientServiceValidationException);
            }
            catch (PatientServiceDependencyValidationException patientServiceDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    patientServiceDependencyValidationException);
            }
            catch (FhirReconciliationServiceValidationException fhirReconciliationServiceValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    fhirReconciliationServiceValidationException);
            }
            catch (FhirReconciliationServiceDependencyValidationException
                   fhirReconciliationServiceDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    fhirReconciliationServiceDependencyValidationException);
            }
        }

        private async ValueTask<PatientOrchestrationValidationException> CreateAndLogValidationExceptionAsync(
            Xeption exception)
        {
            var patientOrchestrationValidationException =
                new PatientOrchestrationValidationException(
                    message: "Patient orchestration validation error occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(patientOrchestrationValidationException);

            return patientOrchestrationValidationException;
        }

        private async ValueTask<PatientOrchestrationDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var patientOrchestrationDependencyValidationException =
                new PatientOrchestrationDependencyValidationException(
                    message: "Patient orchestration dependency validation error occurred, please try again.",
                    exception.InnerException as Xeption);

            await this.loggingBroker.LogErrorAsync(patientOrchestrationDependencyValidationException);

            return patientOrchestrationDependencyValidationException;
        }
    }
}
