// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Clients.Extensions;
using RESTFulSense.Models;
using Xeptions;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.FhirRecords
{
    public partial class FhirRecordsControllerTests
    {
        [Theory]
        [MemberData(nameof(ValidationExceptions))]
        public async Task ShouldReturnBadRequestOnGetByIdIfValidationErrorOccurredAsync(Xeption validationException)
        {
            // given
            Guid someId = Guid.NewGuid();

            BadRequestObjectResult expectedBadRequestObjectResult =
                BadRequest(validationException.InnerException);

            var expectedActionResult =
                new ActionResult<FhirRecord>(expectedBadRequestObjectResult);

            this.fhirRecordServiceMock.Setup(service =>
                service.RetrieveFhirRecordByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<FhirRecord> actualActionResult =
                await this.fhirRecordsController.GetFhirRecordByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.fhirRecordServiceMock.Verify(service =>
                service.RetrieveFhirRecordByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(ServerExceptions))]
        public async Task ShouldReturnInternalServerErrorOnGetByIdIfServerErrorOccurredAsync(
            Xeption validationException)
        {
            // given
            Guid someId = Guid.NewGuid();

            InternalServerErrorObjectResult expectedInternalServerErrorObjectResult =
                InternalServerError(validationException);

            var expectedActionResult =
                new ActionResult<FhirRecord>(expectedInternalServerErrorObjectResult);

            this.fhirRecordServiceMock.Setup(service =>
                service.RetrieveFhirRecordByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<FhirRecord> actualActionResult =
                await this.fhirRecordsController.GetFhirRecordByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.fhirRecordServiceMock.Verify(service =>
                service.RetrieveFhirRecordByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNotFoundOnGetByIdIfItemDoesNotExistAsync()
        {
            // given
            Guid someId = Guid.NewGuid();
            string someMessage = GetRandomString();

            var notFoundFhirRecordException =
                new NotFoundFhirRecordException(
                    message: someMessage);

            var fhirRecordValidationException =
                new FhirRecordValidationException(
                    message: someMessage,
                    innerException: notFoundFhirRecordException);

            NotFoundObjectResult expectedNotFoundObjectResult =
                NotFound(notFoundFhirRecordException);

            var expectedActionResult =
                new ActionResult<FhirRecord>(expectedNotFoundObjectResult);

            this.fhirRecordServiceMock.Setup(service =>
                service.RetrieveFhirRecordByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(fhirRecordValidationException);

            // when
            ActionResult<FhirRecord> actualActionResult =
                await this.fhirRecordsController.GetFhirRecordByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.fhirRecordServiceMock.Verify(service =>
                service.RetrieveFhirRecordByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
        }
    }
}
