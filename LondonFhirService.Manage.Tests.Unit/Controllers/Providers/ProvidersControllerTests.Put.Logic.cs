// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

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
        public async Task ShouldReturnOkOnPutAsync()
        {
            // given
            Provider randomProvider = CreateRandomProvider();
            Provider inputProvider = randomProvider;
            Provider storageProvider = inputProvider.DeepClone();
            Provider expectedProvider = storageProvider.DeepClone();

            var expectedObjectResult =
                new OkObjectResult(expectedProvider);

            var expectedActionResult =
                new ActionResult<Provider>(expectedObjectResult);

            providerServiceMock
                .Setup(service => service.ModifyProviderAsync(
                    inputProvider,
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(storageProvider);

            // when
            ActionResult<Provider> actualActionResult = await providersController.PutProviderAsync(randomProvider);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            providerServiceMock
                .Verify(service => service.ModifyProviderAsync(
                    inputProvider,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            providerServiceMock.VerifyNoOtherCalls();
        }
    }
}
