// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

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
        public async Task ShouldThrowValidationExceptionOnAddIfProviderIsNullAndLogItAsync()
        {
            // given
            Provider nullProvider = null;

            var nullProviderServiceException =
                new NullProviderServiceException(message: "Provider is null.");

            var expectedProviderServiceValidationException =
                new ProviderServiceValidationException(
                    message: "Provider validation errors occurred, please try again.",
                    innerException: nullProviderServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                    broker.ApplyAddAuditValuesAsync(nullProvider))
                .ReturnsAsync(nullProvider);

            // when
            ValueTask<Provider> addProviderTask =
                this.providerService.AddProviderAsync(nullProvider);

            ProviderServiceValidationException actualProviderServiceValidationException =
                await Assert.ThrowsAsync<ProviderServiceValidationException>(() =>
                    addProviderTask.AsTask());

            // then
            actualProviderServiceValidationException.Should()
                .BeEquivalentTo(expectedProviderServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(nullProvider),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedProviderServiceValidationException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
