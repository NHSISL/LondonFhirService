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

namespace LondonFhirService.Api.Tests.Unit.Controllers.Patients.STU3
{
    public partial class Stu3PatientControllerTests
    {
        [Theory]
        [MemberData(nameof(ValidationExceptions))]
        public async Task ShouldReturnBadRequestOnGetStructuredRecordIfValidationErrorOccurredAsync(
            Xeption validationException)
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            DateTimeOffset randomDateOfBirth = GetRandomDateTimeOffset();
            DateTimeOffset inputDateOfBirth = randomDateOfBirth;
            bool inputDemographicsOnly = false;
            bool inputIncludeInactivePatients = false;
            CancellationToken cancellationToken = CancellationToken.None;

            Parameters randomParameters = CreateRandomGetStructuredRecordParameters(
                dateOfBirth: inputDateOfBirth,
                demographicsOnly: inputDemographicsOnly,
                includeInactivePatients: inputIncludeInactivePatients);

            Parameters inputParameters = randomParameters;

            BadRequestObjectResult expectedBadRequestObjectResult =
                BadRequest(validationException.InnerException);

            var expectedActionResult = new ActionResult<Bundle>(expectedBadRequestObjectResult);

            this.patientCoordinationServiceMock.Setup(coordination =>
                coordination.GetStructuredRecord(
                    inputNhsNumber,
                    inputDateOfBirth.DateTime,
                    inputDemographicsOnly,
                    inputIncludeInactivePatients,
                    cancellationToken))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<Bundle> actualActionResult =
                await this.patientController.GetStructuredRecord(inputNhsNumber, inputParameters, cancellationToken);

            // then
            actualActionResult.Should().BeEquivalentTo(expectedActionResult);

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

        [Theory]
        [MemberData(nameof(ServerExceptions))]
        public async Task ShouldReturnInternalServerErrorOnGetStructuredRecordIfServerErrorOccurredAsync(
            Xeption serverException)
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            DateTimeOffset randomDateOfBirth = GetRandomDateTimeOffset();
            DateTimeOffset inputDateOfBirth = randomDateOfBirth;
            bool inputDemographicsOnly = false;
            bool inputIncludeInactivePatients = false;
            CancellationToken cancellationToken = CancellationToken.None;

            Parameters randomParameters = CreateRandomGetStructuredRecordParameters(
                dateOfBirth: inputDateOfBirth,
                demographicsOnly: inputDemographicsOnly,
                includeInactivePatients: inputIncludeInactivePatients);

            Parameters inputParameters = randomParameters;

            InternalServerErrorObjectResult expectedInternalServerErrorObjectResult =
                InternalServerError(serverException);

            var expectedActionResult = new ActionResult<Bundle>(expectedInternalServerErrorObjectResult);

            this.patientCoordinationServiceMock.Setup(coordination =>
                coordination.GetStructuredRecord(
                    inputNhsNumber,
                    inputDateOfBirth.DateTime,
                    inputDemographicsOnly,
                    inputIncludeInactivePatients,
                    cancellationToken))
                        .ThrowsAsync(serverException);

            // when
            ActionResult<Bundle> actualActionResult =
                await this.patientController.GetStructuredRecord(inputNhsNumber, inputParameters, cancellationToken);

            // then
            actualActionResult.Should().BeEquivalentTo(expectedActionResult);

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
