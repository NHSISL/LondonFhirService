// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

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
        public async Task ShouldReturnOkOnPutAsync()
        {
            // given
            Audit randomAudit = CreateRandomAudit();
            Audit inputAudit = randomAudit;
            Audit storageAudit = inputAudit.DeepClone();
            Audit expectedAudit = storageAudit.DeepClone();

            var expectedObjectResult =
                new OkObjectResult(expectedAudit);

            var expectedActionResult =
                new ActionResult<Audit>(expectedObjectResult);

            auditServiceMock
                .Setup(service => service.ModifyAuditAsync(inputAudit))
                    .ReturnsAsync(storageAudit);

            // when
            ActionResult<Audit> actualActionResult = await auditsController.PutAuditAsync(randomAudit);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            auditServiceMock
               .Verify(service => service.ModifyAuditAsync(inputAudit),
                   Times.Once);

            auditServiceMock.VerifyNoOtherCalls();
        }
    }
}
