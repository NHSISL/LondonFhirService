// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Coordinations.Patients.Exceptions;
using LondonFhirService.Core.Models.Orchestrations.Accesses.Exceptions;
using LondonFhirService.Core.Models.Orchestrations.Patients.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Coordinations.Patients.STU3
{
    public partial class Stu3PatientCoordinationService
    {
        private delegate ValueTask<Bundle> ReturningBundleFunction();
        private delegate ValueTask<string> ReturningStringFunction();

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
            catch (AccessOrchestrationValidationException accessOrchestrationValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(accessOrchestrationValidationException);
            }
            catch (AccessOrchestrationDependencyValidationException accessOrchestrationDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    accessOrchestrationDependencyValidationException);
            }
            catch (PatientOrchestrationValidationException patientOrchestrationValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(patientOrchestrationValidationException);
            }
            catch (PatientOrchestrationDependencyValidationException patientOrchestrationDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    patientOrchestrationDependencyValidationException);
            }
            catch (AccessOrchestrationDependencyException accessOrchestrationDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(accessOrchestrationDependencyException);
            }
            catch (AccessOrchestrationServiceException accessOrchestrationServiceException)
            {
                throw await CreateAndLogDependencyExceptionAsync(accessOrchestrationServiceException);
            }
            catch (PatientOrchestrationDependencyException patientOrchestrationDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(patientOrchestrationDependencyException);
            }
            catch (PatientOrchestrationServiceException patientOrchestrationServiceException)
            {
                throw await CreateAndLogDependencyExceptionAsync(patientOrchestrationServiceException);
            }
            catch (Exception exception)
            {
                var failedPatientCoordinationServiceException =
                    new FailedPatientCoordinationException(
                        message: "Failed patient coordination service error occurred, please contact support.",
                        innerException: exception,
                        data: exception.Data);

                throw await CreateAndLogServiceExceptionAsync(failedPatientCoordinationServiceException);
            }
        }

        private async ValueTask<string> TryCatch(ReturningStringFunction returningStringFunction)
        {
            try
            {
                return await returningStringFunction();
            }
            catch (InvalidArgumentPatientCoordinationException invalidArgumentPatientCoordinationException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidArgumentPatientCoordinationException);
            }
            catch (AccessOrchestrationValidationException accessOrchestrationValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(accessOrchestrationValidationException);
            }
            catch (AccessOrchestrationDependencyValidationException accessOrchestrationDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    accessOrchestrationDependencyValidationException);
            }
            catch (PatientOrchestrationValidationException patientOrchestrationValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(patientOrchestrationValidationException);
            }
            catch (PatientOrchestrationDependencyValidationException patientOrchestrationDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    patientOrchestrationDependencyValidationException);
            }
            catch (AccessOrchestrationDependencyException accessOrchestrationDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(accessOrchestrationDependencyException);
            }
            catch (AccessOrchestrationServiceException accessOrchestrationServiceException)
            {
                throw await CreateAndLogDependencyExceptionAsync(accessOrchestrationServiceException);
            }
            catch (PatientOrchestrationDependencyException patientOrchestrationDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(patientOrchestrationDependencyException);
            }
            catch (PatientOrchestrationServiceException patientOrchestrationServiceException)
            {
                throw await CreateAndLogDependencyExceptionAsync(patientOrchestrationServiceException);
            }
            catch (Exception exception)
            {
                var failedPatientCoordinationServiceException =
                    new FailedPatientCoordinationException(
                        message: "Failed patient coordination service error occurred, please contact support.",
                        innerException: exception,
                        data: exception.Data);

                throw await CreateAndLogServiceExceptionAsync(failedPatientCoordinationServiceException);
            }
        }

        private async ValueTask<PatientCoordinationValidationException> CreateAndLogValidationExceptionAsync(
            InvalidArgumentPatientCoordinationException invalidArgumentPatientCoordinationException)
        {
            var patientCoordinationValidationException =
                new PatientCoordinationValidationException(
                    message: "Patient coordination validation error occurred, please try again.",
                    innerException: invalidArgumentPatientCoordinationException);

            await this.loggingBroker.LogErrorAsync(patientCoordinationValidationException);

            return patientCoordinationValidationException;
        }

        private async ValueTask<PatientCoordinationDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var patientCoordinationDependencyValidationException =
                new PatientCoordinationDependencyValidationException(
                    message: "Patient coordination dependency validation error occurred, please try again.",
                    exception.InnerException as Xeption);

            await this.loggingBroker.LogErrorAsync(patientCoordinationDependencyValidationException);

            return patientCoordinationDependencyValidationException;
        }

        private async ValueTask<PatientCoordinationDependencyException> CreateAndLogDependencyExceptionAsync(
            Xeption exception)
        {
            var patientCoordinationDependencyException =
                new PatientCoordinationDependencyException(
                    message: "Patient coordination dependency error occurred, fix the errors and try again.",
                    innerException: exception.InnerException as Xeption);

            await this.loggingBroker.LogErrorAsync(patientCoordinationDependencyException);

            return patientCoordinationDependencyException;
        }

        private async ValueTask<PatientCoordinationServiceException> CreateAndLogServiceExceptionAsync(
            Xeption exception)
        {
            var patientCoordinationServiceException =
                new PatientCoordinationServiceException(
                    message: "Patient coordination service error occurred, please contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(patientCoordinationServiceException);

            return patientCoordinationServiceException;
        }
    }
}
