// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Orchestrations.Patients.Exceptions;
using Moq;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Patients.R4
{
    public partial class R4PatientOrchestrationServiceTests
    {
        [Theory]
        [MemberData(nameof(DependencyValidationExceptions))]
        public async Task ShouldThrowDependencyValidationExceptionOnGetStructuredRecordIfErrorsAndLogItAsync(
            Xeption dependencyValidationException)
        {
            // given
            string randomString = GetRandomString();
            string inputNhsNumber = randomString;

            var expectedPatientOrchestrationDependencyValidationException =
                new PatientOrchestrationDependencyValidationException(
                    message: "Patient orchestration dependency validation error occurred, please try again.",
                    innerException: dependencyValidationException.InnerException as Xeption);

            this.providerServiceMock.Setup(service =>
                service.RetrieveAllProvidersAsync())
                    .ThrowsAsync(dependencyValidationException);

            // when
            ValueTask<Bundle> retrieveListOfDocumentsToProcessTask =
                this.patientOrchestrationService
                    .GetStructuredRecord(nhsNumber: inputNhsNumber);

            PatientOrchestrationDependencyValidationException
                actualPatientOrchestrationDependencyValidationException =
                    await Assert.ThrowsAsync<PatientOrchestrationDependencyValidationException>(
                        retrieveListOfDocumentsToProcessTask.AsTask);

            // then
            actualPatientOrchestrationDependencyValidationException.Should()
                .BeEquivalentTo(expectedPatientOrchestrationDependencyValidationException);

            this.providerServiceMock.Verify(service =>
                service.RetrieveAllProvidersAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(IsSameExceptionAs(
                    expectedPatientOrchestrationDependencyValidationException))),
                        Times.Once);

            this.providerServiceMock.VerifyNoOtherCalls();
            this.patientServiceMock.VerifyNoOtherCalls();
            this.fhirReconciliationServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(DependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnGetStructuredRecordIfErrorsAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            string randomString = GetRandomString();
            string inputNhsNumber = randomString;

            var expectedPatientOrchestrationDependencyException =
                new PatientOrchestrationDependencyException(
                    message: "Patient orchestration dependency error occurred, fix the errors and try again.",
                    innerException: dependencyException.InnerException as Xeption);

            this.providerServiceMock.Setup(service =>
                service.RetrieveAllProvidersAsync())
                    .ThrowsAsync(dependencyException);

            // when
            ValueTask<Bundle> retrieveListOfDocumentsToProcessTask =
                this.patientOrchestrationService
                    .GetStructuredRecord(nhsNumber: inputNhsNumber);

            PatientOrchestrationDependencyException
                actualPatientOrchestrationDependencyException =
                    await Assert.ThrowsAsync<PatientOrchestrationDependencyException>(
                        retrieveListOfDocumentsToProcessTask.AsTask);

            // then
            actualPatientOrchestrationDependencyException.Should()
                .BeEquivalentTo(expectedPatientOrchestrationDependencyException);

            this.providerServiceMock.Verify(service =>
                service.RetrieveAllProvidersAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(IsSameExceptionAs(
                    expectedPatientOrchestrationDependencyException))),
                        Times.Once);

            this.providerServiceMock.VerifyNoOtherCalls();
            this.patientServiceMock.VerifyNoOtherCalls();
            this.fhirReconciliationServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnGetStructuredRecordIfErrorsAndLogItAsync()
        {
            // given
            string randomString = GetRandomString();
            string inputNhsNumber = randomString;
            string randomExceptionMessage = GetRandomString();
            Exception serviceException = new Exception(randomExceptionMessage);

            this.providerServiceMock.Setup(service =>
                service.RetrieveAllProvidersAsync())
                    .ThrowsAsync(serviceException);

            var failedPatientOrchestrationException =
                new FailedPatientOrchestrationException(
                    message: "Failed patient orchestration error occurred, please contact support.",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedPatientOrchestrationServiceException =
                new PatientOrchestrationServiceException(
                    message: "Patient orchestration service error occurred, please contact support.",
                    innerException: failedPatientOrchestrationException);

            // when
            ValueTask<Bundle> retrieveListOfDocumentsToProcessTask =
                this.patientOrchestrationService
                    .GetStructuredRecord(nhsNumber: inputNhsNumber);

            PatientOrchestrationServiceException actualPatientOrchestrationServiceException =
                await Assert.ThrowsAsync<PatientOrchestrationServiceException>(
                    retrieveListOfDocumentsToProcessTask.AsTask);

            // then
            actualPatientOrchestrationServiceException.Should()
                .BeEquivalentTo(expectedPatientOrchestrationServiceException);

            this.providerServiceMock.Verify(service =>
                service.RetrieveAllProvidersAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(IsSameExceptionAs(
                    expectedPatientOrchestrationServiceException))),
                        Times.Once);

            this.providerServiceMock.VerifyNoOtherCalls();
            this.patientServiceMock.VerifyNoOtherCalls();
            this.fhirReconciliationServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
