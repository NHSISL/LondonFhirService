// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Coordinations.Patients.Exceptions;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Coordinations.Patients
{
    public partial class PatientCoordinationServiceTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnEverythingIfIdIsNullAndLogItAsync(string invalidText)
        {
            // given
            string invalidId = invalidText;

            var invalidArgumentPatientCoordinationException =
                new InvalidArgumentPatientCoordinationException(
                    message: "Invalid patient coordination argument, please correct the errors and try again.");

            invalidArgumentPatientCoordinationException.AddData(
                key: "Id",
                values: "Id is required");

            var expectedPatientCoordinationValidationException =
                new PatentCoordinationValidationException(
                    message: "Patient coordination validation error occurred, please try again.",
                    innerException: invalidArgumentPatientCoordinationException);

            // when
            ValueTask<Bundle> everythingTask =
                this.patientCoordinationService.Everything(id: invalidId);

            PatentCoordinationValidationException actualPatientCoordinationValidationException =
                await Assert.ThrowsAsync<PatentCoordinationValidationException>(
                    everythingTask.AsTask);

            // then
            actualPatientCoordinationValidationException.Should()
                .BeEquivalentTo(expectedPatientCoordinationValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientCoordinationValidationException))),
                        Times.Once);

            this.accessOrchestrationServiceMock.VerifyNoOtherCalls();
            this.patientOrchestrationServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
