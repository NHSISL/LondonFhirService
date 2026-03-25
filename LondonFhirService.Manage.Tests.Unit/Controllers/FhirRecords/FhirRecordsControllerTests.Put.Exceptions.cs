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
        public async Task ShouldReturnBadRequestOnPutIfValidationErrorOccurredAsync(Xeption validationException)
        {
            // given
            FhirRecord someFhirRecord = CreateRandomFhirRecord();

            BadRequestObjectResult expectedBadRequestObjectResult =
                BadRequest(validationException.InnerException);

            var expectedActionResult =
                new ActionResult<FhirRecord>(expectedBadRequestObjectResult);

            this.fhirRecordServiceMock.Setup(service =>
                service.ModifyFhirRecordAsync(It.IsAny<FhirRecord>()))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<FhirRecord> actualActionResult =
                await this.fhirRecordsController.PutFhirRecordAsync(someFhirRecord);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.fhirRecordServiceMock.Verify(service =>
                service.ModifyFhirRecordAsync(It.IsAny<FhirRecord>()),
                    Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(ServerExceptions))]
        public async Task ShouldReturnInternalServerErrorOnPutIfServerErrorOccurredAsync(
            Xeption validationException)
        {
            // given
            FhirRecord someFhirRecord = CreateRandomFhirRecord();

            InternalServerErrorObjectResult expectedInternalServerErrorObjectResult =
                InternalServerError(validationException);

            var expectedActionResult =
                new ActionResult<FhirRecord>(expectedInternalServerErrorObjectResult);

            this.fhirRecordServiceMock.Setup(service =>
                service.ModifyFhirRecordAsync(It.IsAny<FhirRecord>()))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<FhirRecord> actualActionResult =
                await this.fhirRecordsController.PutFhirRecordAsync(someFhirRecord);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.fhirRecordServiceMock.Verify(service =>
                service.ModifyFhirRecordAsync(It.IsAny<FhirRecord>()),
                    Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNotFoundOnPutIfItemDoesNotExistAsync()
        {
            // given
            FhirRecord someFhirRecord = CreateRandomFhirRecord();
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
                service.ModifyFhirRecordAsync(It.IsAny<FhirRecord>()))
                    .ThrowsAsync(fhirRecordValidationException);

            // when
            ActionResult<FhirRecord> actualActionResult =
                await this.fhirRecordsController.PutFhirRecordAsync(someFhirRecord);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.fhirRecordServiceMock.Verify(service =>
                service.ModifyFhirRecordAsync(It.IsAny<FhirRecord>()),
                    Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnConflictOnPutIfAlreadyExistsFhirRecordErrorOccurredAsync()
        {
            // given
            FhirRecord someFhirRecord = CreateRandomFhirRecord();
            var someInnerException = new Exception();
            string someMessage = GetRandomString();

            var alreadyExistsFhirRecordException =
                new AlreadyExistsFhirRecordException(
                    message: someMessage,
                    innerException: someInnerException);

            var fhirRecordDependencyValidationException =
                new FhirRecordDependencyValidationException(
                    message: someMessage,
                    innerException: alreadyExistsFhirRecordException);

            ConflictObjectResult expectedConflictObjectResult =
                Conflict(alreadyExistsFhirRecordException);

            var expectedActionResult =
                new ActionResult<FhirRecord>(expectedConflictObjectResult);

            this.fhirRecordServiceMock.Setup(service =>
                service.ModifyFhirRecordAsync(It.IsAny<FhirRecord>()))
                    .ThrowsAsync(fhirRecordDependencyValidationException);

            // when
            ActionResult<FhirRecord> actualActionResult =
                await this.fhirRecordsController.PutFhirRecordAsync(someFhirRecord);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.fhirRecordServiceMock.Verify(service =>
                service.ModifyFhirRecordAsync(It.IsAny<FhirRecord>()),
                    Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
        }
    }
}
