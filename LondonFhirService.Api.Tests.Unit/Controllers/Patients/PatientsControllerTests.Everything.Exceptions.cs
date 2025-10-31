// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using FluentAssertions;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xeptions;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Api.Tests.Unit.Controllers.Patients
{
    public partial class PatientsControllerTests
    {
        [Theory]
        [MemberData(nameof(ValidationExceptions))]
        public async Task ShouldReturnBadRequestOnEverythingIfValidationErrorOccurredAsync(
            Xeption validationException)
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

            BadRequestObjectResult expectedBadRequestObjectResult =
                BadRequest(validationException.InnerException);

            var expectedActionResult = new ActionResult<Bundle>(expectedBadRequestObjectResult);

            this.patientCoordinationServiceMock.Setup(coordination =>
                coordination.Everything(
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
                await this.patientsController.Everything(
                    inputId,
                    inputStart,
                    inputEnd,
                    inputTypeFilter,
                    inputSince,
                    inputCount,
                    cancellationToken);

            // then
            actualActionResult.Should().BeEquivalentTo(expectedActionResult);

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
