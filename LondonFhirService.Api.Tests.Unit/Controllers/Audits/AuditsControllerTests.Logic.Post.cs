// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.Audits;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Clients.Extensions;
using RESTFulSense.Models;

namespace LondonFhirService.Api.Tests.Unit.Controllers.Audits
{
    public partial class AuditsControllerTests
    {
        [Fact]
        public async Task ShouldReturnCreatedOnPostAsync()
        {
            // given
            Audit randomAudit = CreateRandomAudit();
            Audit inputAudit = randomAudit;
            Audit addedAudit = inputAudit.DeepClone();
            Audit expectedAudit = addedAudit.DeepClone();

            var expectedObjectResult =
                new CreatedObjectResult(expectedAudit);

            var expectedActionResult =
                new ActionResult<Audit>(expectedObjectResult);

            auditServiceMock
                .Setup(service => service.AddAuditAsync(inputAudit))
                    .ReturnsAsync(addedAudit);

            // when
            ActionResult<Audit> actualActionResult = await auditsController.PostAuditAsync(randomAudit);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            auditServiceMock
               .Verify(service => service.AddAuditAsync(inputAudit),
                   Times.Once);

            auditServiceMock.VerifyNoOtherCalls();
        }
    }
}