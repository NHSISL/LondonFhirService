// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace LondonFhirService.Manage.Brokers.Https
{
    public class HttpBroker : IHttpBroker
    {
        private readonly HttpClient httpClient;

        public HttpBroker(HttpClient httpClient)
        {
            this.httpClient = httpClient;

            // Both media types, in this order. A CareConnect endpoint answers application/fhir+json
            // and an authorisation server answers application/json, and this broker serves both
            // calls, so it advertises both rather than being configured per endpoint.
            this.httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/fhir+json"));

            this.httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async ValueTask<string> PostFormUrlEncodedContentAsync(
            string url,
            IDictionary<string, string> formValues,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var formUrlEncodedContent = new FormUrlEncodedContent(formValues);

            using HttpResponseMessage httpResponseMessage = await this.httpClient
                .PostAsync(url, formUrlEncodedContent, cancellationToken)
                .ConfigureAwait(false);

            httpResponseMessage.EnsureSuccessStatusCode();

            return await httpResponseMessage.Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async ValueTask<string> PostJsonContentAsync(
            string url,
            string jsonContent,
            string bearerToken,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    content: jsonContent,
                    encoding: System.Text.Encoding.UTF8,
                    mediaType: "application/json")
            };

            // Set per request rather than on DefaultRequestHeaders. The typed client's message
            // handler is pooled and shared, so a token written onto the defaults would outlive the
            // request that supplied it and travel on the next caller's call.
            //
            // "Bearer" with a capital B, as ConsumerAccessBroker sends it. RFC 7235 makes the
            // scheme case-insensitive, so this is about matching the rest of the codebase and not
            // relying on a third-party provider having read that part of the spec.
            httpRequestMessage.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", bearerToken);

            using HttpResponseMessage httpResponseMessage = await this.httpClient
                .SendAsync(httpRequestMessage, cancellationToken)
                .ConfigureAwait(false);

            httpResponseMessage.EnsureSuccessStatusCode();

            return await httpResponseMessage.Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
