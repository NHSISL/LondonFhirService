// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading;
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
        public async Task ShouldRetrieveProviderByIdAsync()
        {
            // given
            Provider randomProvider = CreateRandomProvider();
            Provider inputProvider = randomProvider;
            Provider storageProvider = randomProvider;
            Provider expectedProvider = storageProvider.DeepClone();

            this.storageBrokerMock.Setup(broker =>
                broker.SelectProviderByIdAsync(inputProvider.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageProvider);

            // when
            Provider actualProvider =
                await this.providerService.RetrieveProviderByIdAsync(
                    inputProvider.Id, TestContext.Current.CancellationToken);

            // then
            actualProvider.Should().BeEquivalentTo(expectedProvider);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectProviderByIdAsync(inputProvider.Id, It.IsAny<CancellationToken>()),
                    Times.Once());

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
