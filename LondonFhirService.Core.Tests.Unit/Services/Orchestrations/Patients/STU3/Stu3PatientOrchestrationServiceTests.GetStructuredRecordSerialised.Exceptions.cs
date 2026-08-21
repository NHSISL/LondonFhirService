// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Models.Orchestrations.Accesses;
using LondonFhirService.Core.Models.Orchestrations.Patients;
using LondonFhirService.Core.Models.Orchestrations.Patients.Exceptions;
using LondonFhirService.Core.Services.Orchestrations.Patients.STU3;
using Moq;
using Xeptions;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Patients.STU3
{
    public partial class Stu3PatientOrchestrationServiceTests
    {
        [Theory]
        [MemberData(nameof(DependencyValidationExceptions))]
        public async Task ShouldThrowDependencyValidationExceptionOnGetStructuredRecordIfErrorsAndLogItAsync(
            Xeption dependencyValidationException)
        {
            // given
            string randomId = GetRandomString();
            string inputNhsNumber = randomId;
            string inputDateOfBirth = DateTime.Now.ToString("yyyy-MM-dd");
            bool? inputDemographicsOnly = false;
            bool? inputActivePatientsOnly = true;
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{inputDateOfBirth}\", " +
                $"demographicsOnly = \"{inputDemographicsOnly}\", " +
                $"includeInactivePatients = \"{inputActivePatientsOnly}\" }}";

            string accessMessage = $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\" }}";

            Stu3PatientOrchestrationService orchestrationService =
                CreateOrchestrationService(new AccessConfigurations { CheckAccessPermissions = false });

            var expectedPatientOrchestrationDependencyValidationException =
                new PatientOrchestrationDependencyValidationException(
                    message: "Patient orchestration dependency validation error occurred, please try again.",
                    innerException: dependencyValidationException.InnerException as Xeption);

            this.providerServiceMock.Setup(service =>
                service.RetrieveAllProvidersAsync())
                    .ThrowsAsync(dependencyValidationException);

            // when
            ValueTask<StructuredRecordsResponse> getStructuredRecordSerialisedTask =
                    orchestrationService.GetStructuredRecordSerialisedAsync(
                        correlationId,
                        inputNhsNumber,
                        inputDateOfBirth,
                        inputDemographicsOnly,
                        inputActivePatientsOnly,
                        cancellationToken: cancellationToken);

            PatientOrchestrationDependencyValidationException
                actualPatientOrchestrationDependencyValidationException =
                    await Assert.ThrowsAsync<PatientOrchestrationDependencyValidationException>(
                        getStructuredRecordSerialisedTask.AsTask);

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

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Orchestration Service Request Submitted",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Access permission check skipped due to configuration (CheckAccessPermissions = false)",
                    accessMessage,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Retrieve active providers and execute request",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            AcceptMetricSpans();
            this.providerServiceMock.VerifyNoOtherCalls();
            this.patientServiceMock.VerifyNoOtherCalls();
            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditAndMetricBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(DependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnGetStructuredRecordIfErrorsAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            string randomId = GetRandomString();
            string inputNhsNumber = randomId;
            string inputDateOfBirth = DateTime.Now.ToString("yyyy-MM-dd");
            bool? inputDemographicsOnly = false;
            bool? inputActivePatientsOnly = true;
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{inputDateOfBirth}\", " +
                $"demographicsOnly = \"{inputDemographicsOnly}\", " +
                $"includeInactivePatients = \"{inputActivePatientsOnly}\" }}";

            string accessMessage = $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\" }}";

            Stu3PatientOrchestrationService orchestrationService =
                CreateOrchestrationService(new AccessConfigurations { CheckAccessPermissions = false });

            var expectedPatientOrchestrationDependencyException =
                new PatientOrchestrationDependencyException(
                    message: "Patient orchestration dependency error occurred, fix the errors and try again.",
                    innerException: dependencyException.InnerException as Xeption);

            this.providerServiceMock.Setup(service =>
                service.RetrieveAllProvidersAsync())
                    .ThrowsAsync(dependencyException);

            // when
            ValueTask<StructuredRecordsResponse> getStructuredRecordSerialisedTask =
                    orchestrationService.GetStructuredRecordSerialisedAsync(
                        correlationId,
                        inputNhsNumber,
                        inputDateOfBirth,
                        inputDemographicsOnly,
                        inputActivePatientsOnly,
                        cancellationToken: cancellationToken);

            PatientOrchestrationDependencyException
                actualPatientOrchestrationDependencyException =
                    await Assert.ThrowsAsync<PatientOrchestrationDependencyException>(
                        getStructuredRecordSerialisedTask.AsTask);

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

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Orchestration Service Request Submitted",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Access permission check skipped due to configuration (CheckAccessPermissions = false)",
                    accessMessage,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Retrieve active providers and execute request",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            AcceptMetricSpans();
            this.providerServiceMock.VerifyNoOtherCalls();
            this.patientServiceMock.VerifyNoOtherCalls();
            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditAndMetricBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(ConsumerAccessDependencyValidationExceptions))]
        public async Task
            ShouldThrowDependencyValidationExceptionOnGetStructuredRecordIfAccessCheckErrorsAndLogItAsync(
                Xeption dependencyValidationException)
        {
            // given
            string randomId = GetRandomString();
            string inputNhsNumber = randomId;
            string inputDateOfBirth = DateTime.Now.ToString("yyyy-MM-dd");
            bool? inputDemographicsOnly = false;
            bool? inputActivePatientsOnly = true;
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            string userId = GetRandomString();

            var expectedPatientOrchestrationDependencyValidationException =
                new PatientOrchestrationDependencyValidationException(
                    message: "Patient orchestration dependency validation error occurred, please try again.",
                    innerException: dependencyValidationException.InnerException as Xeption);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(CreateRandomUser(userId));

            this.consumerAccessServiceMock.Setup(service =>
                service.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(dependencyValidationException);

            // when
            ValueTask<StructuredRecordsResponse> getStructuredRecordSerialisedTask =
                    this.patientOrchestrationService.GetStructuredRecordSerialisedAsync(
                        correlationId,
                        inputNhsNumber,
                        inputDateOfBirth,
                        inputDemographicsOnly,
                        inputActivePatientsOnly,
                        cancellationToken: cancellationToken);

            PatientOrchestrationDependencyValidationException
                actualPatientOrchestrationDependencyValidationException =
                    await Assert.ThrowsAsync<PatientOrchestrationDependencyValidationException>(
                        getStructuredRecordSerialisedTask.AsTask);

            // then
            actualPatientOrchestrationDependencyValidationException.Should()
                .BeEquivalentTo(expectedPatientOrchestrationDependencyValidationException);

            this.consumerAccessServiceMock.Verify(service =>
                service.CheckConsumerAccessAsync(
                    It.Is(SameValidateAccessRequestAs(userId, inputNhsNumber, correlationId)),
                    default),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(IsSameExceptionAs(
                    expectedPatientOrchestrationDependencyValidationException))),
                        Times.Once);

            this.providerServiceMock.Verify(service =>
                service.RetrieveAllProvidersAsync(),
                    Times.Never);

            AcceptMetricSpans();
            this.providerServiceMock.VerifyNoOtherCalls();
            this.patientServiceMock.VerifyNoOtherCalls();
            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(ConsumerAccessDependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnGetStructuredRecordIfAccessCheckErrorsAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            string randomId = GetRandomString();
            string inputNhsNumber = randomId;
            string inputDateOfBirth = DateTime.Now.ToString("yyyy-MM-dd");
            bool? inputDemographicsOnly = false;
            bool? inputActivePatientsOnly = true;
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            string userId = GetRandomString();

            var expectedPatientOrchestrationDependencyException =
                new PatientOrchestrationDependencyException(
                    message: "Patient orchestration dependency error occurred, fix the errors and try again.",
                    innerException: dependencyException.InnerException as Xeption);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(CreateRandomUser(userId));

            this.consumerAccessServiceMock.Setup(service =>
                service.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(dependencyException);

            // when
            ValueTask<StructuredRecordsResponse> getStructuredRecordSerialisedTask =
                    this.patientOrchestrationService.GetStructuredRecordSerialisedAsync(
                        correlationId,
                        inputNhsNumber,
                        inputDateOfBirth,
                        inputDemographicsOnly,
                        inputActivePatientsOnly,
                        cancellationToken: cancellationToken);

            PatientOrchestrationDependencyException actualPatientOrchestrationDependencyException =
                await Assert.ThrowsAsync<PatientOrchestrationDependencyException>(
                    getStructuredRecordSerialisedTask.AsTask);

            // then
            actualPatientOrchestrationDependencyException.Should()
                .BeEquivalentTo(expectedPatientOrchestrationDependencyException);

            this.consumerAccessServiceMock.Verify(service =>
                service.CheckConsumerAccessAsync(
                    It.Is(SameValidateAccessRequestAs(userId, inputNhsNumber, correlationId)),
                    default),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(IsSameExceptionAs(
                    expectedPatientOrchestrationDependencyException))),
                        Times.Once);

            this.providerServiceMock.Verify(service =>
                service.RetrieveAllProvidersAsync(),
                    Times.Never);

            AcceptMetricSpans();
            this.providerServiceMock.VerifyNoOtherCalls();
            this.patientServiceMock.VerifyNoOtherCalls();
            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnGetStructuredRecordIfErrorsAndLogItAsync()
        {
            // given
            string randomId = GetRandomString();
            string inputNhsNumber = randomId;
            string inputDateOfBirth = DateTime.Now.ToString("yyyy-MM-dd");
            bool? inputDemographicsOnly = false;
            bool? inputActivePatientsOnly = true;
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{inputDateOfBirth}\", " +
                $"demographicsOnly = \"{inputDemographicsOnly}\", " +
                $"includeInactivePatients = \"{inputActivePatientsOnly}\" }}";

            string accessMessage = $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\" }}";

            Stu3PatientOrchestrationService orchestrationService =
                CreateOrchestrationService(new AccessConfigurations { CheckAccessPermissions = false });

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
            ValueTask<StructuredRecordsResponse> getStructuredRecordSerialisedTask =
                    orchestrationService.GetStructuredRecordSerialisedAsync(
                        correlationId,
                        inputNhsNumber,
                        inputDateOfBirth,
                        inputDemographicsOnly,
                        inputActivePatientsOnly,
                        cancellationToken: cancellationToken);

            PatientOrchestrationServiceException actualPatientOrchestrationServiceException =
                await Assert.ThrowsAsync<PatientOrchestrationServiceException>(
                    getStructuredRecordSerialisedTask.AsTask);

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

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Orchestration Service Request Submitted",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Access permission check skipped due to configuration (CheckAccessPermissions = false)",
                    accessMessage,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Retrieve active providers and execute request",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            AcceptMetricSpans();
            this.providerServiceMock.VerifyNoOtherCalls();
            this.patientServiceMock.VerifyNoOtherCalls();
            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditAndMetricBrokerMock.VerifyNoOtherCalls();
        }
    }
}
