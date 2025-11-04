// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Foundations.Patients.Exceptions;
using LondonFhirService.Core.Services.Foundations.Patients.STU3;
using Moq;
using Xeptions;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Patients.STU3
{
    public partial class Stu3PatientServiceTests
    {
        [Theory]
        [MemberData(nameof(DependencyValidationExceptions))]
        public async Task ShouldThrowDependencyValidationOnEverythingAndLogItAsync(
            Xeption dependencyValidationException)
        {
            // given
            List<string> randomProviderNames = new List<string>
            {
                "DDS"
            };

            List<string> inputProviderNames = randomProviderNames.DeepClone();
            string randomId = GetRandomString();
            string inputId = randomId;

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
                service.ExecuteWithTimeoutAsync(
                    ddsFhirProviderMock.Object,
                    default,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ThrowsAsync(dependencyValidationException);

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            ValueTask<List<Bundle>> everythingTask =
                mockedPatientService.Everything(
                    providerNames: inputProviderNames,
                    id: inputId,
                    cancellationToken: default);

            PatientServiceDependencyValidationException actualPatientServiceDependencyValidationException =
                await Assert.ThrowsAsync<PatientServiceDependencyValidationException>(
                    testCode: everythingTask.AsTask);

            // then
            actualPatientServiceDependencyValidationException.Should()
                .BeEquivalentTo(expectedPatientServiceDependencyValidationException);

            patientServiceMock.Verify(service =>
                service.ExecuteWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object,
                    default,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null),
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
        public async Task ShouldThrowDependencyOnEverythingAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            List<string> randomProviderNames = new List<string>
            {
                "DDS"
            };

            List<string> inputProviderNames = randomProviderNames.DeepClone();
            string randomId = GetRandomString();
            string inputId = randomId;

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
                service.ExecuteWithTimeoutAsync(
                    ddsFhirProviderMock.Object,
                    default,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ThrowsAsync(dependencyException);

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            ValueTask<List<Bundle>> everythingTask =
                mockedPatientService.Everything(
                    providerNames: inputProviderNames,
                    id: inputId,
                    cancellationToken: default);

            PatientServiceDependencyException actualPatientServiceDependencyException =
                await Assert.ThrowsAsync<PatientServiceDependencyException>(
                    testCode: everythingTask.AsTask);

            // then
            actualPatientServiceDependencyException.Should()
                .BeEquivalentTo(expectedPatientServiceDependencyException);

            patientServiceMock.Verify(service =>
                service.ExecuteWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object,
                    default,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientServiceDependencyException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnEverythingIfServiceErrorOccursAndLogItAsync()
        {
            // given
            List<string> randomProviderNames = new List<string>
            {
                "DDS"
            };

            List<string> inputProviderNames = randomProviderNames.DeepClone();
            string randomId = GetRandomString();
            string inputId = randomId;

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
                service.ExecuteWithTimeoutAsync(
                    ddsFhirProviderMock.Object,
                    default,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ThrowsAsync(serviceException);

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            ValueTask<List<Bundle>> everythingTask =
                mockedPatientService.Everything(
                    providerNames: inputProviderNames,
                    id: inputId,
                    cancellationToken: default);

            PatientServiceException actualPatientServiceException =
                await Assert.ThrowsAsync<PatientServiceException>(
                    testCode: everythingTask.AsTask);

            // then
            actualPatientServiceException.Should()
                .BeEquivalentTo(expectedPatientServiceException);

            patientServiceMock.Verify(service =>
                service.ExecuteWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object,
                    default,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null),
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
