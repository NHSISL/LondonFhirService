// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Orchestrations.Patients.Exceptions;
using Moq;
using Xeptions;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Patients.STU3
{
    public partial class Stu3PatientOrchestrationServiceTests
    {
        [Theory]
        [MemberData(nameof(DependencyValidationExceptions))]
        public async Task ShouldThrowDependencyValidationExceptionOnEverythingIfErrorsAndLogItAsync(
            Xeption dependencyValidationException)
        {
            // given
            string randomId = GetRandomString();
            string inputId = randomId;
            DateTimeOffset? inputStart = GetRandomDateTimeOffset();
            DateTimeOffset? inputEnd = GetRandomDateTimeOffset();
            string inputTypeFilter = GetRandomString();
            DateTimeOffset? inputSince = GetRandomDateTimeOffset();
            int? inputCount = GetRandomNumber();
            CancellationToken cancellationToken = CancellationToken.None;
            List<Bundle> randomBundles = CreateRandomBundles();
            Bundle randomBundle = CreateRandomBundle();
            Bundle expectedBundle = randomBundle.DeepClone();
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-Everything";

            string message =
                $"Parameters:  {{ id = \"{inputId}\", start = \"{inputStart}\", " +
                $"end = \"{inputEnd}\", typeFilter = \"{inputTypeFilter}\", " +
                $"since = \"{inputSince}\", count = \"{inputCount}\" }}";

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
                    .EverythingAsync(
                        correlationId,
                        inputId,
                        inputStart,
                        inputEnd,
                        inputTypeFilter,
                        inputSince,
                        inputCount,
                        cancellationToken);

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

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Orchestration Service Request Submitted",
                    message,
                    string.Empty,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Retrieve active providers and execute request",
                    message,
                    string.Empty,
                    correlationId.ToString()),
                        Times.Once);

            this.providerServiceMock.VerifyNoOtherCalls();
            this.patientServiceMock.VerifyNoOtherCalls();
            this.fhirReconciliationServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(DependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnEverythingIfErrorsAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            string randomId = GetRandomString();
            string inputId = randomId;
            DateTimeOffset? inputStart = GetRandomDateTimeOffset();
            DateTimeOffset? inputEnd = GetRandomDateTimeOffset();
            string inputTypeFilter = GetRandomString();
            DateTimeOffset? inputSince = GetRandomDateTimeOffset();
            int? inputCount = GetRandomNumber();
            CancellationToken cancellationToken = CancellationToken.None;
            List<Bundle> randomBundles = CreateRandomBundles();
            Bundle randomBundle = CreateRandomBundle();
            Bundle expectedBundle = randomBundle.DeepClone();
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-Everything";

            string message =
                $"Parameters:  {{ id = \"{inputId}\", start = \"{inputStart}\", " +
                $"end = \"{inputEnd}\", typeFilter = \"{inputTypeFilter}\", " +
                $"since = \"{inputSince}\", count = \"{inputCount}\" }}";

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
                    .EverythingAsync(
                        correlationId,
                        inputId,
                        inputStart,
                        inputEnd,
                        inputTypeFilter,
                        inputSince,
                        inputCount,
                        cancellationToken);

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

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Orchestration Service Request Submitted",
                    message,
                    string.Empty,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Retrieve active providers and execute request",
                    message,
                    string.Empty,
                    correlationId.ToString()),
                        Times.Once);

            this.providerServiceMock.VerifyNoOtherCalls();
            this.patientServiceMock.VerifyNoOtherCalls();
            this.fhirReconciliationServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnEverythingIfErrorsAndLogItAsync()
        {
            // Given
            string randomId = GetRandomString();
            string inputId = randomId;
            DateTimeOffset? inputStart = GetRandomDateTimeOffset();
            DateTimeOffset? inputEnd = GetRandomDateTimeOffset();
            string inputTypeFilter = GetRandomString();
            DateTimeOffset? inputSince = GetRandomDateTimeOffset();
            int? inputCount = GetRandomNumber();
            CancellationToken cancellationToken = CancellationToken.None;
            List<Bundle> randomBundles = CreateRandomBundles();
            Bundle randomBundle = CreateRandomBundle();
            Bundle expectedBundle = randomBundle.DeepClone();
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-Everything";

            string message =
                $"Parameters:  {{ id = \"{inputId}\", start = \"{inputStart}\", " +
                $"end = \"{inputEnd}\", typeFilter = \"{inputTypeFilter}\", " +
                $"since = \"{inputSince}\", count = \"{inputCount}\" }}";

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

            // When
            ValueTask<Bundle> retrieveListOfDocumentsToProcessTask =
                this.patientOrchestrationService
                    .EverythingAsync(
                        correlationId,
                        inputId,
                        inputStart,
                        inputEnd,
                        inputTypeFilter,
                        inputSince,
                        inputCount,
                        cancellationToken);

            PatientOrchestrationServiceException actualPatientOrchestrationServiceException =
                await Assert.ThrowsAsync<PatientOrchestrationServiceException>(
                    retrieveListOfDocumentsToProcessTask.AsTask);

            // Then
            actualPatientOrchestrationServiceException.Should()
                .BeEquivalentTo(expectedPatientOrchestrationServiceException);

            this.providerServiceMock.Verify(service =>
                service.RetrieveAllProvidersAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(IsSameExceptionAs(
                    expectedPatientOrchestrationServiceException))),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Orchestration Service Request Submitted",
                    message,
                    string.Empty,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Retrieve active providers and execute request",
                    message,
                    string.Empty,
                    correlationId.ToString()),
                        Times.Once);

            this.providerServiceMock.VerifyNoOtherCalls();
            this.patientServiceMock.VerifyNoOtherCalls();
            this.fhirReconciliationServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }
    }
}
