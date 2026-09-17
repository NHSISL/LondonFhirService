// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using FluentAssertions;
using Moq;
using RESTFulSense.Clients.Extensions;
using RESTFulSense.Models;
using Xeptions;

using LondonFhirService.Manage.Models.Foundations.Patients;
using LondonFhirService.Manage.Models.Foundations.Patients.Exceptions;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.Patients
{
    public partial class PatientsControllerTests
    {
        [Theory]
        [MemberData(nameof(ValidationExceptions))]
        public async Task ShouldReturnBadRequestOnPostGetStructuredRecordIfValidationErrorOccurredAsync(
            Xeption validationException)
        {
            // given
            StructuredRecordRequest someStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            BadRequestObjectResult expectedBadRequestObjectResult =
                BadRequest(validationException.InnerException);

            var expectedActionResult =
                new ActionResult<string>(expectedBadRequestObjectResult);

            this.patientServiceMock.Setup(service =>
                service.GetStructuredRecordAsync(
                    It.IsAny<StructuredRecordRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(validationException);

            // when
            ActionResult<string> actualActionResult =
                await this.patientsController.PostGetStructuredRecordAsync(
                    someStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.patientServiceMock.Verify(service =>
                service.GetStructuredRecordAsync(
                    It.IsAny<StructuredRecordRequest>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.patientServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(ServerExceptions))]
        public async Task ShouldReturnInternalServerErrorOnPostGetStructuredRecordIfServerErrorOccurredAsync(
            Xeption serverException)
        {
            // given
            StructuredRecordRequest someStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            InternalServerErrorObjectResult expectedInternalServerErrorObjectResult =
                InternalServerError(serverException);

            var expectedActionResult =
                new ActionResult<string>(expectedInternalServerErrorObjectResult);

            this.patientServiceMock.Setup(service =>
                service.GetStructuredRecordAsync(
                    It.IsAny<StructuredRecordRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(serverException);

            // when
            ActionResult<string> actualActionResult =
                await this.patientsController.PostGetStructuredRecordAsync(
                    someStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.patientServiceMock.Verify(service =>
                service.GetStructuredRecordAsync(
                    It.IsAny<StructuredRecordRequest>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.patientServiceMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// The upstream refused, so the operator gets a 400 and - the point of the screen - the
        /// refusal's own text. It arrives in ProblemDetails.Detail rather than in the errors
        /// dictionary RESTFulSense builds from Exception.Data, because Data is what the logging
        /// broker turns into the line it writes and a refusal can name a patient.
        /// </summary>
        [Fact]
        public async Task ShouldReturnBadRequestWithUpstreamBodyOnPostGetStructuredRecordIfUpstreamRefusedAsync()
        {
            // given
            StructuredRecordRequest someStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            string randomMessage = GetRandomString();
            string refusalBody = CreateRefusalBody();

            var dependencyValidationException =
                new PatientServiceDependencyValidationException(
                    message: randomMessage,
                    innerException: new Xeption(),
                    responseBody: refusalBody);

            var expectedProblemDetails = new ProblemDetails
            {
                Title = randomMessage,
                Status = StatusCodes.Status400BadRequest,
                Detail = refusalBody
            };

            this.patientServiceMock.Setup(service =>
                service.GetStructuredRecordAsync(
                    It.IsAny<StructuredRecordRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(dependencyValidationException);

            // when
            ActionResult<string> actualActionResult =
                await this.patientsController.PostGetStructuredRecordAsync(
                    someStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            // then
            var actualObjectResult = actualActionResult.Result as ObjectResult;
            actualObjectResult.Should().NotBeNull();
            actualObjectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

            actualObjectResult.Value.Should().BeEquivalentTo(expectedProblemDetails);

            this.patientServiceMock.Verify(service =>
                service.GetStructuredRecordAsync(
                    It.IsAny<StructuredRecordRequest>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.patientServiceMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// The upstream failed rather than judged, so a 500 - but the provider's own error body
        /// still reaches the operator. Dropping it here was the reason this endpoint used to
        /// answer a 500 that said nothing except to contact support.
        /// </summary>
        [Fact]
        public async Task
            ShouldReturnInternalServerErrorWithUpstreamBodyOnPostGetStructuredRecordIfUpstreamFailedAsync()
        {
            // given
            StructuredRecordRequest someStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            string randomMessage = GetRandomString();
            string failureBody = CreateRefusalBody();

            var dependencyException =
                new PatientServiceDependencyException(
                    message: randomMessage,
                    innerException: new Xeption(),
                    responseBody: failureBody);

            var expectedProblemDetails = new ProblemDetails
            {
                Title = randomMessage,
                Status = StatusCodes.Status500InternalServerError,
                Detail = failureBody
            };

            this.patientServiceMock.Setup(service =>
                service.GetStructuredRecordAsync(
                    It.IsAny<StructuredRecordRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(dependencyException);

            // when
            ActionResult<string> actualActionResult =
                await this.patientsController.PostGetStructuredRecordAsync(
                    someStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            // then
            var actualObjectResult = actualActionResult.Result as ObjectResult;
            actualObjectResult.Should().NotBeNull();
            actualObjectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

            actualObjectResult.Value.Should().BeEquivalentTo(expectedProblemDetails);

            this.patientServiceMock.Verify(service =>
                service.GetStructuredRecordAsync(
                    It.IsAny<StructuredRecordRequest>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.patientServiceMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// A timeout has no response body by definition, so the rule above would have left the
        /// operator with a generic wrapper title and an empty detail - on the one screen whose
        /// purpose is explaining why a call failed. This service writes the timeout message
        /// itself, so it is worth relaying where an arbitrary inner message is not.
        /// </summary>
        [Fact]
        public async Task ShouldReturnTheTimeoutMessageOnPostGetStructuredRecordIfItTimedOutAsync()
        {
            // given
            StructuredRecordRequest someStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            string expectedDetail = "Patient request timed out, please try again.";

            var dependencyException =
                new PatientServiceDependencyException(
                    message: GetRandomString(),
                    innerException: new TimedOutPatientServiceException(
                        message: expectedDetail,
                        innerException: new Xeption()));

            this.patientServiceMock.Setup(service =>
                service.GetStructuredRecordAsync(
                    It.IsAny<StructuredRecordRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(dependencyException);

            // when
            ActionResult<string> actualActionResult =
                await this.patientsController.PostGetStructuredRecordAsync(
                    someStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            // then
            var actualObjectResult = actualActionResult.Result as ObjectResult;
            actualObjectResult.Should().NotBeNull();

            var actualProblemDetails = actualObjectResult.Value as ProblemDetails;
            actualProblemDetails.Should().NotBeNull();
            actualProblemDetails.Detail.Should().Be(expectedDetail);

            this.patientServiceMock.Verify(service =>
                service.GetStructuredRecordAsync(
                    It.IsAny<StructuredRecordRequest>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);
        }

        /// <summary>
        /// A transport failure - a refused connection, a DNS miss - never had a response to keep,
        /// so there is nothing to show beyond the status. Detail stays null rather than becoming
        /// an empty string the page would render as an empty payload box.
        /// </summary>
        [Fact]
        public async Task ShouldReturnInternalServerErrorWithoutDetailOnPostGetStructuredRecordIfNoUpstreamBodyAsync()
        {
            // given
            StructuredRecordRequest someStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            var dependencyException =
                new PatientServiceDependencyException(
                    message: GetRandomString(),
                    innerException: new Xeption());

            this.patientServiceMock.Setup(service =>
                service.GetStructuredRecordAsync(
                    It.IsAny<StructuredRecordRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(dependencyException);

            // when
            ActionResult<string> actualActionResult =
                await this.patientsController.PostGetStructuredRecordAsync(
                    someStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            // then
            var actualObjectResult = actualActionResult.Result as ObjectResult;
            actualObjectResult.Should().NotBeNull();

            var actualProblemDetails = actualObjectResult.Value as ProblemDetails;
            actualProblemDetails.Should().NotBeNull();
            actualProblemDetails.Detail.Should().BeNull();

            this.patientServiceMock.Verify(service =>
                service.GetStructuredRecordAsync(
                    It.IsAny<StructuredRecordRequest>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.patientServiceMock.VerifyNoOtherCalls();
        }
    }
}
