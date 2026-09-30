// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Net;
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

        public async ValueTask<ConsumerAccessResponse> CheckConsumerAccessAsync(
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

            using HttpResponseMessage httpResponseMessage = await this.httpClient
                .SendAsync(httpRequestMessage, cancellationToken)
                .ConfigureAwait(false);

            EnsureAnsweredStatusCode(httpResponseMessage);

            string content = await httpResponseMessage.Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            return new ConsumerAccessResponse
            {
                StatusCode = httpResponseMessage.StatusCode,
                ContentType = httpResponseMessage.Content.Headers.ContentType?.MediaType,
                WwwAuthenticate = httpResponseMessage.Headers.WwwAuthenticate.ToString(),
                Content = content
            };
        }

        /// <summary>
        /// EnsureSuccessStatusCode, except for 401 and 403, which the dependency uses both to answer
        /// about a consumer and to refuse this service's own credentials. Both are handed back with
        /// everything that tells them apart. Every other status still fails with the same
        /// HttpRequestException as ever, so a validation or server error from the dependency stays
        /// a dependency failure. The one check a broker cannot avoid when its resource speaks in
        /// more than one status; what each answer means is left to the service.
        /// </summary>
        private static void EnsureAnsweredStatusCode(HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                return;
            }

            httpResponseMessage.EnsureSuccessStatusCode();
        }
    }
}
