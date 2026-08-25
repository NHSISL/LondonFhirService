// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.Providers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Clients.Extensions;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.Providers
{
    public partial class ProvidersControllerTests
    {
        [Fact]
        public async Task ShouldReturnRecordsOnGetAsync()
        {
            // given
            IQueryable<Provider> randomProviders = CreateRandomProviders();
            IQueryable<Provider> storageProviders = randomProviders.DeepClone();
            IQueryable<Provider> expectedProvider = storageProviders.DeepClone();

            var expectedObjectResult =
                new OkObjectResult(expectedProvider);

            var expectedActionResult =
                new ActionResult<IQueryable<Provider>>(expectedObjectResult);

            providerServiceMock
                .Setup(service => service.RetrieveAllProvidersAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageProviders);

            // when
            ActionResult<IQueryable<Provider>> actualActionResult = await providersController.Get();

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            providerServiceMock
               .Verify(service => service.RetrieveAllProvidersAsync(It.IsAny<CancellationToken>()),
                   Times.Once);

            providerServiceMock.VerifyNoOtherCalls();
        }
    }
}
