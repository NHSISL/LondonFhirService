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
        public async Task ShouldReturnBadRequestOnDeleteIfValidationErrorOccurredAsync(Xeption validationException)
        {
            // given
            Guid someId = Guid.NewGuid();

            BadRequestObjectResult expectedBadRequestObjectResult =
                BadRequest(validationException.InnerException);

            var expectedActionResult =
                new ActionResult<Provider>(expectedBadRequestObjectResult);

            this.providerServiceMock.Setup(service =>
                service.RemoveProviderByIdAsync(It.IsAny<Guid>(), default))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<Provider> actualActionResult =
                await this.providersController.DeleteProviderByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.providerServiceMock.Verify(service =>
                service.RemoveProviderByIdAsync(It.IsAny<Guid>(), default),
                    Times.Once);

            this.providerServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(ServerExceptions))]
        public async Task ShouldReturnInternalServerErrorOnDeleteIfServerErrorOccurredAsync(
            Xeption validationException)
        {
            // given
            Guid someId = Guid.NewGuid();

            InternalServerErrorObjectResult expectedInternalServerErrorObjectResult =
                InternalServerError(validationException);

            var expectedActionResult =
                new ActionResult<Provider>(expectedInternalServerErrorObjectResult);

            this.providerServiceMock.Setup(service =>
                service.RemoveProviderByIdAsync(It.IsAny<Guid>(), default))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<Provider> actualActionResult =
                await this.providersController.DeleteProviderByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.providerServiceMock.Verify(service =>
                service.RemoveProviderByIdAsync(It.IsAny<Guid>(), default),
                    Times.Once);

            this.providerServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNotFoundOnDeleteIfItemDoesNotExistAsync()
        {
            // given
            Guid someId = Guid.NewGuid();
            string someMessage = GetRandomString();

            var notFoundProviderServiceException =
                new NotFoundProviderServiceException(
                    message: someMessage);

            var providerServiceValidationException =
                new ProviderServiceValidationException(
                    message: someMessage,
                    innerException: notFoundProviderServiceException);

            NotFoundObjectResult expectedNotFoundObjectResult =
                NotFound(notFoundProviderServiceException);

            var expectedActionResult =
                new ActionResult<Provider>(expectedNotFoundObjectResult);

            this.providerServiceMock.Setup(service =>
                service.RemoveProviderByIdAsync(It.IsAny<Guid>(), default))
                    .ThrowsAsync(providerServiceValidationException);

            // when
            ActionResult<Provider> actualActionResult =
                await this.providersController.DeleteProviderByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.providerServiceMock.Verify(service =>
                service.RemoveProviderByIdAsync(It.IsAny<Guid>(), default),
                    Times.Once);

            this.providerServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnLockedOnDeleteIfRecordIsLockedAsync()
        {
            // given
            Guid someId = Guid.NewGuid();
            var someInnerException = new Exception();
            string someMessage = GetRandomString();

            var lockedProviderServiceException =
                new LockedProviderServiceException(
                    message: someMessage,
                    innerException: someInnerException);

            var providerServiceDependencyValidationException =
                new ProviderServiceDependencyValidationException(
                    message: someMessage,
                    innerException: lockedProviderServiceException);

            LockedObjectResult expectedConflictObjectResult =
                Locked(lockedProviderServiceException);

            var expectedActionResult =
                new ActionResult<Provider>(expectedConflictObjectResult);

            this.providerServiceMock.Setup(service =>
                service.RemoveProviderByIdAsync(It.IsAny<Guid>(), default))
                    .ThrowsAsync(providerServiceDependencyValidationException);

            // when
            ActionResult<Provider> actualActionResult =
                await this.providersController.DeleteProviderByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.providerServiceMock.Verify(service =>
                service.RemoveProviderByIdAsync(It.IsAny<Guid>(), default),
                    Times.Once);

            this.providerServiceMock.VerifyNoOtherCalls();
        }
    }
}
