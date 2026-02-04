// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Audits;
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
        [MemberData(nameof(ServerExceptions))]
        public async Task ShouldReturnInternalServerErrorOnGetIfServerErrorOccurredAsync(
            Xeption serverException)
        {
            // given
            IQueryable<Audit> someAudits = CreateRandomAudits();

            InternalServerErrorObjectResult expectedInternalServerErrorObjectResult =
                InternalServerError(serverException);

            var expectedActionResult =
                new ActionResult<IQueryable<Audit>>(expectedInternalServerErrorObjectResult);

            this.auditServiceMock.Setup(service =>
                service.RetrieveAllAuditsAsync())
                    .ThrowsAsync(serverException);

            // when
            ActionResult<IQueryable<Audit>> actualActionResult =
                await this.auditsController.Get();

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.auditServiceMock.Verify(service =>
                service.RetrieveAllAuditsAsync(),
                    Times.Once);

            this.auditServiceMock.VerifyNoOtherCalls();
        }
    }
}
