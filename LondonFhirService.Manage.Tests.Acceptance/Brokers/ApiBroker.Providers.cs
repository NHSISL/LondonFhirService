// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LondonFhirService.Manage.Tests.Acceptance.Models.Providers;

namespace LondonFhirService.Manage.Tests.Acceptance.Brokers
{
    public partial class ApiBroker
    {
        private const string providersRelativeUrl = "api/providers";

        public async ValueTask<Provider> PostProviderAsync(Provider provider) =>
            await this.apiFactoryClient.PostContentAsync(providersRelativeUrl, provider);

        public async ValueTask<List<Provider>> GetAllProvidersAsync() =>
            await this.apiFactoryClient.GetContentAsync<List<Provider>>($"{providersRelativeUrl}/");

        public async ValueTask<Provider> GetProviderByIdAsync(Guid providerId) =>
            await this.apiFactoryClient.GetContentAsync<Provider>($"{providersRelativeUrl}/{providerId}");

        public async ValueTask<Provider> PutProviderAsync(Provider provider) =>
            await this.apiFactoryClient.PutContentAsync(providersRelativeUrl, provider);

        public async ValueTask<Provider> DeleteProviderByIdAsync(Guid providerId) =>
            await this.apiFactoryClient.DeleteContentAsync<Provider>($"{providersRelativeUrl}/{providerId}");
    }
}
