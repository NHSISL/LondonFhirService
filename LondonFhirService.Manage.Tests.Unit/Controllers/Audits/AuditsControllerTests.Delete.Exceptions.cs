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

namespace LondonFhirService.Manage.Tests.Unit.Controllers.Audits
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
                service.RemoveAuditByIdAsync(It.IsAny<Guid>(), default))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<Audit> actualActionResult =
                await this.auditsController.DeleteAuditByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.auditServiceMock.Verify(service =>
                service.RemoveAuditByIdAsync(It.IsAny<Guid>(), default),
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

            InternalServerErrorObjectResult expectedInternalServerErrorObjectResult =
                InternalServerError(validationException);

            var expectedActionResult =
                new ActionResult<Audit>(expectedInternalServerErrorObjectResult);

            this.auditServiceMock.Setup(service =>
                service.RemoveAuditByIdAsync(It.IsAny<Guid>(), default))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<Audit> actualActionResult =
                await this.auditsController.DeleteAuditByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.auditServiceMock.Verify(service =>
                service.RemoveAuditByIdAsync(It.IsAny<Guid>(), default),
                    Times.Once);

            this.auditServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNotFoundOnDeleteIfItemDoesNotExistAsync()
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
                service.RemoveAuditByIdAsync(It.IsAny<Guid>(), default))
                    .ThrowsAsync(auditServiceValidationException);

            // when
            ActionResult<Audit> actualActionResult =
                await this.auditsController.DeleteAuditByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.auditServiceMock.Verify(service =>
                service.RemoveAuditByIdAsync(It.IsAny<Guid>(), default),
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

            var lockedAuditServiceException =
                new LockedAuditServiceException(
                    message: someMessage,
                    innerException: someInnerException);

            var auditServiceDependencyValidationException =
                new AuditServiceDependencyValidationException(
                    message: someMessage,
                    innerException: lockedAuditServiceException);

            LockedObjectResult expectedConflictObjectResult =
                Locked(lockedAuditServiceException);

            var expectedActionResult =
                new ActionResult<Audit>(expectedConflictObjectResult);

            this.auditServiceMock.Setup(service =>
                service.RemoveAuditByIdAsync(It.IsAny<Guid>(), default))
                    .ThrowsAsync(auditServiceDependencyValidationException);

            // when
            ActionResult<Audit> actualActionResult =
                await this.auditsController.DeleteAuditByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.auditServiceMock.Verify(service =>
                service.RemoveAuditByIdAsync(It.IsAny<Guid>(), default),
                    Times.Once);

            this.auditServiceMock.VerifyNoOtherCalls();
        }
    }
}
