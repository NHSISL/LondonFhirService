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
            string rawExpectedBundle = fhirJsonSerializer.SerializeToString(expectedBundle);
            var expectedObjectResult = new OkObjectResult(rawExpectedBundle);
            var expectedActionResult = new ActionResult<string>(expectedObjectResult);

            this.patientCoordinationServiceMock.Setup(coordination =>
                coordination.EverythingSerialisedAsync(
                    inputId,
                    inputStart,
                    inputEnd,
                    inputTypeFilter,
                    inputSince,
                    inputCount,
                    cancellationToken))
                    .ReturnsAsync(rawExpectedBundle);

            // when
            ActionResult<string> actualActionResult =
                await this.patientController.Everything(inputId, inputParameters, cancellationToken);

            // then
            actualActionResult.Result.Should().BeOfType<OkObjectResult>();
            actualActionResult.Should().BeEquivalentTo(expectedActionResult);
            var okResult = actualActionResult.Result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(rawExpectedBundle);

            this.patientCoordinationServiceMock.Verify(coordination =>
                coordination.EverythingSerialisedAsync(
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
