// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Manage.Tests.Acceptance.Models.Providers;

namespace LondonFhirService.Manage.Tests.Acceptance.Apis.Providers
{
    public partial class ProviderApiTests
    {
        [Fact]
        public async Task ShouldPostProviderAsync()
        {
            // given
            Provider randomProvider = CreateRandomProvider();
            Provider inputProvider = randomProvider;
            Provider expectedProvider = inputProvider;

            // when
            await this.apiBroker.PostProviderAsync(inputProvider);

            Provider actualProvider =
                await this.apiBroker.GetProviderByIdAsync(inputProvider.Id);

            // then
            actualProvider.Should().BeEquivalentTo(expectedProvider, options => options
                .Excluding(property => property.CreatedBy)
                .Excluding(property => property.CreatedDate)
                .Excluding(property => property.UpdatedBy)
                .Excluding(property => property.UpdatedDate));

            await this.apiBroker.DeleteProviderByIdAsync(actualProvider.Id);
        }

        [Fact]
        public async Task ShouldGetAllProvidersAsync()
        {
            // given
            List<Provider> randomProviders = await PostRandomProvidersAsync();
            List<Provider> expectedProviders = randomProviders;

            // when
            List<Provider> actualProviders = await this.apiBroker.GetAllProvidersAsync();

            // then
            foreach (Provider expectedProvider in expectedProviders)
            {
                Provider actualProvider =
                    actualProviders.Single(provider => provider.Id == expectedProvider.Id);

                actualProvider.Should().BeEquivalentTo(expectedProvider, options => options
                    .Excluding(property => property.CreatedBy)
                    .Excluding(property => property.CreatedDate)
                    .Excluding(property => property.UpdatedBy)
                    .Excluding(property => property.UpdatedDate));

                await this.apiBroker.DeleteProviderByIdAsync(actualProvider.Id);
            }
        }

        [Fact]
        public async Task ShouldGetProviderByIdAsync()
        {
            // given
            Provider randomProvider = await PostRandomProviderAsync();
            Provider expectedProvider = randomProvider;

            // when
            Provider actualProvider = await this.apiBroker.GetProviderByIdAsync(randomProvider.Id);

            // then
            actualProvider.Should().BeEquivalentTo(expectedProvider, options => options
                .Excluding(property => property.CreatedBy)
                .Excluding(property => property.CreatedDate)
                .Excluding(property => property.UpdatedBy)
                .Excluding(property => property.UpdatedDate));

            await this.apiBroker.DeleteProviderByIdAsync(actualProvider.Id);
        }

        [Fact]
        public async Task ShouldPutProviderAsync()
        {
            // given
            Provider randomProvider = await PostRandomProviderAsync();
            Provider modifiedProvider = UpdateProviderWithRandomValues(randomProvider);

            // when
            await this.apiBroker.PutProviderAsync(modifiedProvider);

            Provider actualProvider =
                await this.apiBroker.GetProviderByIdAsync(randomProvider.Id);

            // then
            actualProvider.Should().BeEquivalentTo(modifiedProvider, options => options
                .Excluding(property => property.CreatedBy)
                .Excluding(property => property.CreatedDate)
                .Excluding(property => property.UpdatedBy)
                .Excluding(property => property.UpdatedDate));

            await this.apiBroker.DeleteProviderByIdAsync(actualProvider.Id);
        }

        [Fact]
        public async Task ShouldDeleteProviderByIdAsync()
        {
            // given
            Provider randomProvider = await PostRandomProviderAsync();
            Provider expectedProvider = randomProvider;

            // when
            Provider deletedProvider =
                await this.apiBroker.DeleteProviderByIdAsync(randomProvider.Id);

            // then
            deletedProvider.Should().BeEquivalentTo(expectedProvider, options => options
                .Excluding(property => property.CreatedBy)
                .Excluding(property => property.CreatedDate)
                .Excluding(property => property.UpdatedBy)
                .Excluding(property => property.UpdatedDate));
        }
    }
}
