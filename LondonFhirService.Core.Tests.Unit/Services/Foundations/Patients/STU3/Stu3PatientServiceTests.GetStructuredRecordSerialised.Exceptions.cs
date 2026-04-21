// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.Patients.Exceptions;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Services.Foundations.Patients.STU3;
using LondonFhirService.Providers.FHIR.STU3.Abstractions;
using Moq;
using Xeptions;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Patients.STU3
{
    public partial class Stu3PatientServiceTests
    {
        [Theory]
        [MemberData(nameof(DependencyValidationExceptions))]
        public async Task ShouldThrowDependencyValidationOnGetStructuredRecordSerialisedAndLogItAsync(
            Xeption dependencyValidationException)
        {
            // given
            Provider ddsProvider =
                new Provider { FriendlyName = "DDS Provider", FullyQualifiedName = "DDS", IsPrimary = true };

            List<Provider> randomProviders = new List<Provider>
            {
                ddsProvider
            };

            List<Provider> inputProviders = randomProviders.DeepClone();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{null}\", " +
                $"demographicsOnly = \"{null}\", " +
                $"includeInactivePatients = \"{null}\" }}";

            var failedPatientDependencyValidationException =
                new FailedPatientDependencyValidationException(
                    message: "Failed patient dependency validation error occurred, please try again.",
                    innerException: dependencyValidationException.InnerException,
                    data: dependencyValidationException.Data);

            var expectedPatientServiceDependencyValidationException =
                new PatientServiceDependencyValidationException(
                    message: "Patient service dependency validation error occurred, please try again.",
                    innerException: failedPatientDependencyValidationException);

            var patientServiceMock = new Mock<Stu3PatientService>(
                this.fhirBroker,
                this.auditBrokerMock.Object,
                this.identifierBrokerMock.Object,
                this.securityAuditBrokerMock.Object,
                this.storageBrokerFactoryMock.Object,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<IFhirProvider>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>()))
                .ThrowsAsync(dependencyValidationException);

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            ValueTask<List<(string Provider, string Json)>> getStructuredRecordTask =
                mockedPatientService.GetStructuredRecordSerialisedAsync(
                    activeProviders: inputProviders,
                    correlationId: correlationId,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            PatientServiceDependencyValidationException actualPatientServiceDependencyValidationException =
                await Assert.ThrowsAsync<PatientServiceDependencyValidationException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceDependencyValidationException.Should()
                .BeEquivalentTo(expectedPatientServiceDependencyValidationException);

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<IFhirProvider>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>()),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientServiceDependencyValidationException))),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Foundation Service Request Submitted",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Parallel Provider Execution Started",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(DependencyExceptions))]
        public async Task ShouldThrowDependencyOnGetStructuredRecordSerialisedAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            Provider ddsProvider =
                new Provider { FriendlyName = "DDS Provider", FullyQualifiedName = "DDS", IsPrimary = true };

            List<Provider> randomProviders = new List<Provider>
            {
                ddsProvider
            };

            List<Provider> inputProviders = randomProviders.DeepClone();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{null}\", " +
                $"demographicsOnly = \"{null}\", " +
                $"includeInactivePatients = \"{null}\" }}";

            var failedPatientDependencyException =
                new FailedPatientDependencyException(
                    message: "Failed patient dependency error occurred, contact support.",
                    innerException: dependencyException.InnerException,
                    data: dependencyException.Data);

            var expectedPatientServiceDependencyException =
                new PatientServiceDependencyException(
                    message: "Patient service dependency error occurred, contact support.",
                    innerException: failedPatientDependencyException);

            var patientServiceMock = new Mock<Stu3PatientService>(
                this.fhirBroker,
                this.auditBrokerMock.Object,
                this.identifierBrokerMock.Object,
                this.securityAuditBrokerMock.Object,
                this.storageBrokerFactoryMock.Object,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<IFhirProvider>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>()))
                .ThrowsAsync(dependencyException);

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            ValueTask<List<(string Provider, string Json)>> getStructuredRecordTask =
                mockedPatientService.GetStructuredRecordSerialisedAsync(
                    activeProviders: inputProviders,
                    correlationId: correlationId,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            PatientServiceDependencyException actualPatientServiceDependencyException =
                await Assert.ThrowsAsync<PatientServiceDependencyException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceDependencyException.Should()
                .BeEquivalentTo(expectedPatientServiceDependencyException);

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<IFhirProvider>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>()),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientServiceDependencyException))),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Foundation Service Request Submitted",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Parallel Provider Execution Started",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task
            ShouldThrowServiceExceptionOnGetStructuredRecordSerialisedIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Provider ddsProvider =
                new Provider { FriendlyName = "DDS Provider", FullyQualifiedName = "DDS", IsPrimary = true };

            List<Provider> randomProviders = new List<Provider>
            {
                ddsProvider
            };

            List<Provider> inputProviders = randomProviders.DeepClone();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            Guid correlationId = Guid.NewGuid();
            var serviceException = new Exception();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{null}\", " +
                $"demographicsOnly = \"{null}\", " +
                $"includeInactivePatients = \"{null}\" }}";

            var failedPatientServiceException =
                new FailedPatientServiceException(
                    message: "Failed patient service error occurred, please contact support.",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedPatientServiceException =
                new PatientServiceException(
                    message: "Patient service error occurred, contact support.",
                    innerException: failedPatientServiceException);

            var patientServiceMock = new Mock<Stu3PatientService>(
                this.fhirBroker,
                this.auditBrokerMock.Object,
                this.identifierBrokerMock.Object,
                this.securityAuditBrokerMock.Object,
                this.storageBrokerFactoryMock.Object,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<IFhirProvider>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>()))
                .ThrowsAsync(serviceException);

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            ValueTask<List<(string Provider, string Json)>> getStructuredRecordTask =
                mockedPatientService.GetStructuredRecordSerialisedAsync(
                    activeProviders: inputProviders,
                    correlationId: correlationId,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            PatientServiceException actualPatientServiceException =
                await Assert.ThrowsAsync<PatientServiceException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceException.Should()
                .BeEquivalentTo(expectedPatientServiceException);

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<IFhirProvider>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>()),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientServiceException))),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Foundation Service Request Submitted",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Parallel Provider Execution Started",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(CancelledExceptions))]
        public async Task ShouldThrowDependencyCancellationOnGetStructuredRecordSerialisedAndLogItAsync(
            Xeption cancelledProviderException)
        {
            // given
            Provider ddsProvider =
                new Provider { FriendlyName = "DDS Provider", FullyQualifiedName = "DDS", IsPrimary = true };

            List<Provider> randomProviders = new List<Provider>
            {
                ddsProvider
            };

            List<Provider> inputProviders = randomProviders.DeepClone();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{null}\", " +
                $"demographicsOnly = \"{null}\", " +
                $"includeInactivePatients = \"{null}\" }}";

            var cancelledInnerException =
                (OperationCanceledException)cancelledProviderException.InnerException.InnerException;

            var cancelledPatientServiceException =
                new CancelledPatientServiceException(
                    message: "Patient service was cancelled, please try again.",
                    innerException: cancelledInnerException,
                    data: cancelledInnerException.Data);

            var expectedPatientServiceDependencyException =
                new PatientServiceDependencyException(
                    message: "Patient service dependency error occurred, contact support.",
                    innerException: cancelledPatientServiceException);

            var patientServiceMock = new Mock<Stu3PatientService>(
                this.fhirBroker,
                this.auditBrokerMock.Object,
                this.identifierBrokerMock.Object,
                this.securityAuditBrokerMock.Object,
                this.storageBrokerFactoryMock.Object,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<IFhirProvider>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>()))
                .ThrowsAsync(cancelledProviderException);

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            ValueTask<List<(string Provider, string Json)>> getStructuredRecordTask =
                mockedPatientService.GetStructuredRecordSerialisedAsync(
                    activeProviders: inputProviders,
                    correlationId: correlationId,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            PatientServiceDependencyException
                actualPatientServiceDependencyException =
                    await Assert.ThrowsAsync<PatientServiceDependencyException>(
                        testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceDependencyException.Should()
                .BeEquivalentTo(expectedPatientServiceDependencyException);

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<IFhirProvider>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>()),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientServiceDependencyException))),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Foundation Service Request Submitted",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Parallel Provider Execution Started",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(NetworkExceptions))]
        public async Task
            ShouldThrowServiceExceptionOnGetStructuredRecordSerialisedOnNetworkFailureAndLogItAsync(
                Xeption networkProviderException)
        {
            // given
            Provider ddsProvider =
                new Provider { FriendlyName = "DDS Provider", FullyQualifiedName = "DDS", IsPrimary = true };

            List<Provider> randomProviders = new List<Provider>
            {
                ddsProvider
            };

            List<Provider> inputProviders = randomProviders.DeepClone();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{null}\", " +
                $"demographicsOnly = \"{null}\", " +
                $"includeInactivePatients = \"{null}\" }}";

            var networkOperationCanceledException =
                (OperationCanceledException)networkProviderException.InnerException.InnerException;

            var networkPatientServiceException =
                new NetworkPatientServiceException(
                    message: "Network connectivity failure occurred, please check connection and try again.",
                    innerException: networkOperationCanceledException,
                    data: networkOperationCanceledException.Data);

            var expectedPatientServiceException =
                new PatientServiceException(
                    message: "Patient service error occurred, contact support.",
                    innerException: networkPatientServiceException);

            var patientServiceMock = new Mock<Stu3PatientService>(
                this.fhirBroker,
                this.auditBrokerMock.Object,
                this.identifierBrokerMock.Object,
                this.securityAuditBrokerMock.Object,
                this.storageBrokerFactoryMock.Object,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<IFhirProvider>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>()))
                .ThrowsAsync(networkProviderException);

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            ValueTask<List<(string Provider, string Json)>> getStructuredRecordTask =
                mockedPatientService.GetStructuredRecordSerialisedAsync(
                    activeProviders: inputProviders,
                    correlationId: correlationId,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            PatientServiceException actualPatientServiceException =
                await Assert.ThrowsAsync<PatientServiceException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceException.Should()
                .BeEquivalentTo(expectedPatientServiceException);

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<IFhirProvider>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>()),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientServiceException))),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Foundation Service Request Submitted",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Parallel Provider Execution Started",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }
    }
}
