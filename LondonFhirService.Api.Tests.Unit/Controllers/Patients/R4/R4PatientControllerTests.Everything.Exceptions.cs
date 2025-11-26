// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using FluentAssertions;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Models;
using Xeptions;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Api.Tests.Unit.Controllers.Patients.R4
{
    public partial class R4PatientControllerTests
    {
        [Theory]
        [MemberData(nameof(ValidationExceptions))]
        public async Task ShouldReturnBadRequestOnEverythingIfValidationErrorOccurredAsync(
            Xeption validationException)
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

            Parameters randomParameters = CreateRandomParameters(
                start: inputStart,
                end: inputEnd,
                typeFilter: inputTypeFilter,
                since: inputSince,
                count: inputCount);

            Parameters inputParameters = randomParameters;

            BadRequestObjectResult expectedBadRequestObjectResult =
                BadRequest(validationException.InnerException);

            var expectedActionResult = new ActionResult<Bundle>(expectedBadRequestObjectResult);

            this.patientCoordinationServiceMock.Setup(coordination =>
                coordination.EverythingAsync(
                    inputId,
                    inputStart,
                    inputEnd,
                    inputTypeFilter,
                    inputSince,
                    inputCount,
                    cancellationToken))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<Bundle> actualActionResult =
                await this.patientController.Everything(inputId, inputParameters, cancellationToken);

            // then
            actualActionResult.Should().BeEquivalentTo(expectedActionResult);

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

        [Theory]
        [MemberData(nameof(ServerExceptions))]
        public async Task ShouldReturnInternalServerErrorOnEverythingIfServerErrorOccurredAsync(
            Xeption serverException)
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

            Parameters randomParameters = CreateRandomParameters(
                start: inputStart,
                end: inputEnd,
                typeFilter: inputTypeFilter,
                since: inputSince,
                count: inputCount);

            Parameters inputParameters = randomParameters;

            InternalServerErrorObjectResult expectedInternalServerErrorObjectResult =
                InternalServerError(serverException);

            var expectedActionResult = new ActionResult<Bundle>(expectedInternalServerErrorObjectResult);

            this.patientCoordinationServiceMock.Setup(coordination =>
                coordination.EverythingAsync(
                    inputId,
                    inputStart,
                    inputEnd,
                    inputTypeFilter,
                    inputSince,
                    inputCount,
                    cancellationToken))
                    .ThrowsAsync(serverException);

            // when
            ActionResult<Bundle> actualActionResult =
                await this.patientController.Everything(inputId, inputParameters, cancellationToken);

            // then
            actualActionResult.Should().BeEquivalentTo(expectedActionResult);

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
