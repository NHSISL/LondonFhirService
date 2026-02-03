// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Audits;
using LondonFhirService.Core.Models.Foundations.Audits.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Clients.Extensions;
using RESTFulSense.Models;
using Xeptions;

namespace LondonFhirService.Api.Tests.Unit.Controllers.Audits
{
    public partial class AuditsControllerTests
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
                new ActionResult<Audit>(expectedBadRequestObjectResult);

            this.auditServiceMock.Setup(service =>
                service.RemoveAuditByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<Audit> actualActionResult =
                await this.auditsController.DeleteAuditByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.auditServiceMock.Verify(service =>
                service.RemoveAuditByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.auditServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(ServerExceptions))]
        public async Task ShouldReturnInternalServerErrorOnDeleteIfServerErrorOccurredAsync(
            Xeption validationException)
        {
            // given
            Guid someId = Guid.NewGuid();

            InternalServerErrorObjectResult expectedBadRequestObjectResult =
                InternalServerError(validationException);

            var expectedActionResult =
                new ActionResult<Audit>(expectedBadRequestObjectResult);

            this.auditServiceMock.Setup(service =>
                service.RemoveAuditByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<Audit> actualActionResult =
                await this.auditsController.DeleteAuditByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.auditServiceMock.Verify(service =>
                service.RemoveAuditByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.auditServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNotFoundOnDeleteIfItemDoesNotExistAsync()
        {
            // given
            Guid someId = Guid.NewGuid();
            string someMessage = GetRandomString();

            var notFoundAuditException =
                new NotFoundAuditServiceException(
                    message: someMessage);

            var auditValidationException =
                new AuditServiceValidationException(
                    message: someMessage,
                    innerException: notFoundAuditException);

            NotFoundObjectResult expectedNotFoundObjectResult =
                NotFound(notFoundAuditException);

            var expectedActionResult =
                new ActionResult<Audit>(expectedNotFoundObjectResult);

            this.auditServiceMock.Setup(service =>
                service.RemoveAuditByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(auditValidationException);

            // when
            ActionResult<Audit> actualActionResult =
                await this.auditsController.DeleteAuditByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.auditServiceMock.Verify(service =>
                service.RemoveAuditByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.auditServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnLockedOnDeleteIfRecordIsLockedAsync()
        {
            // given
            Guid someId = Guid.NewGuid();
            var someInnerException = new Exception();
            string someMessage = GetRandomString();

            var lockedAuditException =
                new LockedAuditServiceException(
                    message: someMessage,
                    innerException: someInnerException);

            var auditDependencyValidationException =
                new AuditServiceDependencyValidationException(
                    message: someMessage,
                    innerException: lockedAuditException);

            LockedObjectResult expectedConflictObjectResult =
                Locked(lockedAuditException);

            var expectedActionResult =
                new ActionResult<Audit>(expectedConflictObjectResult);

            this.auditServiceMock.Setup(service =>
                service.RemoveAuditByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(auditDependencyValidationException);

            // when
            ActionResult<Audit> actualActionResult =
                await this.auditsController.DeleteAuditByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.auditServiceMock.Verify(service =>
                service.RemoveAuditByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.auditServiceMock.VerifyNoOtherCalls();
        }
    }
}
