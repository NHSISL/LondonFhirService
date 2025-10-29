// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Coordinations.Patients.Exceptions;
using Moq;
using Xeptions;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Coordinations.Patients
{
    public partial class PatientCoordinationServiceTests
    {
        [Theory]
        [MemberData(nameof(DependencyValidationExceptions))]
        public async Task ShouldThrowDependencyValidationExceptionOnEverythingIfErrorsAndLogItAsync(
            Xeption dependencyValidationException)
        {
            // Given
            string inputId = GetRandomString();

            var expectedPatientCoordinationDependencyValidationException =
                new PatientCoordinationDependencyValidationException(
                    message: "Patient coordination dependency validation error occurred, please try again.",
                    innerException: dependencyValidationException.InnerException as Xeption);

            this.accessOrchestrationServiceMock.Setup(orchestration =>
                    orchestration.ValidateAccess(It.IsAny<string>()))
                    .ThrowsAsync(dependencyValidationException);

            // When
            ValueTask<Bundle> retrieveListOfDocumentsToProcessTask =
                this.patientCoordinationService
                    .Everything(id: inputId);

            PatientCoordinationDependencyValidationException
                actuaPatientCoordinationDependencyValidationException =
                    await Assert.ThrowsAsync<PatientCoordinationDependencyValidationException>(
                        retrieveListOfDocumentsToProcessTask.AsTask);

            // Then
            actuaPatientCoordinationDependencyValidationException.Should()
                .BeEquivalentTo(expectedPatientCoordinationDependencyValidationException);

            this.accessOrchestrationServiceMock.Verify(orchestration =>
                orchestration.ValidateAccess(It.IsAny<string>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                 broker.LogErrorAsync(It.Is(IsSameExceptionAs(
                     expectedPatientCoordinationDependencyValidationException))),
                         Times.Once);

            this.accessOrchestrationServiceMock.VerifyNoOtherCalls();
            this.patientOrchestrationServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
