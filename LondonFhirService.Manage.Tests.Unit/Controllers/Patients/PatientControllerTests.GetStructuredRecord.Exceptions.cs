// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Manage.Models.Foundations.Patients;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Clients.Extensions;
using RESTFulSense.Models;
using Xeptions;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.Patients
{
    public partial class PatientControllerTests
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
                service.GetStructuredRecord(
                    It.IsAny<StructuredRecordRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(validationException);

            // when
            ActionResult<string> actualActionResult =
                await this.patientController.PostGetStructuredRecordAsync(
                    someStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.patientServiceMock.Verify(service =>
                service.GetStructuredRecord(
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
                service.GetStructuredRecord(
                    It.IsAny<StructuredRecordRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(serverException);

            // when
            ActionResult<string> actualActionResult =
                await this.patientController.PostGetStructuredRecordAsync(
                    someStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.patientServiceMock.Verify(service =>
                service.GetStructuredRecord(
                    It.IsAny<StructuredRecordRequest>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.patientServiceMock.VerifyNoOtherCalls();
        }
    }
}
