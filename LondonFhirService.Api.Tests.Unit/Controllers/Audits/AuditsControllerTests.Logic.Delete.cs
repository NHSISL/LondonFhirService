// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.Audits;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Clients.Extensions;

namespace LondonFhirService.Api.Tests.Unit.Controllers.Audits
{
    public partial class AuditsControllerTests
    {
        [Fact]
        public async Task ShouldRemoveRecordOnDeleteByIdsAsync()
        {
            // given
            Audit randomAudit = CreateRandomAudit();
            Guid inputId = randomAudit.Id;
            Audit storageAudit = randomAudit;
            Audit expectedAudit = storageAudit.DeepClone();

            var expectedObjectResult =
                new OkObjectResult(expectedAudit);

            var expectedActionResult =
                new ActionResult<Audit>(expectedObjectResult);

            auditServiceMock
                .Setup(service => service.RemoveAuditByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(storageAudit);

            // when
            ActionResult<Audit> actualActionResult = await auditsController.DeleteAuditByIdAsync(inputId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            auditServiceMock
                .Verify(service => service.RemoveAuditByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            auditServiceMock.VerifyNoOtherCalls();
        }
    }
}
