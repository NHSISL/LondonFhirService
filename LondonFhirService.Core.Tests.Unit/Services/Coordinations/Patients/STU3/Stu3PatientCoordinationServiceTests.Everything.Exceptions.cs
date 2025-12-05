// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Coordinations.Patients.Exceptions;
using Moq;
using Xeptions;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Coordinations.Patients.STU3
{
    public partial class Stu3PatientCoordinationServiceTests
    {
        [Theory]
        [MemberData(nameof(DependencyValidationExceptions))]
        public async Task ShouldThrowDependencyValidationExceptionOnEverythingIfErrorsAndLogItAsync(
            Xeption dependencyValidationException)
        {
            // given
            string randomString = GetRandomString();
            string inputId = randomString;

            var expectedPatientCoordinationDependencyValidationException =
                new PatientCoordinationDependencyValidationException(
                    message: "Patient coordination dependency validation error occurred, please try again.",
                    innerException: dependencyValidationException.InnerException as Xeption);

            this.identifierBrokerMock.Setup(broker =>
                broker.GetIdentifierAsync())
                    .ThrowsAsync(dependencyValidationException);

            // when
            ValueTask<Bundle> retrieveListOfDocumentsToProcessTask =
                this.patientCoordinationService
                    .EverythingAsync(id: inputId);

            PatientCoordinationDependencyValidationException
                actualPatientCoordinationDependencyValidationException =
                    await Assert.ThrowsAsync<PatientCoordinationDependencyValidationException>(
                        retrieveListOfDocumentsToProcessTask.AsTask);

            // then
            actualPatientCoordinationDependencyValidationException.Should()
                .BeEquivalentTo(expectedPatientCoordinationDependencyValidationException);

            this.identifierBrokerMock.Verify(broker =>
                broker.GetIdentifierAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(IsSameExceptionAs(
                    expectedPatientCoordinationDependencyValidationException))),
                        Times.Once);

            this.accessOrchestrationServiceMock.VerifyNoOtherCalls();
            this.patientOrchestrationServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(DependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnEverythingIfErrorsAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            string randomString = GetRandomString();
            string inputId = randomString;
            Guid correlationId = Guid.NewGuid();

            var expectedPatientCoordinationDependencyException =
                new PatientCoordinationDependencyException(
                    message: "Patient coordination dependency error occurred, fix the errors and try again.",
                    innerException: dependencyException.InnerException as Xeption);

            this.identifierBrokerMock.Setup(broker =>
                broker.GetIdentifierAsync())
                    .ThrowsAsync(dependencyException);

            // when
            ValueTask<Bundle> retrieveListOfDocumentsToProcessTask =
                this.patientCoordinationService
                    .EverythingAsync(id: inputId);

            PatientCoordinationDependencyException actualPatientCoordinationDependencyException =
                await Assert.ThrowsAsync<PatientCoordinationDependencyException>(
                    retrieveListOfDocumentsToProcessTask.AsTask);

            // then
            actualPatientCoordinationDependencyException.Should()
                .BeEquivalentTo(expectedPatientCoordinationDependencyException);

            this.identifierBrokerMock.Verify(broker =>
                broker.GetIdentifierAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(IsSameExceptionAs(
                    expectedPatientCoordinationDependencyException))),
                        Times.Once);

            this.accessOrchestrationServiceMock.VerifyNoOtherCalls();
            this.patientOrchestrationServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnEverythingIfErrorsAndLogItAsync()
        {
            // Given
            string randomString = GetRandomString();
            string inputId = randomString;
            string randomExceptionMessage = GetRandomString();
            Guid correlationId = Guid.NewGuid();
            Exception serviceException = new Exception(randomExceptionMessage);

            this.identifierBrokerMock.Setup(broker =>
                broker.GetIdentifierAsync())
                    .ThrowsAsync(serviceException);

            var failedPatientCoordinationServiceException =
                new FailedPatientCoordinationException(
                    message: "Failed patient coordination service error occurred, please contact support.",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedPatientCoordinationServiceException =
                new PatientCoordinationServiceException(
                    message: "Patient coordination service error occurred, please contact support.",
                    innerException: failedPatientCoordinationServiceException);

            // When
            ValueTask<Bundle> retrieveListOfDocumentsToProcessTask =
                this.patientCoordinationService
                    .EverythingAsync(id: inputId);

            PatientCoordinationServiceException actualPatientCoordinationServiceException =
                await Assert.ThrowsAsync<PatientCoordinationServiceException>(
                    retrieveListOfDocumentsToProcessTask.AsTask);

            // Then
            actualPatientCoordinationServiceException.Should()
                .BeEquivalentTo(expectedPatientCoordinationServiceException);

            this.identifierBrokerMock.Verify(broker =>
                broker.GetIdentifierAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(IsSameExceptionAs(
                    expectedPatientCoordinationServiceException))),
                        Times.Once);

            this.accessOrchestrationServiceMock.VerifyNoOtherCalls();
            this.patientOrchestrationServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
        }
    }
}
