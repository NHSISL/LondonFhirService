// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Linq;
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
        public async Task ShouldReturnRecordsOnGetAsync()
        {
            // given
            IQueryable<Audit> randomAudits = CreateRandomAudits();
            IQueryable<Audit> storageAudits = randomAudits.DeepClone();
            IQueryable<Audit> expectedAudit = storageAudits.DeepClone();

            var expectedObjectResult =
                new OkObjectResult(expectedAudit);

            var expectedActionResult =
                new ActionResult<IQueryable<Audit>>(expectedObjectResult);

            auditServiceMock
                .Setup(service => service.RetrieveAllAuditsAsync())
                    .ReturnsAsync(storageAudits);

            // when
            ActionResult<IQueryable<Audit>> actualActionResult = await auditsController.Get();

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            auditServiceMock
               .Verify(service => service.RetrieveAllAuditsAsync(),
                   Times.Once);

            auditServiceMock.VerifyNoOtherCalls();
        }
    }
}
