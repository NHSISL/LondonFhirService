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
        public async Task ShouldThrowValidationExceptionOnRemoveIfIdIsInvalidAndLogItAsync()
        {
            // given
            Guid invalidProviderId = Guid.Empty;

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
            ValueTask<Provider> removeProviderByIdTask =
                this.providerService.RemoveProviderByIdAsync(invalidProviderId);

            ProviderServiceValidationException actualProviderServiceValidationException =
                await Assert.ThrowsAsync<ProviderServiceValidationException>(
                    removeProviderByIdTask.AsTask);

            // then
            actualProviderServiceValidationException.Should()
                .BeEquivalentTo(expectedProviderServiceValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedProviderServiceValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteProviderAsync(It.IsAny<Provider>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
