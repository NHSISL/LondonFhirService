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
using LondonFhirService.Core.Models.Foundations.Patients.Exceptions;
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
        public async Task ShouldThrowDependencyValidationOnGetStructuredRecordAndLogItAsync(
            Xeption dependencyValidationException)
        {
            // given
            List<string> randomProviderNames = new List<string>
            {
                "DDS"
            };

            List<string> inputProviderNames = randomProviderNames.DeepClone();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;

            var expectedPatientServiceDependencyValidationException =
                new PatientServiceDependencyValidationException(
                    message: "Patient service dependency validation error occurred, please try again.",
                    innerException: dependencyValidationException);

            var patientServiceMock = new Mock<Stu3PatientService>(
                this.fhirBroker,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordWithTimeoutAsync(
                    It.IsAny<IFhirProvider>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>()))
                .ThrowsAsync(dependencyValidationException);

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            ValueTask<List<Bundle>> getStructuredRecordTask =
                mockedPatientService.GetStructuredRecordAsync(
                    providerNames: inputProviderNames,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            PatientServiceDependencyValidationException actualPatientServiceDependencyValidationException =
                await Assert.ThrowsAsync<PatientServiceDependencyValidationException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceDependencyValidationException.Should()
                .BeEquivalentTo(expectedPatientServiceDependencyValidationException);

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordWithTimeoutAsync(
                    It.IsAny<IFhirProvider>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>()),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientServiceDependencyValidationException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(DependencyExceptions))]
        public async Task ShouldThrowDependencyOnGetStructuredRecordAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            List<string> randomProviderNames = new List<string>
            {
                "DDS"
            };

            List<string> inputProviderNames = randomProviderNames.DeepClone();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;

            var expectedPatientServiceDependencyException =
                new PatientServiceDependencyException(
                    message: "Patient service dependency error occurred, contact support.",
                    innerException: dependencyException);

            var patientServiceMock = new Mock<Stu3PatientService>(
                this.fhirBroker,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordWithTimeoutAsync(
                    It.IsAny<IFhirProvider>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>()))
                .ThrowsAsync(dependencyException);

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            ValueTask<List<Bundle>> getStructuredRecordTask =
                mockedPatientService.GetStructuredRecordAsync(
                    providerNames: inputProviderNames,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            PatientServiceDependencyException actualPatientServiceDependencyException =
                await Assert.ThrowsAsync<PatientServiceDependencyException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceDependencyException.Should()
                .BeEquivalentTo(expectedPatientServiceDependencyException);

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordWithTimeoutAsync(
                    It.IsAny<IFhirProvider>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>()),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientServiceDependencyException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnGetStructuredRecordIfServiceErrorOccursAndLogItAsync()
        {
            // given
            List<string> randomProviderNames = new List<string>
            {
                "DDS"
            };

            List<string> inputProviderNames = randomProviderNames.DeepClone();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            var serviceException = new Exception();

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
                this.loggingBrokerMock.Object,
                this.patientServiceConfig)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordWithTimeoutAsync(
                    It.IsAny<IFhirProvider>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>()))
                .ThrowsAsync(serviceException);

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            ValueTask<List<Bundle>> getStructuredRecordTask =
                mockedPatientService.GetStructuredRecordAsync(
                    providerNames: inputProviderNames,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            PatientServiceException actualPatientServiceException =
                await Assert.ThrowsAsync<PatientServiceException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceException.Should()
                .BeEquivalentTo(expectedPatientServiceException);

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordWithTimeoutAsync(
                    It.IsAny<IFhirProvider>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>()),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }
    }
}
