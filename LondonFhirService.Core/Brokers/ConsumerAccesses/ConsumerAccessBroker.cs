// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;

namespace LondonFhirService.Core.Brokers.ConsumerAccesses
{
    public class ConsumerAccessBroker : IConsumerAccessBroker
    {
        private readonly ConsumerAccessConfiguration configuration;
        private readonly HttpClient httpClient;
        private readonly TokenCredential tokenCredential;

        public ConsumerAccessBroker(
            ConsumerAccessConfiguration configuration,
            HttpClient httpClient,
            TokenCredential tokenCredential)
        {
            this.configuration = configuration;
            this.httpClient = httpClient;
            this.tokenCredential = tokenCredential;
        }

        public async ValueTask<ConsumerAccess> CheckConsumerAccessAsync(
            ValidateAccessRequest request,
            CancellationToken cancellationToken = default)
        {
            AccessToken accessToken = await this.tokenCredential
                .GetTokenAsync(
                    new TokenRequestContext(new[] { this.configuration.Scope }),
                    cancellationToken)
                .ConfigureAwait(false);

            using var httpRequestMessage =
                new HttpRequestMessage(HttpMethod.Post, this.configuration.Url)
                {
                    Content = JsonContent.Create(request)
                };

            httpRequestMessage.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken.Token);

            HttpResponseMessage httpResponseMessage = await this.httpClient
                .SendAsync(httpRequestMessage, cancellationToken)
                .ConfigureAwait(false);

            httpResponseMessage.EnsureSuccessStatusCode();

            return await httpResponseMessage.Content
                .ReadFromJsonAsync<ConsumerAccess>(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
