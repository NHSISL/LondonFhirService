// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Providers;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Providers
{
    public partial class ProviderServiceTests
    {
        [Fact]
        public async Task ShouldReturnProviders()
        {
            // given
            IQueryable<Provider> randomProviders = CreateRandomProviders();
            IQueryable<Provider> storageProviders = randomProviders;
            IQueryable<Provider> expectedProviders = storageProviders;

            this.storageBrokerMock.Setup(broker =>
                    broker.SelectAllProvidersAsync())
                .ReturnsAsync(storageProviders);

            // when
            IQueryable<Provider> actualProviders = await this.providerService.RetrieveAllProvidersAsync();

            // then
            actualProviders.Should().BeEquivalentTo(expectedProviders);

            this.storageBrokerMock.Verify(broker =>
                    broker.SelectAllProvidersAsync(),
                Times.Once());

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
