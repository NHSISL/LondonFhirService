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
using RESTFulSense.Models;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.Providers
{
    public partial class ProvidersControllerTests
    {
        [Fact]
        public async Task ShouldReturnCreatedOnPostAsync()
        {
            // given
            Provider randomProvider = CreateRandomProvider();
            Provider inputProvider = randomProvider;
            Provider addedProvider = inputProvider.DeepClone();
            Provider expectedProvider = addedProvider.DeepClone();

            var expectedObjectResult =
                new CreatedObjectResult(expectedProvider);

            var expectedActionResult =
                new ActionResult<Provider>(expectedObjectResult);

            providerServiceMock
                .Setup(service => service.AddProviderAsync(
                    inputProvider,
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(addedProvider);

            // when
            ActionResult<Provider> actualActionResult = await providersController.PostProviderAsync(randomProvider);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            providerServiceMock
                .Verify(service => service.AddProviderAsync(
                    inputProvider,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            providerServiceMock.VerifyNoOtherCalls();
        }
    }
}
