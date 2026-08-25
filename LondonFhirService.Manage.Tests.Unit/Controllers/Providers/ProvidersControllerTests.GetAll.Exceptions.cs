// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Providers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Clients.Extensions;
using RESTFulSense.Models;
using Xeptions;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.Providers
{
    public partial class ProvidersControllerTests
    {
        [Theory]
        [MemberData(nameof(ServerExceptions))]
        public async Task ShouldReturnInternalServerErrorOnGetIfServerErrorOccurredAsync(
            Xeption serverException)
        {
            // given
            InternalServerErrorObjectResult expectedInternalServerErrorObjectResult =
                InternalServerError(serverException);

            var expectedActionResult =
                new ActionResult<IQueryable<Provider>>(expectedInternalServerErrorObjectResult);

            this.providerServiceMock.Setup(service =>
                service.RetrieveAllProvidersAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(serverException);

            // when
            ActionResult<IQueryable<Provider>> actualActionResult =
                await this.providersController.Get();

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.providerServiceMock.Verify(service =>
                service.RetrieveAllProvidersAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            this.providerServiceMock.VerifyNoOtherCalls();
        }
    }
}
