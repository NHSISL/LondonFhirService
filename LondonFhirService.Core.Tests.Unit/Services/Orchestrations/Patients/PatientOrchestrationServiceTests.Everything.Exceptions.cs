﻿// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Orchestrations.Patients.Exceptions;
using Moq;
using Xeptions;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Patients
{
    public partial class PatientOrchestrationServiceTests
    {
        [Theory]
        [MemberData(nameof(DependencyValidationExceptions))]
        public async Task ShouldThrowDependencyValidationExceptionOnEverythingIfErrorsAndLogItAsync(
            Xeption dependencyValidationException)
        {
            // given
            string randomString = GetRandomString();
            string inputId = randomString;

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
                    .Everything(id: inputId);

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
        public async Task ShouldThrowDependencyExceptionOnEverythingIfErrorsAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            string randomString = GetRandomString();
            string inputId = randomString;

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
                    .Everything(id: inputId);

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
    }
}
