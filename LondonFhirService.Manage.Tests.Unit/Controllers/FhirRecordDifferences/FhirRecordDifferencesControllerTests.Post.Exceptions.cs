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
        public async Task ShouldReturnBadRequestOnPostIfValidationErrorOccurredAsync(Xeption validationException)
        {
            // given
            FhirRecordDifference someFhirRecordDifference = CreateRandomFhirRecordDifference();

            BadRequestObjectResult expectedBadRequestObjectResult =
                BadRequest(validationException.InnerException);

            var expectedActionResult =
                new ActionResult<FhirRecordDifference>(expectedBadRequestObjectResult);

            this.fhirRecordDifferenceServiceMock.Setup(service =>
                service.AddFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<FhirRecordDifference> actualActionResult =
                await this.fhirRecordDifferencesController.PostFhirRecordDifferenceAsync(someFhirRecordDifference);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.fhirRecordDifferenceServiceMock.Verify(service =>
                service.AddFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Once);

            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(ServerExceptions))]
        public async Task ShouldReturnInternalServerErrorOnPostIfServerErrorOccurredAsync(
            Xeption validationException)
        {
            // given
            FhirRecordDifference someFhirRecordDifference = CreateRandomFhirRecordDifference();

            InternalServerErrorObjectResult expectedInternalServerErrorObjectResult =
                InternalServerError(validationException);

            var expectedActionResult =
                new ActionResult<FhirRecordDifference>(expectedInternalServerErrorObjectResult);

            this.fhirRecordDifferenceServiceMock.Setup(service =>
                service.AddFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<FhirRecordDifference> actualActionResult =
                await this.fhirRecordDifferencesController.PostFhirRecordDifferenceAsync(someFhirRecordDifference);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.fhirRecordDifferenceServiceMock.Verify(service =>
                service.AddFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Once);

            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnConflictOnPostIfAlreadyExistsFhirRecordDifferenceErrorOccurredAsync()
        {
            // given
            FhirRecordDifference someFhirRecordDifference = CreateRandomFhirRecordDifference();
            var someInnerException = new Exception();
            string someMessage = GetRandomString();

            var alreadyExistsFhirRecordDifferenceException =
                new AlreadyExistsFhirRecordDifferenceException(
                    message: someMessage,
                    innerException: someInnerException);

            var fhirRecordDifferenceDependencyValidationException =
                new FhirRecordDifferenceDependencyValidationException(
                    message: someMessage,
                    innerException: alreadyExistsFhirRecordDifferenceException);

            ConflictObjectResult expectedConflictObjectResult =
                Conflict(alreadyExistsFhirRecordDifferenceException);

            var expectedActionResult =
                new ActionResult<FhirRecordDifference>(expectedConflictObjectResult);

            this.fhirRecordDifferenceServiceMock.Setup(service =>
                service.AddFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()))
                    .ThrowsAsync(fhirRecordDifferenceDependencyValidationException);

            // when
            ActionResult<FhirRecordDifference> actualActionResult =
                await this.fhirRecordDifferencesController.PostFhirRecordDifferenceAsync(someFhirRecordDifference);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.fhirRecordDifferenceServiceMock.Verify(service =>
                service.AddFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Once);

            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
        }
    }
}