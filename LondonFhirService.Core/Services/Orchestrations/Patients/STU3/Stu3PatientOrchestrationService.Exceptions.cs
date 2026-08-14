// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions;
using LondonFhirService.Core.Models.Foundations.Patients.Exceptions;
using LondonFhirService.Core.Models.Foundations.Providers.Exceptions;
using LondonFhirService.Core.Models.Orchestrations.Patients;
using LondonFhirService.Core.Models.Orchestrations.Patients.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Orchestrations.Patients.STU3
{
    public partial class Stu3PatientOrchestrationService
    {
        private delegate ValueTask<StructuredRecordsResponse> ReturningStructuredRecordsResponseFunction();

        private delegate ValueTask ReturningNothingFunction();

        private async ValueTask<StructuredRecordsResponse> TryCatch(
            ReturningStructuredRecordsResponseFunction returningStructuredRecordsResponseFunction)
        {
            try
            {
                return await returningStructuredRecordsResponseFunction();
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
            catch (UnauthorizedPatientOrchestrationException unauthorizedPatientOrchestrationException)
            {
                throw await CreateAndLogValidationExceptionAsync(unauthorizedPatientOrchestrationException);
            }
            catch (ForbiddenPatientOrchestrationException forbiddenPatientOrchestrationException)
            {
                throw await CreateAndLogValidationExceptionAsync(forbiddenPatientOrchestrationException);
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
            catch (ConsumerAccessServiceValidationException consumerAccessServiceValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(consumerAccessServiceValidationException);
            }
            catch (ProviderServiceDependencyException providerServiceDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(providerServiceDependencyException);
            }
            catch (ProviderServiceException providerServiceException)
            {
                throw await CreateAndLogDependencyExceptionAsync(providerServiceException);
            }
            catch (PatientServiceDependencyException patientServiceDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(patientServiceDependencyException);
            }
            catch (PatientServiceException patientServiceException)
            {
                throw await CreateAndLogDependencyExceptionAsync(patientServiceException);
            }
            catch (ConsumerAccessServiceDependencyException consumerAccessServiceDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(consumerAccessServiceDependencyException);
            }
            catch (ConsumerAccessServiceException consumerAccessServiceException)
            {
                throw await CreateAndLogDependencyExceptionAsync(consumerAccessServiceException);
            }
            catch (Exception exception)
            {
                var failedPatientOrchestrationException =
                    new FailedPatientOrchestrationException(
                        message: "Failed patient orchestration error occurred, please contact support.",
                        innerException: exception,
                        data: exception.Data);

                throw await CreateAndLogServiceExceptionAsync(failedPatientOrchestrationException);
            }
        }

        private async ValueTask TryCatch(ReturningNothingFunction returningNothingFunction)
        {
            try
            {
                await returningNothingFunction();
            }
            catch (InvalidArgumentPatientOrchestrationException invalidArgumentPatientOrchestrationException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidArgumentPatientOrchestrationException);
            }
            catch (UnauthorizedPatientOrchestrationException unauthorizedPatientOrchestrationException)
            {
                throw await CreateAndLogValidationExceptionAsync(unauthorizedPatientOrchestrationException);
            }
            catch (ForbiddenPatientOrchestrationException forbiddenPatientOrchestrationException)
            {
                throw await CreateAndLogValidationExceptionAsync(forbiddenPatientOrchestrationException);
            }
            catch (ConsumerAccessServiceValidationException consumerAccessServiceValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(consumerAccessServiceValidationException);
            }
            catch (ConsumerAccessServiceDependencyException consumerAccessServiceDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(consumerAccessServiceDependencyException);
            }
            catch (ConsumerAccessServiceException consumerAccessServiceException)
            {
                throw await CreateAndLogDependencyExceptionAsync(consumerAccessServiceException);
            }
            catch (Exception exception)
            {
                var failedPatientOrchestrationException =
                    new FailedPatientOrchestrationException(
                        message: "Failed patient orchestration error occurred, please contact support.",
                        innerException: exception,
                        data: exception.Data);

                throw await CreateAndLogServiceExceptionAsync(failedPatientOrchestrationException);
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

        private async ValueTask<PatientOrchestrationDependencyException> CreateAndLogDependencyExceptionAsync(
            Xeption exception)
        {
            var patientOrchestrationDependencyException =
                new PatientOrchestrationDependencyException(
                    message: "Patient orchestration dependency error occurred, fix the errors and try again.",
                    innerException: exception.InnerException as Xeption);

            await this.loggingBroker.LogErrorAsync(patientOrchestrationDependencyException);

            return patientOrchestrationDependencyException;
        }

        private async ValueTask<PatientOrchestrationServiceException> CreateAndLogServiceExceptionAsync(
            Xeption exception)
        {
            var patientOrchestrationServiceException =
                new PatientOrchestrationServiceException(
                    message: "Patient orchestration service error occurred, please contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(patientOrchestrationServiceException);

            return patientOrchestrationServiceException;
        }
    }
}
