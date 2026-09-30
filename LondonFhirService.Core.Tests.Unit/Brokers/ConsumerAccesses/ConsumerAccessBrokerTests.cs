// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using FluentAssertions;
using LondonFhirService.Core.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using Moq;
using Tynamix.ObjectFiller;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Brokers.ConsumerAccesses
{
    /// <summary>
    /// ConsumerAccessService answers the access question with more than one status: 200 when access
    /// is allowed, 403 when it is refused and 401 when it does not know the consumer. The broker's
    /// job is to hand those back as they arrived - status and body together - and to fail every
    /// other status exactly as EnsureSuccessStatusCode always made it fail, so a validation or
    /// server error from the dependency is still a dependency failure one layer up.
    /// </summary>
    public class ConsumerAccessBrokerTests
    {
        private readonly Mock<TokenCredential> tokenCredentialMock;
        private readonly StubHttpMessageHandler httpMessageHandler;
        private readonly ConsumerAccessConfiguration configuration;
        private readonly ConsumerAccessBroker consumerAccessBroker;

        public ConsumerAccessBrokerTests()
        {
            this.tokenCredentialMock = new Mock<TokenCredential>();
            this.httpMessageHandler = new StubHttpMessageHandler();

            this.configuration = new ConsumerAccessConfiguration
            {
                Url = "https://consumer-access.test/api/access",
                Scope = "api://consumer-access/.default"
            };

            this.consumerAccessBroker = new ConsumerAccessBroker(
                configuration: this.configuration,
                httpClient: new HttpClient(this.httpMessageHandler),
                tokenCredential: this.tokenCredentialMock.Object);
        }

        [Theory]
        [InlineData(HttpStatusCode.OK, "application/json", null)]
        [InlineData(HttpStatusCode.Unauthorized, "application/problem+json", null)]
        [InlineData(HttpStatusCode.Unauthorized, null, "Bearer error=\"invalid_token\"")]
        [InlineData(HttpStatusCode.Forbidden, "application/json", null)]
        [InlineData(HttpStatusCode.Forbidden, "application/problem+json", null)]
        public async Task ShouldReturnTheStatusCodeAndBodyOnCheckConsumerAccessAsync(
            HttpStatusCode answeredStatusCode,
            string answeredMediaType,
            string answeredWwwAuthenticate)
        {
            // given
            string randomToken = GetRandomString();
            string randomContent = answeredMediaType is null ? string.Empty : GetRandomString();
            ValidateAccessRequest inputValidateAccessRequest = CreateRandomValidateAccessRequest();

            var expectedConsumerAccessResponse = new ConsumerAccessResponse
            {
                StatusCode = answeredStatusCode,
                ContentType = answeredMediaType,
                WwwAuthenticate = answeredWwwAuthenticate ?? string.Empty,
                Content = randomContent
            };

            this.tokenCredentialMock.Setup(credential =>
                credential.GetTokenAsync(
                    It.Is<TokenRequestContext>(context =>
                        context.Scopes.Single() == this.configuration.Scope),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new AccessToken(randomToken, DateTimeOffset.UtcNow.AddHours(1)));

            this.httpMessageHandler.Respond(
                answeredStatusCode,
                randomContent,
                answeredMediaType,
                answeredWwwAuthenticate);

            // when
            ConsumerAccessResponse actualConsumerAccessResponse =
                await this.consumerAccessBroker.CheckConsumerAccessAsync(
                    inputValidateAccessRequest,
                    TestContext.Current.CancellationToken);

            // then
            // Everything the service needs to tell the answers apart, untouched: the status, the
            // body, its media type - an Access body is application/json, a problem is
            // application/problem+json - and any WWW-Authenticate challenge, which is all a
            // gateway's bare 401 carries. Which is which is for the service to decide, not this.
            actualConsumerAccessResponse.Should().BeEquivalentTo(expectedConsumerAccessResponse);

            this.httpMessageHandler.RequestMethod.Should().Be(HttpMethod.Post);
            this.httpMessageHandler.RequestUri.Should().Be(new Uri(this.configuration.Url));
            this.httpMessageHandler.RequestAuthorization.Scheme.Should().Be("Bearer");
            this.httpMessageHandler.RequestAuthorization.Parameter.Should().Be(randomToken);

            ValidateAccessRequest actualValidateAccessRequest =
                JsonSerializer.Deserialize<ValidateAccessRequest>(
                    this.httpMessageHandler.RequestContent,
                    JsonSerializerOptions.Web);

            actualValidateAccessRequest.Should().BeEquivalentTo(inputValidateAccessRequest);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        public async Task ShouldThrowHttpRequestExceptionOnCheckConsumerAccessIfStatusIsNotAnAnswerAsync(
            HttpStatusCode failedStatusCode)
        {
            // given
            ValidateAccessRequest inputValidateAccessRequest = CreateRandomValidateAccessRequest();

            this.tokenCredentialMock.Setup(credential =>
                credential.GetTokenAsync(
                    It.IsAny<TokenRequestContext>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new AccessToken(GetRandomString(), DateTimeOffset.UtcNow.AddHours(1)));

            this.httpMessageHandler.Respond(
                failedStatusCode,
                GetRandomString(),
                mediaType: "application/json",
                wwwAuthenticate: null);

            // when
            ValueTask<ConsumerAccessResponse> checkConsumerAccessTask =
                this.consumerAccessBroker.CheckConsumerAccessAsync(
                    inputValidateAccessRequest,
                    TestContext.Current.CancellationToken);

            HttpRequestException actualHttpRequestException =
                await Assert.ThrowsAsync<HttpRequestException>(
                    checkConsumerAccessTask.AsTask);

            // then
            // The same native exception EnsureSuccessStatusCode has always thrown, so the service's
            // existing mapping of it - a critical dependency failure - is unchanged.
            actualHttpRequestException.StatusCode.Should().Be(failedStatusCode);
        }

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static ValidateAccessRequest CreateRandomValidateAccessRequest() =>
            new ValidateAccessRequest
            {
                ConsumerUserId = GetRandomString(),
                NhsNumber = GetRandomString(),
                CorrelationId = Guid.NewGuid()
            };

        /// <summary>
        /// Stands in for the dependency at the transport, so the broker's real HttpClient call runs.
        /// The request is read inside SendAsync because the broker disposes it once it returns.
        /// </summary>
        private sealed class StubHttpMessageHandler : HttpMessageHandler
        {
            private HttpStatusCode statusCode;
            private string content;
            private string mediaType;
            private string wwwAuthenticate;

            public HttpMethod RequestMethod { get; private set; }
            public Uri RequestUri { get; private set; }
            public AuthenticationHeaderValue RequestAuthorization { get; private set; }
            public string RequestContent { get; private set; }

            public void Respond(
                HttpStatusCode statusCode,
                string content,
                string mediaType,
                string wwwAuthenticate)
            {
                this.statusCode = statusCode;
                this.content = content;
                this.mediaType = mediaType;
                this.wwwAuthenticate = wwwAuthenticate;
            }

            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                this.RequestMethod = request.Method;
                this.RequestUri = request.RequestUri;
                this.RequestAuthorization = request.Headers.Authorization;
                this.RequestContent = await request.Content.ReadAsStringAsync(cancellationToken);

                // No media type means no body at all, the way a gateway's bare 401 arrives.
                HttpContent responseContent = this.mediaType is null
                    ? new ByteArrayContent(Array.Empty<byte>())
                    : new StringContent(this.content, Encoding.UTF8, this.mediaType);

                var httpResponseMessage = new HttpResponseMessage(this.statusCode)
                {
                    Content = responseContent
                };

                if (this.wwwAuthenticate is not null)
                {
                    httpResponseMessage.Headers.TryAddWithoutValidation(
                        "WWW-Authenticate",
                        this.wwwAuthenticate);
                }

                return httpResponseMessage;
            }
        }
    }
}
