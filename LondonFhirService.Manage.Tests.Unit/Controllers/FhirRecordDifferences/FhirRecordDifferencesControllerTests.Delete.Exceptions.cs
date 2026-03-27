// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Clients.Extensions;
using RESTFulSense.Models;
using Xeptions;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.FhirRecordDifferences
{
    public partial class FhirRecordDifferencesControllerTests
    {
        [Theory]
        [MemberData(nameof(ValidationExceptions))]
        public async Task ShouldReturnBadRequestOnDeleteIfValidationErrorOccurredAsync(Xeption validationException)
        {
            // given
            Guid someId = Guid.NewGuid();

            BadRequestObjectResult expectedBadRequestObjectResult =
                BadRequest(validationException.InnerException);

            var expectedActionResult =
                new ActionResult<FhirRecordDifference>(expectedBadRequestObjectResult);

            this.fhirRecordDifferenceServiceMock.Setup(service =>
                service.RemoveFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<FhirRecordDifference> actualActionResult =
                await this.fhirRecordDifferencesController.DeleteFhirRecordDifferenceByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.fhirRecordDifferenceServiceMock.Verify(service =>
                service.RemoveFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(ServerExceptions))]
        public async Task ShouldReturnInternalServerErrorOnDeleteIfServerErrorOccurredAsync(
            Xeption validationException)
        {
            // given
            Guid someId = Guid.NewGuid();

            InternalServerErrorObjectResult expectedInternalServerErrorObjectResult =
                InternalServerError(validationException);

            var expectedActionResult =
                new ActionResult<FhirRecordDifference>(expectedInternalServerErrorObjectResult);

            this.fhirRecordDifferenceServiceMock.Setup(service =>
                service.RemoveFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<FhirRecordDifference> actualActionResult =
                await this.fhirRecordDifferencesController.DeleteFhirRecordDifferenceByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.fhirRecordDifferenceServiceMock.Verify(service =>
                service.RemoveFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNotFoundOnDeleteIfItemDoesNotExistAsync()
        {
            // given
            Guid someId = Guid.NewGuid();
            string someMessage = GetRandomString();

            var notFoundFhirRecordDifferenceException =
                new NotFoundFhirRecordDifferenceException(
                    message: someMessage);

            var fhirRecordDifferenceValidationException =
                new FhirRecordDifferenceValidationException(
                    message: someMessage,
                    innerException: notFoundFhirRecordDifferenceException);

            NotFoundObjectResult expectedNotFoundObjectResult =
                NotFound(notFoundFhirRecordDifferenceException);

            var expectedActionResult =
                new ActionResult<FhirRecordDifference>(expectedNotFoundObjectResult);

            this.fhirRecordDifferenceServiceMock.Setup(service =>
                service.RemoveFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(fhirRecordDifferenceValidationException);

            // when
            ActionResult<FhirRecordDifference> actualActionResult =
                await this.fhirRecordDifferencesController.DeleteFhirRecordDifferenceByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.fhirRecordDifferenceServiceMock.Verify(service =>
                service.RemoveFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnLockedOnDeleteIfRecordIsLockedAsync()
        {
            // given
            Guid someId = Guid.NewGuid();
            var someInnerException = new Exception();
            string someMessage = GetRandomString();

            var lockedFhirRecordDifferenceException =
                new LockedFhirRecordDifferenceException(
                    message: someMessage,
                    innerException: someInnerException);

            var fhirRecordDifferenceDependencyValidationException =
                new FhirRecordDifferenceDependencyValidationException(
                    message: someMessage,
                    innerException: lockedFhirRecordDifferenceException);

            LockedObjectResult expectedConflictObjectResult =
                Locked(lockedFhirRecordDifferenceException);

            var expectedActionResult =
                new ActionResult<FhirRecordDifference>(expectedConflictObjectResult);

            this.fhirRecordDifferenceServiceMock.Setup(service =>
                service.RemoveFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(fhirRecordDifferenceDependencyValidationException);

            // when
            ActionResult<FhirRecordDifference> actualActionResult =
                await this.fhirRecordDifferencesController.DeleteFhirRecordDifferenceByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.fhirRecordDifferenceServiceMock.Verify(service =>
                service.RemoveFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
        }
    }
}
