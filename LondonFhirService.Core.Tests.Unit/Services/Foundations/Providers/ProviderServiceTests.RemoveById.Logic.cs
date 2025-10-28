// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.Providers;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Providers
{
    public partial class ProviderServiceTests
    {
        [Fact]
        public async Task ShouldRemoveProviderByIdAsync()
        {
            // given
            Guid randomId = Guid.NewGuid();
            Guid inputProviderId = randomId;
            Provider randomProvider = CreateRandomProvider();
            Provider storageProvider = randomProvider;
            Provider expectedInputProvider = storageProvider;
            Provider deletedProvider = expectedInputProvider;
            Provider expectedProvider = deletedProvider.DeepClone();

            this.storageBrokerMock.Setup(broker =>
                broker.SelectProviderByIdAsync(inputProviderId))
                    .ReturnsAsync(storageProvider);

            this.storageBrokerMock.Setup(broker =>
                broker.DeleteProviderAsync(expectedInputProvider))
                    .ReturnsAsync(deletedProvider);

            // when
            Provider actualProvider = await this.providerService
                .RemoveProviderByIdAsync(inputProviderId);

            // then
            actualProvider.Should().BeEquivalentTo(expectedProvider);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectProviderByIdAsync(inputProviderId),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteProviderAsync(expectedInputProvider),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
