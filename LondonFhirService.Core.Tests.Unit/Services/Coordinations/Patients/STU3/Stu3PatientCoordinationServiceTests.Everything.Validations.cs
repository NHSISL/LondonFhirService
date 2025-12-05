// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Coordinations.Patients.Exceptions;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Coordinations.Patients.STU3
{
    public partial class Stu3PatientCoordinationServiceTests
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
                values: "Text is required");

            var expectedPatientCoordinationValidationException =
                new PatientCoordinationValidationException(
                    message: "Patient coordination validation error occurred, please try again.",
                    innerException: invalidArgumentPatientCoordinationException);

            // when
            ValueTask<Bundle> everythingTask =
                this.patientCoordinationService.EverythingAsync(id: invalidId);

            PatientCoordinationValidationException actualPatientCoordinationValidationException =
                await Assert.ThrowsAsync<PatientCoordinationValidationException>(
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
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
        }
    }
}
