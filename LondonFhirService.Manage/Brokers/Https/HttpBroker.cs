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

            return await ReadContentOrThrowAsync(httpResponseMessage, cancellationToken)
                .ConfigureAwait(false);
        }

        public async ValueTask<string> PostJsonContentAsync(
            string url,
            string jsonContent,
            string mediaType,
            string bearerToken,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    content: jsonContent,
                    encoding: System.Text.Encoding.UTF8,
                    mediaType: mediaType)
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

            return await ReadContentOrThrowAsync(httpResponseMessage, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Reads the body first and only then looks at the status, which is the opposite order to
        /// EnsureSuccessStatusCode. That call throws before the content is read, so an
        /// authorisation server explaining itself in a 400, or a provider returning an
        /// OperationOutcome with a 404, arrived at the operator as a bare status line with the one
        /// useful part discarded - on a screen whose entire purpose is showing what the upstream
        /// actually said.
        ///
        /// The body travels on a plain HttpRequestException rather than a broker-specific type, so
        /// the exception crossing this boundary stays the native one the service already maps, and
        /// the status code stays machine readable on the exception rather than only in the text.
        /// </summary>
        private static async ValueTask<string> ReadContentOrThrowAsync(
            HttpResponseMessage httpResponseMessage,
            CancellationToken cancellationToken)
        {
            string responseBody = await httpResponseMessage.Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return responseBody;
            }

            throw new HttpRequestException(
                message:
                    $"Response status code does not indicate success: " +
                        $"{(int)httpResponseMessage.StatusCode} " +
                        $"({httpResponseMessage.ReasonPhrase}). Response body: {responseBody}",

                inner: null,
                statusCode: httpResponseMessage.StatusCode);
        }
    }
}
