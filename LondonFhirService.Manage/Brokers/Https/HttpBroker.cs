// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using LondonFhirService.Core.Brokers.Correlations;

namespace LondonFhirService.Manage.Brokers.Https
{
    public class HttpBroker : IHttpBroker
    {
        /// <summary>
        /// The least time a response body read is given, however much of the call's timeout the
        /// send already spent. Short enough that an exhausted budget is not extended meaningfully,
        /// long enough to read a refusal and report its status rather than a cancellation.
        /// </summary>
        private static readonly TimeSpan MinimumReadBudget = TimeSpan.FromSeconds(5);

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
            long startedAt = Stopwatch.GetTimestamp();

            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(formValues)
            };

            using HttpResponseMessage httpResponseMessage = await this.httpClient
                .SendAsync(
                    httpRequestMessage,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken)
                .ConfigureAwait(false);

            HttpContentResponse httpContentResponse = await this
                .ReadContentOrThrowAsync(httpResponseMessage, startedAt, cancellationToken)
                .ConfigureAwait(false);

            return httpContentResponse.Body;
        }

        public async ValueTask<HttpContentResponse> PostJsonContentAsync(
            string url,
            string jsonContent,
            string mediaType,
            string bearerToken,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            long startedAt = Stopwatch.GetTimestamp();

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
                .SendAsync(
                    httpRequestMessage,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken)
                .ConfigureAwait(false);

            return await this
                .ReadContentOrThrowAsync(httpResponseMessage, startedAt, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Empty rather than null when the upstream sent no header, so a caller can test it the
        /// same way whatever answered. Only the hosts in this solution set it - CorrelationMiddleware
        /// writes the W3C trace id in "N" form - and nothing outside them is expected to.
        /// </summary>
        private static string ReadCorrelationId(HttpResponseMessage httpResponseMessage) =>
            httpResponseMessage.Headers.TryGetValues(
                CorrelationBroker.CorrelationIdHeaderName,
                out var correlationIds)
                ? correlationIds.FirstOrDefault() ?? string.Empty
                : string.Empty;

        /// <summary>
        /// A refusal explains itself in a few hundred characters. The cap is here because the
        /// thing on the other end is not always a FHIR server answering politely - a proxy or
        /// gateway in between can return a whole HTML error page, and that should not be carried
        /// around on an exception.
        /// </summary>
        private const int MaximumResponseBodyLength = 4000;

        /// <summary>
        /// The byte ceiling on a failed response, which is what actually bounds the allocation -
        /// MaximumResponseBodyLength is a character count applied after decoding, and by then the
        /// bytes have already been held. Four bytes per character is the worst case UTF-8 can
        /// produce, so this is enough to fill the character cap from any encoding and no more.
        /// </summary>
        private const int MaximumErrorBodyBytes = 4 * MaximumResponseBodyLength;

        /// <summary>
        /// Not EnsureSuccessStatusCode, which throws before the content is read - so an
        /// authorisation server explaining itself in a 400, or a provider returning an
        /// OperationOutcome with a 404, reached the operator as a bare status line with the one
        /// useful part discarded, on a screen whose entire purpose is showing what the upstream
        /// actually said.
        ///
        /// A success is read whole: it is the payload the caller asked for. A failure is read up
        /// to MaximumErrorBodyBytes and no further, because only the first few thousand characters
        /// of it are ever kept and the thing on the other end is not always a FHIR server
        /// answering politely - a proxy or gateway in between can return a multi megabyte HTML
        /// error page. Reading the whole of that only to keep 4000 characters meant an upstream
        /// this host does not control decided how much memory the failure path used.
        ///
        /// The body travels on HttpResponseException.ResponseBody, which derives from
        /// HttpRequestException - so the exception crossing this boundary is still the native one
        /// the service maps, and the status code stays machine readable rather than only in the
        /// text.
        ///
        /// It goes on a property and deliberately NOT in the message or in Exception.Data. A
        /// provider refusing a patient lookup can say why in an OperationOutcome that names the
        /// patient; the message and Data are both parts that reach a log sink or Application
        /// Insights by default, because LoggingBroker appends a summary built from Data to every
        /// message it logs. Keeping the body off both means the identifiable part travels only
        /// where something reads it on purpose.
        /// </summary>
        private async ValueTask<HttpContentResponse> ReadContentOrThrowAsync(
            HttpResponseMessage httpResponseMessage,
            long startedAt,
            CancellationToken cancellationToken)
        {
            // Checked here as well as at the public entry points. The send between them can take
            // the whole timeout, so a caller who walked away during it is most likely to have done
            // so by the time this runs - and stopping now costs nothing, where continuing
            // allocates a linked source and starts a read for an answer nobody is waiting for.
            cancellationToken.ThrowIfCancellationRequested();

            using CancellationTokenSource readCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            readCancellation.CancelAfter(RemainingBudget(startedAt));

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                string body = await GuardTimeout(
                    httpResponseMessage.Content.ReadAsStringAsync(readCancellation.Token),
                    readCancellation,
                    cancellationToken).ConfigureAwait(false);

                return new HttpContentResponse(
                    Body: body,
                    CorrelationId: ReadCorrelationId(httpResponseMessage));
            }

            string responseBody = await GuardTimeout(
                ReadBoundedContentAsync(httpResponseMessage.Content, readCancellation.Token)
                    .AsTask(),
                readCancellation,
                cancellationToken).ConfigureAwait(false);

            // A bearer challenge, not content. An auth server rejecting the token this broker sent
            // - wrong audience, expired, malformed - answers with an empty body by design: the
            // reason lives on WWW-Authenticate instead, per RFC 6750. Falling back to it only when
            // the body is empty means a provider that DOES explain itself in the body (an
            // OperationOutcome, a token endpoint's JSON error) keeps taking priority - this is
            // filling a gap, not overriding what the upstream chose to send.
            string effectiveBody = string.IsNullOrWhiteSpace(responseBody)
                ? ReadBearerChallenge(httpResponseMessage)
                : responseBody;

            throw new HttpResponseException(
                message:
                    $"Response status code does not indicate success: " +
                        $"{(int)httpResponseMessage.StatusCode} " +
                        $"({httpResponseMessage.ReasonPhrase}).",

                statusCode: httpResponseMessage.StatusCode,
                responseBody: Truncate(effectiveBody));
        }

        /// <summary>
        /// The "Bearer ..." challenge a JwtBearer-protected upstream answers with on a 401, e.g.
        /// error="invalid_token", error_description="The audience 'https://x' is invalid". It is
        /// protocol text about the token, never patient data, so it is safe on the same property an
        /// OperationOutcome body travels on and by the same route - PatientService reads it off
        /// HttpResponseException.ResponseBody without knowing which of the two it received.
        ///
        /// Null when there is no Bearer challenge to read - a non-auth 4xx with an empty body, say
        /// - so an upstream that genuinely said nothing still reports that way rather than an empty
        /// string dressed up as a value.
        /// </summary>
        private static string ReadBearerChallenge(HttpResponseMessage httpResponseMessage) =>
            httpResponseMessage.Headers.WwwAuthenticate
                .Where(header => string.IsNullOrWhiteSpace(header.Parameter) is false)
                .Select(header => $"{header.Scheme} {header.Parameter}")
                .FirstOrDefault();

        /// <summary>
        /// HttpClient.Timeout does not cover this. It bounds the SendAsync call, and under
        /// HttpCompletionOption.ResponseHeadersRead that call returns once the headers are in -
        /// measured, a body that stalls after its headers ran until the request was abandoned
        /// rather than timing out. Nothing else would have caught it: this host registers no
        /// request timeout middleware.
        ///
        /// So the read gets a budget of its own - what is LEFT of the client's timeout after the
        /// send, not a fresh copy of it. A fresh copy meant one call could take two full timeouts:
        /// headers at 149 seconds of a 150 second budget, then another 150 for a body that never
        /// came, against a host with no request timeout middleware to stop it.
        ///
        /// A breach is rethrown in the shape HttpClient.Timeout produces - a cancelled
        /// task wrapping a TimeoutException. That is what PatientService matches on to tell a
        /// timeout from a caller who walked away, so a stalled provider still reaches the operator
        /// as "please try again" rather than as silence.
        /// </summary>
        /// <summary>
        /// What is left of the call's timeout. Floored rather than allowed to go negative, both
        /// because CancelAfter rejects a negative span and because a send that used the whole
        /// budget should still get long enough to read a short error body and report the status
        /// properly, rather than turning a 400 into a cancellation.
        ///
        /// An infinite timeout stays infinite; there is no budget to divide.
        /// </summary>
        private TimeSpan RemainingBudget(long startedAt)
        {
            if (this.httpClient.Timeout == Timeout.InfiniteTimeSpan)
            {
                return Timeout.InfiniteTimeSpan;
            }

            TimeSpan remaining = this.httpClient.Timeout - Stopwatch.GetElapsedTime(startedAt);

            return remaining < MinimumReadBudget ? MinimumReadBudget : remaining;
        }

        private static async Task<string> GuardTimeout(
            Task<string> readTask,
            CancellationTokenSource readCancellation,
            CancellationToken callerToken)
        {
            try
            {
                return await readTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException operationCanceledException)
                when (readCancellation.IsCancellationRequested
                    && callerToken.IsCancellationRequested is false)
            {
                throw new TaskCanceledException(
                    message: "The response body was not read within the configured timeout.",
                    innerException: new TimeoutException(
                        "Timed out reading the response body.",
                        operationCanceledException));
            }
        }

        /// <summary>
        /// Stops reading once there is enough to fill the character cap, rather than draining the
        /// stream and discarding the rest. Decoding is UTF-8 regardless of what the response
        /// claims: this text is shown to an operator and never parsed, and a body cut at a byte
        /// boundary can leave a partial character at the end whatever the encoding.
        /// </summary>
        private static async ValueTask<string> ReadBoundedContentAsync(
            HttpContent httpContent,
            CancellationToken cancellationToken)
        {
            using Stream contentStream = await httpContent
                .ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);

            byte[] buffer = new byte[MaximumErrorBodyBytes];
            int filled = 0;

            while (filled < buffer.Length)
            {
                int read = await contentStream
                    .ReadAsync(buffer.AsMemory(filled, buffer.Length - filled), cancellationToken)
                    .ConfigureAwait(false);

                if (read == 0)
                {
                    break;
                }

                filled += read;
            }

            return Encoding.UTF8.GetString(buffer, 0, filled);
        }

        private static string Truncate(string responseBody)
        {
            if (responseBody is null || responseBody.Length <= MaximumResponseBodyLength)
            {
                return responseBody;
            }

            return responseBody.Substring(0, MaximumResponseBodyLength) + "... [truncated]";
        }
    }
}
