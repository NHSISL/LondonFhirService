// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Models.Foundations.Providers.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Providers
{
    public partial class ProviderServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnRetrieveByIdIfIdIsInvalidAndLogItAsync()
        {
            // given
            var invalidProviderId = Guid.Empty;

            var invalidProviderServiceException =
                new InvalidProviderServiceException(
                    message: "Invalid provider. Please correct the errors and try again.");

            invalidProviderServiceException.AddData(
                key: nameof(Provider.Id),
                values: "Id is required");

            var expectedProviderServiceValidationException =
                new ProviderServiceValidationException(
                    message: "Provider validation errors occurred, please try again.",
                    innerException: invalidProviderServiceException);

            // when
            ValueTask<Provider> retrieveProviderByIdTask =
                this.providerService.RetrieveProviderByIdAsync(invalidProviderId);

            ProviderServiceValidationException actualProviderServiceValidationException =
                await Assert.ThrowsAsync<ProviderServiceValidationException>(
                    retrieveProviderByIdTask.AsTask);

            // then
            actualProviderServiceValidationException.Should()
                .BeEquivalentTo(expectedProviderServiceValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(
                    It.Is(SameExceptionAs(expectedProviderServiceValidationException))),
                        Times.Once());

            this.storageBrokerMock.Verify(broker =>
                broker.SelectProviderByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowNotFoundExceptionOnRetrieveByIdIfProviderIsNotFoundAndLogItAsync()
        {
            //given
            Guid someProviderId = Guid.NewGuid();
            Provider noProvider = null;

            var notFoundProviderServiceException = new NotFoundProviderServiceException(
                $"Couldn't find provider with providerId: {someProviderId}.");

            var expectedProviderServiceValidationException =
                new ProviderServiceValidationException(
                    message: "Provider validation errors occurred, please try again.",
                    innerException: notFoundProviderServiceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectProviderByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(noProvider);

            //when
            ValueTask<Provider> retrieveProviderByIdTask =
                this.providerService.RetrieveProviderByIdAsync(someProviderId);

            ProviderServiceValidationException actualProviderServiceValidationException =
                await Assert.ThrowsAsync<ProviderServiceValidationException>(
                    retrieveProviderByIdTask.AsTask);

            //then
            actualProviderServiceValidationException.Should().BeEquivalentTo(expectedProviderServiceValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectProviderByIdAsync(It.IsAny<Guid>()),
                    Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedProviderServiceValidationException))),
                    Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
