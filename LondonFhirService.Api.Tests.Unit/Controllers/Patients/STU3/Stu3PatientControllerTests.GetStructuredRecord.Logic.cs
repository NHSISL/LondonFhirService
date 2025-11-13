// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using FluentAssertions;
using Force.DeepCloner;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Api.Tests.Unit.Controllers.Patients.STU3
{
    public partial class Stu3PatientControllerTests
    {
        [Fact]
        public async Task ShouldReturnBundleOnGetStrucuredRecordAsync()
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            DateTimeOffset randomDateOfBirth = GetRandomDateTimeOffset();
            DateTimeOffset inputDateOfBirth = randomDateOfBirth;
            bool inputDemographicsOnly = false;
            bool inputIncludeInactivePatients = false;
            CancellationToken cancellationToken = CancellationToken.None;

            Parameters inputParameters = CreateRandomGetStructuredRecordParameters(
                dateOfBirth: inputDateOfBirth,
                demographicsOnly: inputDemographicsOnly,
                includeInactivePatients: inputIncludeInactivePatients);

            Bundle randomBundle = CreateRandomBundle();
            Bundle expectedBundle = randomBundle.DeepClone();
            var expectedObjectResult = new OkObjectResult(expectedBundle);
            var expectedActionResult = new ActionResult<Bundle>(expectedObjectResult);

            this.patientCoordinationServiceMock.Setup(coordination =>
                coordination.GetStructuredRecord(
                    inputNhsNumber,
                    inputDateOfBirth.DateTime,
                    inputDemographicsOnly,
                    inputIncludeInactivePatients,
                    cancellationToken))
                    .ReturnsAsync(expectedBundle);

            // when
            ActionResult<Bundle> actualActionResult =
                await this.patientController.GetStructuredRecord(inputNhsNumber, inputParameters, cancellationToken);

            // then
            actualActionResult.Result.Should().BeOfType<OkObjectResult>();
            actualActionResult.Should().BeEquivalentTo(expectedActionResult);
            var okResult = actualActionResult.Result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(expectedBundle);

            this.patientCoordinationServiceMock.Verify(coordination =>
                coordination.GetStructuredRecord(
                    inputNhsNumber,
                    inputDateOfBirth.DateTime,
                    inputDemographicsOnly,
                    inputIncludeInactivePatients,
                    cancellationToken),
                    Times.Once);

            this.patientCoordinationServiceMock.VerifyNoOtherCalls();
        }
    }
}
