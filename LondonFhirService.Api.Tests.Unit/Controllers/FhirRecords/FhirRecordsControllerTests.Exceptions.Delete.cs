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

namespace LondonFhirService.Api.Tests.Unit.Controllers.FhirRecords
{
    public partial class FhirRecordsControllerTests
    {
        [Theory]
        [MemberData(nameof(ValidationExceptions))]
        public async Task ShouldReturnBadRequestOnDeleteIfValidationErrorOccurredAsync(
            Xeption validationException)
        {
            // given
            Guid someId = Guid.NewGuid();

            BadRequestObjectResult expectedBadRequestObjectResult =
                BadRequest(validationException.InnerException);

            var expectedActionResult =
                new ActionResult<FhirRecord>(expectedBadRequestObjectResult);

            this.fhirRecordServiceMock.Setup(service =>
                service.RemoveFhirRecordByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<FhirRecord> actualActionResult =
                await this.fhirRecordsController.DeleteFhirRecordByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.fhirRecordServiceMock.Verify(service =>
                service.RemoveFhirRecordByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(ServerExceptions))]
        public async Task ShouldReturnInternalServerErrorOnDeleteIfServerErrorOccurredAsync(
            Xeption serverException)
        {
            // given
            Guid someId = Guid.NewGuid();

            InternalServerErrorObjectResult expectedInternalServerErrorObjectResult =
                InternalServerError(serverException);

            var expectedActionResult =
                new ActionResult<FhirRecord>(expectedInternalServerErrorObjectResult);

            this.fhirRecordServiceMock.Setup(service =>
                service.RemoveFhirRecordByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(serverException);

            // when
            ActionResult<FhirRecord> actualActionResult =
                await this.fhirRecordsController.DeleteFhirRecordByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.fhirRecordServiceMock.Verify(service =>
                service.RemoveFhirRecordByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNotFoundOnDeleteIfFhirRecordDoesNotExistAsync()
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
                service.RemoveFhirRecordByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(fhirRecordValidationException);

            // when
            ActionResult<FhirRecord> actualActionResult =
                await this.fhirRecordsController.DeleteFhirRecordByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.fhirRecordServiceMock.Verify(service =>
                service.RemoveFhirRecordByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnLockedOnDeleteIfFhirRecordIsLockedAsync()
        {
            // given
            Guid someId = Guid.NewGuid();
            var someInnerException = new Exception();
            string someMessage = GetRandomString();

            var lockedFhirRecordException =
                new LockedFhirRecordException(
                    message: someMessage,
                    innerException: someInnerException);

            var fhirRecordDependencyValidationException =
                new FhirRecordDependencyValidationException(
                    message: someMessage,
                    innerException: lockedFhirRecordException);

            LockedObjectResult expectedLockedObjectResult =
                Locked(lockedFhirRecordException);

            var expectedActionResult =
                new ActionResult<FhirRecord>(expectedLockedObjectResult);

            this.fhirRecordServiceMock.Setup(service =>
                service.RemoveFhirRecordByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(fhirRecordDependencyValidationException);

            // when
            ActionResult<FhirRecord> actualActionResult =
                await this.fhirRecordsController.DeleteFhirRecordByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.fhirRecordServiceMock.Verify(service =>
                service.RemoveFhirRecordByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
        }
    }
}
