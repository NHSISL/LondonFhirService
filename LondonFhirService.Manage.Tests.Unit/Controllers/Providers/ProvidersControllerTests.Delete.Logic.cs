// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
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
        public async Task ShouldRemoveRecordOnDeleteByIdsAsync()
        {
            // given
            Provider randomProvider = CreateRandomProvider();
            Guid inputId = randomProvider.Id;
            Provider storageProvider = randomProvider;
            Provider expectedProvider = storageProvider.DeepClone();

            var expectedObjectResult =
                new OkObjectResult(expectedProvider);

            var expectedActionResult =
                new ActionResult<Provider>(expectedObjectResult);

            providerServiceMock
                .Setup(service => service.RemoveProviderByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(storageProvider);

            // when
            ActionResult<Provider> actualActionResult = await providersController.DeleteProviderByIdAsync(inputId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            providerServiceMock
                .Verify(service => service.RemoveProviderByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            providerServiceMock.VerifyNoOtherCalls();
        }
    }
}
