// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Providers;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Providers
{
    public partial class ProviderServiceTests
    {
        [Fact]
        public async Task ShouldReturnProvidersAsListAsync()
        {
            // given
            List<Provider> randomProviders = CreateRandomProviders().ToList();
            List<Provider> storageProviders = randomProviders;
            List<Provider> expectedProviders = storageProviders;

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllProvidersAsListAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageProviders);

            // when
            List<Provider> actualProviders =
                await this.providerService.RetrieveAllProvidersAsListAsync(TestContext.Current.CancellationToken);

            // then
            actualProviders.Should().BeEquivalentTo(expectedProviders);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllProvidersAsListAsync(It.IsAny<CancellationToken>()),
                    Times.Once());

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
