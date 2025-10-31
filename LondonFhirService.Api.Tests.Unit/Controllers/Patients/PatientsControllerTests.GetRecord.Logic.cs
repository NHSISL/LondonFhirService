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

namespace LondonFhirService.Api.Tests.Unit.Controllers.Patients
{
    public partial class PatientsControllerTests
    {
        [Fact]
        public async Task ShouldReturnBundleOnGetRecordAsync()
        {
            // given
            string randomId = GetRandomString();
            string inputId = randomId;
            DateTimeOffset? inputStart = GetRandomDateTimeOffset();
            DateTimeOffset? inputEnd = GetRandomDateTimeOffset();
            string inputTypeFilter = GetRandomString();
            DateTimeOffset? inputSince = GetRandomDateTimeOffset();
            int? inputCount = GetRandomNumber();
            CancellationToken cancellationToken = CancellationToken.None;
            Bundle randomBundle = CreateRandomBundle();
            Bundle expectedBundle = randomBundle.DeepClone();
            var expectedObjectResult = new OkObjectResult(expectedBundle);
            var expectedActionResult = new ActionResult<Bundle>(expectedObjectResult);

            this.patientCoordinationServiceMock.Setup(coordination =>
                coordination.Everything(
                    inputId,
                    inputStart,
                    inputEnd,
                    inputTypeFilter,
                    inputSince,
                    inputCount,
                    cancellationToken))
                    .ReturnsAsync(expectedBundle);

            // when
            ActionResult<Bundle> actualActionResult =
                await this.patientsController.GetRecord(
                    inputId,
                    inputStart,
                    inputEnd,
                    inputTypeFilter,
                    inputSince,
                    inputCount,
                    cancellationToken);

            // then
            actualActionResult.Result.Should().BeOfType<OkObjectResult>();
            actualActionResult.Should().BeEquivalentTo(expectedActionResult);
            var okResult = actualActionResult.Result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(expectedBundle);

            this.patientCoordinationServiceMock.Verify(coordination =>
                coordination.Everything(
                    inputId,
                    inputStart,
                    inputEnd,
                    inputTypeFilter,
                    inputSince,
                    inputCount,
                    cancellationToken),
                    Times.Once);

            this.patientCoordinationServiceMock.VerifyNoOtherCalls();
        }
    }
}
