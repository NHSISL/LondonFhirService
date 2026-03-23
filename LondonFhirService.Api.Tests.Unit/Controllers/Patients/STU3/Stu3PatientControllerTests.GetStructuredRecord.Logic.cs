// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using FluentAssertions;
using Force.DeepCloner;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Api.Tests.Unit.Controllers.Patients.STU3
{
    public partial class Stu3PatientControllerTests
    {
        [Fact]
        public async Task ShouldReturnBundleOnGetStructuredRecordAsync()
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            DateTimeOffset randomDateOfBirth = GetRandomDateTimeOffset();
            string inputDateOfBirth = randomDateOfBirth.ToString("yyyy-MM-dd");
            bool inputDemographicsOnly = false;
            bool inputIncludeInactivePatients = false;
            CancellationToken cancellationToken = CancellationToken.None;

            Parameters inputParameters = CreateRandomGetStructuredRecordParameters(
                nhsNumber: inputNhsNumber,
                dateOfBirth: inputDateOfBirth,
                demographicsOnly: inputDemographicsOnly,
                includeInactivePatients: inputIncludeInactivePatients);

            Bundle randomBundle = CreateRandomBundle();
            Bundle expectedBundle = randomBundle.DeepClone();
            string rawExpectedBundle = fhirJsonSerializer.SerializeToString(expectedBundle);
            var expectedObjectResult = new OkObjectResult(rawExpectedBundle);
            var expectedActionResult = new ActionResult<Bundle>(expectedObjectResult);

            this.patientCoordinationServiceMock.Setup(coordination =>
                coordination.GetStructuredRecordSerialisedAsync(
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputIncludeInactivePatients,
                    cancellationToken))
                    .ReturnsAsync(rawExpectedBundle);

            // when
            ActionResult<Bundle> actualActionResult =
                await this.patientController.GetStructuredRecord(inputParameters, cancellationToken);

            // then
            actualActionResult.Result.Should().BeOfType<ContentResult>();
            actualActionResult.Should().BeEquivalentTo(expectedActionResult);
            var contentResult = actualActionResult.Result as ContentResult;
            contentResult.Content.Should().BeEquivalentTo(rawExpectedBundle);

            this.patientCoordinationServiceMock.Verify(coordination =>
                coordination.GetStructuredRecordSerialisedAsync(
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputIncludeInactivePatients,
                    cancellationToken),
                    Times.Once);

            this.patientCoordinationServiceMock.VerifyNoOtherCalls();
        }
    }
}
