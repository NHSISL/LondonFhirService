// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Models.Foundations.Providers.Exceptions;
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
        [MemberData(nameof(ValidationExceptions))]
        public async Task ShouldReturnBadRequestOnPostIfValidationErrorOccurredAsync(Xeption validationException)
        {
            // given
            Provider someProvider = CreateRandomProvider();

            BadRequestObjectResult expectedBadRequestObjectResult =
                BadRequest(validationException.InnerException);

            var expectedActionResult =
                new ActionResult<Provider>(expectedBadRequestObjectResult);

            this.providerServiceMock.Setup(service =>
                service.AddProviderAsync(It.IsAny<Provider>(), default))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<Provider> actualActionResult =
                await this.providersController.PostProviderAsync(someProvider);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.providerServiceMock.Verify(service =>
                service.AddProviderAsync(It.IsAny<Provider>(), default),
                    Times.Once);

            this.providerServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(ServerExceptions))]
        public async Task ShouldReturnInternalServerErrorOnPostIfServerErrorOccurredAsync(
            Xeption validationException)
        {
            // given
            Provider someProvider = CreateRandomProvider();

            InternalServerErrorObjectResult expectedInternalServerErrorObjectResult =
                InternalServerError(validationException);

            var expectedActionResult =
                new ActionResult<Provider>(expectedInternalServerErrorObjectResult);

            this.providerServiceMock.Setup(service =>
                service.AddProviderAsync(It.IsAny<Provider>(), default))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<Provider> actualActionResult =
                await this.providersController.PostProviderAsync(someProvider);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.providerServiceMock.Verify(service =>
                service.AddProviderAsync(It.IsAny<Provider>(), default),
                    Times.Once);

            this.providerServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnConflictOnPostIfAlreadyExistsProviderErrorOccurredAsync()
        {
            // given
            Provider someProvider = CreateRandomProvider();
            var someInnerException = new Exception();
            string someMessage = GetRandomString();

            var alreadyExistsProviderServiceException =
                new AlreadyExistsProviderServiceException(
                    message: someMessage,
                    innerException: someInnerException);

            var providerServiceDependencyValidationException =
                new ProviderServiceDependencyValidationException(
                    message: someMessage,
                    innerException: alreadyExistsProviderServiceException);

            ConflictObjectResult expectedConflictObjectResult =
                Conflict(alreadyExistsProviderServiceException);

            var expectedActionResult =
                new ActionResult<Provider>(expectedConflictObjectResult);

            this.providerServiceMock.Setup(service =>
                service.AddProviderAsync(It.IsAny<Provider>(), default))
                    .ThrowsAsync(providerServiceDependencyValidationException);

            // when
            ActionResult<Provider> actualActionResult =
                await this.providersController.PostProviderAsync(someProvider);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.providerServiceMock.Verify(service =>
                service.AddProviderAsync(It.IsAny<Provider>(), default),
                    Times.Once);

            this.providerServiceMock.VerifyNoOtherCalls();
        }
    }
}
