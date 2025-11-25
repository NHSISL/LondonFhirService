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

namespace LondonFhirService.Api.Tests.Unit.Controllers.Patients.R4
{
    public partial class R4PatientControllerTests
    {
        [Fact]
        public async Task ShouldReturnBundleOnGetRecordAsync()
        {
            // given
            string randomId = GetRandomString();
            string inputId = randomId;
            DateTimeOffset randomInputStart = GetRandomDateTimeOffset();
            DateTimeOffset inputStart = randomInputStart;
            DateTimeOffset randomInputEnd = GetRandomDateTimeOffset();
            DateTimeOffset inputEnd = randomInputEnd;
            string randomInputTypeFilter = GetRandomString();
            string inputTypeFilter = randomInputTypeFilter;
            DateTimeOffset randomInputSince = GetRandomDateTimeOffset();
            DateTimeOffset inputSince = randomInputSince;
            int randomInputCount = GetRandomNumber();
            int inputCount = randomInputCount;
            CancellationToken cancellationToken = CancellationToken.None;

            Parameters inputParameters = CreateRandomParameters(
                start: inputStart,
                end: inputEnd,
                typeFilter: inputTypeFilter,
                since: inputSince,
                count: inputCount);

            Bundle randomBundle = CreateRandomBundle();
            Bundle expectedBundle = randomBundle.DeepClone();
            var expectedObjectResult = new OkObjectResult(expectedBundle);
            var expectedActionResult = new ActionResult<Bundle>(expectedObjectResult);

            this.patientCoordinationServiceMock.Setup(coordination =>
                coordination.EverythingAsync(
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
                await this.patientController.Everything(inputId, inputParameters, cancellationToken);

            // then
            actualActionResult.Result.Should().BeOfType<OkObjectResult>();
            actualActionResult.Should().BeEquivalentTo(expectedActionResult);
            var okResult = actualActionResult.Result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(expectedBundle);

            this.patientCoordinationServiceMock.Verify(coordination =>
                coordination.EverythingAsync(
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
