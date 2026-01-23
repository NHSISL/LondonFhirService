// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

//using System;
//using System.Threading.Tasks;
//using Hl7.Fhir.Model;
//using LondonFhirService.Core.Models.Foundations.FhirReconciliations.Exceptions;
//using LondonFhirService.Core.Models.Foundations.Patients.Exceptions;
//using LondonFhirService.Core.Models.Foundations.Providers.Exceptions;
//using LondonFhirService.Core.Models.Orchestrations.Patients.Exceptions;
//using Xeptions;

//namespace LondonFhirService.Core.Services.Orchestrations.Patients.R4
//{
//    public partial class R4PatientOrchestrationService
//    {
//        private delegate ValueTask<Bundle> ReturningBundleFunction();
//        private delegate ValueTask<string> ReturningStringFunction();

//        private async ValueTask<Bundle> TryCatch(ReturningBundleFunction returningBundleFunction)
//        {
//            try
//            {
//                return await returningBundleFunction();
//            }
//            catch (InvalidArgumentPatientOrchestrationException invalidArgumentPatientOrchestrationException)
//            {
//                throw await CreateAndLogValidationExceptionAsync(invalidArgumentPatientOrchestrationException);
//            }
//            catch (InvalidPrimaryProviderPatientOrchestrationException
//                   invalidPrimaryProviderPatientOrchestrationException)
//            {
//                throw await CreateAndLogValidationExceptionAsync(invalidPrimaryProviderPatientOrchestrationException);
//            }
//            catch (ProviderServiceValidationException providerServiceValidationException)
//            {
//                throw await CreateAndLogDependencyValidationExceptionAsync(providerServiceValidationException);
//            }
//            catch (ProviderServiceDependencyValidationException providerServiceDependencyValidationException)
//            {
//                throw await CreateAndLogDependencyValidationExceptionAsync(
//                    providerServiceDependencyValidationException);
//            }
//            catch (PatientServiceValidationException patientServiceValidationException)
//            {
//                throw await CreateAndLogDependencyValidationExceptionAsync(patientServiceValidationException);
//            }
//            catch (PatientServiceDependencyValidationException patientServiceDependencyValidationException)
//            {
//                throw await CreateAndLogDependencyValidationExceptionAsync(
//                    patientServiceDependencyValidationException);
//            }
//            catch (FhirReconciliationServiceValidationException fhirReconciliationServiceValidationException)
//            {
//                throw await CreateAndLogDependencyValidationExceptionAsync(
//                    fhirReconciliationServiceValidationException);
//            }
//            catch (FhirReconciliationServiceDependencyValidationException
//                   fhirReconciliationServiceDependencyValidationException)
//            {
//                throw await CreateAndLogDependencyValidationExceptionAsync(
//                    fhirReconciliationServiceDependencyValidationException);
//            }
//            catch (ProviderServiceDependencyException providerServiceDependencyException)
//            {
//                throw await CreateAndLogDependencyExceptionAsync(providerServiceDependencyException);
//            }
//            catch (ProviderServiceException providerServiceException)
//            {
//                throw await CreateAndLogDependencyExceptionAsync(providerServiceException);
//            }
//            catch (PatientServiceDependencyException patientServiceDependencyException)
//            {
//                throw await CreateAndLogDependencyExceptionAsync(patientServiceDependencyException);
//            }
//            catch (PatientServiceException patientServiceException)
//            {
//                throw await CreateAndLogDependencyExceptionAsync(patientServiceException);
//            }
//            catch (FhirReconciliationServiceDependencyException fhirReconciliationServiceDependencyException)
//            {
//                throw await CreateAndLogDependencyExceptionAsync(fhirReconciliationServiceDependencyException);
//            }
//            catch (FhirReconciliationServiceException fhirReconciliationServiceException)
//            {
//                throw await CreateAndLogDependencyExceptionAsync(fhirReconciliationServiceException);
//            }
//            catch (Exception exception)
//            {
//                var failedPatientOrchestrationException =
//                    new FailedPatientOrchestrationException(
//                        message: "Failed patient orchestration error occurred, please contact support.",
//                        innerException: exception,
//                        data: exception.Data);

//                throw await CreateAndLogServiceExceptionAsync(failedPatientOrchestrationException);
//            }
//        }

//        private async ValueTask<string> TryCatch(ReturningStringFunction returningStringFunction)
//        {
//            try
//            {
//                return await returningStringFunction();
//            }
//            catch (InvalidArgumentPatientOrchestrationException invalidArgumentPatientOrchestrationException)
//            {
//                throw await CreateAndLogValidationExceptionAsync(invalidArgumentPatientOrchestrationException);
//            }
//            catch (InvalidPrimaryProviderPatientOrchestrationException
//                   invalidPrimaryProviderPatientOrchestrationException)
//            {
//                throw await CreateAndLogValidationExceptionAsync(invalidPrimaryProviderPatientOrchestrationException);
//            }
//            catch (ProviderServiceValidationException providerServiceValidationException)
//            {
//                throw await CreateAndLogDependencyValidationExceptionAsync(providerServiceValidationException);
//            }
//            catch (ProviderServiceDependencyValidationException providerServiceDependencyValidationException)
//            {
//                throw await CreateAndLogDependencyValidationExceptionAsync(
//                    providerServiceDependencyValidationException);
//            }
//            catch (PatientServiceValidationException patientServiceValidationException)
//            {
//                throw await CreateAndLogDependencyValidationExceptionAsync(patientServiceValidationException);
//            }
//            catch (PatientServiceDependencyValidationException patientServiceDependencyValidationException)
//            {
//                throw await CreateAndLogDependencyValidationExceptionAsync(
//                    patientServiceDependencyValidationException);
//            }
//            catch (FhirReconciliationServiceValidationException fhirReconciliationServiceValidationException)
//            {
//                throw await CreateAndLogDependencyValidationExceptionAsync(
//                    fhirReconciliationServiceValidationException);
//            }
//            catch (FhirReconciliationServiceDependencyValidationException
//                   fhirReconciliationServiceDependencyValidationException)
//            {
//                throw await CreateAndLogDependencyValidationExceptionAsync(
//                    fhirReconciliationServiceDependencyValidationException);
//            }
//            catch (ProviderServiceDependencyException providerServiceDependencyException)
//            {
//                throw await CreateAndLogDependencyExceptionAsync(providerServiceDependencyException);
//            }
//            catch (ProviderServiceException providerServiceException)
//            {
//                throw await CreateAndLogDependencyExceptionAsync(providerServiceException);
//            }
//            catch (PatientServiceDependencyException patientServiceDependencyException)
//            {
//                throw await CreateAndLogDependencyExceptionAsync(patientServiceDependencyException);
//            }
//            catch (PatientServiceException patientServiceException)
//            {
//                throw await CreateAndLogDependencyExceptionAsync(patientServiceException);
//            }
//            catch (FhirReconciliationServiceDependencyException fhirReconciliationServiceDependencyException)
//            {
//                throw await CreateAndLogDependencyExceptionAsync(fhirReconciliationServiceDependencyException);
//            }
//            catch (FhirReconciliationServiceException fhirReconciliationServiceException)
//            {
//                throw await CreateAndLogDependencyExceptionAsync(fhirReconciliationServiceException);
//            }
//            catch (Exception exception)
//            {
//                var failedPatientOrchestrationException =
//                    new FailedPatientOrchestrationException(
//                        message: "Failed patient orchestration error occurred, please contact support.",
//                        innerException: exception,
//                        data: exception.Data);

//                throw await CreateAndLogServiceExceptionAsync(failedPatientOrchestrationException);
//            }
//        }

//        private async ValueTask<PatientOrchestrationValidationException> CreateAndLogValidationExceptionAsync(
//            Xeption exception)
//        {
//            var patientOrchestrationValidationException =
//                new PatientOrchestrationValidationException(
//                    message: "Patient orchestration validation error occurred, please try again.",
//                    innerException: exception);

//            await this.loggingBroker.LogErrorAsync(patientOrchestrationValidationException);

//            return patientOrchestrationValidationException;
//        }

//        private async ValueTask<PatientOrchestrationDependencyValidationException>
//            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
//        {
//            var patientOrchestrationDependencyValidationException =
//                new PatientOrchestrationDependencyValidationException(
//                    message: "Patient orchestration dependency validation error occurred, please try again.",
//                    exception.InnerException as Xeption);

//            await this.loggingBroker.LogErrorAsync(patientOrchestrationDependencyValidationException);

//            return patientOrchestrationDependencyValidationException;
//        }

//        private async ValueTask<PatientOrchestrationDependencyException> CreateAndLogDependencyExceptionAsync(
//            Xeption exception)
//        {
//            var patientOrchestrationDependencyException =
//                new PatientOrchestrationDependencyException(
//                    message: "Patient orchestration dependency error occurred, fix the errors and try again.",
//                    innerException: exception.InnerException as Xeption);

//            await this.loggingBroker.LogErrorAsync(patientOrchestrationDependencyException);

//            return patientOrchestrationDependencyException;
//        }

//        private async ValueTask<PatientOrchestrationServiceException> CreateAndLogServiceExceptionAsync(
//            Xeption exception)
//        {
//            var patientOrchestrationServiceException =
//                new PatientOrchestrationServiceException(
//                    message: "Patient orchestration service error occurred, please contact support.",
//                    innerException: exception);

//            await this.loggingBroker.LogErrorAsync(patientOrchestrationServiceException);

//            return patientOrchestrationServiceException;
//        }
//    }
//}
