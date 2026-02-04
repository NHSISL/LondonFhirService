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
        public async Task ShouldReturnBadRequestOnPostIfValidationErrorOccurredAsync(Xeption validationException)
        {
            // given
            Audit someAudit = CreateRandomAudit();

            BadRequestObjectResult expectedBadRequestObjectResult =
                BadRequest(validationException.InnerException);

            var expectedActionResult =
                new ActionResult<Audit>(expectedBadRequestObjectResult);

            this.auditServiceMock.Setup(service =>
                service.AddAuditAsync(It.IsAny<Audit>()))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<Audit> actualActionResult =
                await this.auditsController.PostAuditAsync(someAudit);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.auditServiceMock.Verify(service =>
                service.AddAuditAsync(It.IsAny<Audit>()),
                    Times.Once);

            this.auditServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(ServerExceptions))]
        public async Task ShouldReturnInternalServerErrorOnPostIfServerErrorOccurredAsync(
            Xeption validationException)
        {
            // given
            Audit someAudit = CreateRandomAudit();

            InternalServerErrorObjectResult expectedBadRequestObjectResult =
                InternalServerError(validationException);

            var expectedActionResult =
                new ActionResult<Audit>(expectedBadRequestObjectResult);

            this.auditServiceMock.Setup(service =>
                service.AddAuditAsync(It.IsAny<Audit>()))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<Audit> actualActionResult =
                await this.auditsController.PostAuditAsync(someAudit);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.auditServiceMock.Verify(service =>
                service.AddAuditAsync(It.IsAny<Audit>()),
                    Times.Once);

            this.auditServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnConflictOnPostIfAlreadyExistsAuditErrorOccurredAsync()
        {
            // given
            Audit someAudit = CreateRandomAudit();
            var someInnerException = new Exception();
            string someMessage = GetRandomString();

            var alreadyExistsAuditException =
                new AlreadyExistsAuditServiceException(
                    message: someMessage,
                    innerException: someInnerException,
                    data: someInnerException.Data);

            var auditDependencyValidationException =
                new AuditServiceDependencyValidationException(
                    message: someMessage,
                    innerException: alreadyExistsAuditException);

            ConflictObjectResult expectedConflictObjectResult =
                Conflict(alreadyExistsAuditException);

            var expectedActionResult =
                new ActionResult<Audit>(expectedConflictObjectResult);

            this.auditServiceMock.Setup(service =>
                service.AddAuditAsync(It.IsAny<Audit>()))
                    .ThrowsAsync(auditDependencyValidationException);

            // when
            ActionResult<Audit> actualActionResult =
                await this.auditsController.PostAuditAsync(someAudit);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.auditServiceMock.Verify(service =>
                service.AddAuditAsync(It.IsAny<Audit>()),
                    Times.Once);

            this.auditServiceMock.VerifyNoOtherCalls();
        }
    }
}