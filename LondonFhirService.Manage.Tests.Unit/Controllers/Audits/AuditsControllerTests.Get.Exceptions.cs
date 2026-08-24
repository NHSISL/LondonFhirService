// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Audits;
using LondonFhirService.Core.Models.Foundations.Audits.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Clients.Extensions;
using RESTFulSense.Models;
using Xeptions;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.Audits
{
    public partial class AuditsControllerTests
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
                new ActionResult<Audit>(expectedBadRequestObjectResult);

            this.auditServiceMock.Setup(service =>
                service.RetrieveAuditByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(validationException);

            // when
            ActionResult<Audit> actualActionResult =
                await this.auditsController.GetAuditByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.auditServiceMock.Verify(service =>
                service.RetrieveAuditByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.auditServiceMock.VerifyNoOtherCalls();
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
                new ActionResult<Audit>(expectedInternalServerErrorObjectResult);

            this.auditServiceMock.Setup(service =>
                service.RetrieveAuditByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(validationException);

            // when
            ActionResult<Audit> actualActionResult =
                await this.auditsController.GetAuditByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.auditServiceMock.Verify(service =>
                service.RetrieveAuditByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.auditServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNotFoundOnGetByIdIfItemDoesNotExistAsync()
        {
            // given
            Guid someId = Guid.NewGuid();
            string someMessage = GetRandomString();

            var notFoundAuditServiceException =
                new NotFoundAuditServiceException(
                    message: someMessage);

            var auditServiceValidationException =
                new AuditServiceValidationException(
                    message: someMessage,
                    innerException: notFoundAuditServiceException);

            NotFoundObjectResult expectedNotFoundObjectResult =
                NotFound(notFoundAuditServiceException);

            var expectedActionResult =
                new ActionResult<Audit>(expectedNotFoundObjectResult);

            this.auditServiceMock.Setup(service =>
                service.RetrieveAuditByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(auditServiceValidationException);

            // when
            ActionResult<Audit> actualActionResult =
                await this.auditsController.GetAuditByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.auditServiceMock.Verify(service =>
                service.RetrieveAuditByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.auditServiceMock.VerifyNoOtherCalls();
        }
    }
}
