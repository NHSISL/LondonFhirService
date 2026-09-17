// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using FluentAssertions;
using Moq;

using LondonFhirService.Manage.Brokers.Https;
using LondonFhirService.Manage.Tests.Acceptance.Models.Patients;

namespace LondonFhirService.Manage.Tests.Acceptance.Apis.Patients
{
    public partial class PatientApiTests
    {
        [Fact]
        public async Task ShouldReturnStructuredRecordOnPostGetStructuredRecordAsync()
        {
            // given
            StructuredRecordRequest inputStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            string randomAccessToken = GetRandomString();
            string tokenResponse = CreateTokenResponse(randomAccessToken);

            string expectedStructuredRecord =
                CreateStructuredRecordResponse(inputStructuredRecordRequest.NhsNumber);

            this.apiBroker.HttpBrokerMock.Setup(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(tokenResponse);

            this.apiBroker.HttpBrokerMock.Setup(broker =>
                broker.PostJsonContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new HttpContentResponse(expectedStructuredRecord, acceptanceCorrelationId));

            // when
            HttpResponseMessage actualResponse =
                await this.apiBroker.PostGetStructuredRecordAsync(inputStructuredRecordRequest);

            // then
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // The contract a browser actually gets, now that the client above sends the Accept
            // header axios sends. text/plain, and the payload byte for byte - MVC's
            // StringOutputFormatter writes a string action result verbatim rather than quoting it
            // as a JSON string.
            //
            // It is text/plain because of the */* in that header: RespectBrowserAcceptHeader is
            // false by default, so MVC disregards an Accept header containing a wildcard and takes
            // the first formatter that can write a string. Turning that option on, or reordering
            // the formatters, flips this to a quoted application/json string and breaks the page's
            // payload box - which is exactly what this assertion is here to catch.
            actualResponse.Content.Headers.ContentType?.MediaType.Should().Be("text/plain");

            string actualStructuredRecord = await actualResponse.Content.ReadAsStringAsync(
                TestContext.Current.CancellationToken);

            actualStructuredRecord.Should().Be(expectedStructuredRecord);

            // The token travels to the record call rather than being fetched and dropped.
            this.apiBroker.HttpBrokerMock.Verify(broker =>
                broker.PostJsonContentAsync(
                    "https://acceptance.example.nhs.uk/patient/$getstructuredrecord",
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    randomAccessToken,
                    It.IsAny<CancellationToken>()),
                        Times.Once);
        }

        /// <summary>
        /// The credentials on the wire are the ones the caller typed, not the host's. This is the
        /// endpoint's reason to exist - an operator proving a given consumer's credentials work -
        /// so it is asserted on the outbound form rather than inferred from a 200.
        /// </summary>
        [Fact]
        public async Task ShouldSendSuppliedCredentialsOnPostGetStructuredRecordAsync()
        {
            // given
            StructuredRecordRequest inputStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            string tokenResponse = CreateTokenResponse(GetRandomString());

            SetupSuccessfulCall(tokenResponse, CreateStructuredRecordResponse(
                inputStructuredRecordRequest.NhsNumber));

            // when
            HttpResponseMessage actualResponse =
                await this.apiBroker.PostGetStructuredRecordAsync(inputStructuredRecordRequest);

            // then
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            this.apiBroker.HttpBrokerMock.Verify(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    "https://acceptance.example.nhs.uk/token",
                    It.Is<IDictionary<string, string>>(formValues =>
                        formValues["client_id"] == inputStructuredRecordRequest.ClientId
                        && formValues["client_secret"] == inputStructuredRecordRequest.ClientSecret
                        && formValues["scope"] == inputStructuredRecordRequest.Scope
                        && formValues["grant_type"] == inputStructuredRecordRequest.GrantType),
                    It.IsAny<CancellationToken>()),
                        Times.Once);
        }

        [Fact]
        public async Task ShouldFallBackToConfiguredCredentialsOnPostGetStructuredRecordIfBlankAsync()
        {
            // given
            StructuredRecordRequest inputStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            inputStructuredRecordRequest.ClientId = string.Empty;
            inputStructuredRecordRequest.ClientSecret = null;
            inputStructuredRecordRequest.Scope = "   ";
            inputStructuredRecordRequest.GrantType = string.Empty;

            SetupSuccessfulCall(
                CreateTokenResponse(GetRandomString()),
                CreateStructuredRecordResponse(inputStructuredRecordRequest.NhsNumber));

            // when
            HttpResponseMessage actualResponse =
                await this.apiBroker.PostGetStructuredRecordAsync(inputStructuredRecordRequest);

            // then
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // The values in this suite's appsettings.json, which is what the host binds
            // PatientConfiguration from for this run.
            this.apiBroker.HttpBrokerMock.Verify(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    "https://acceptance.example.nhs.uk/token",
                    It.Is<IDictionary<string, string>>(formValues =>
                        formValues["client_id"] == "acceptance-client-id"
                        && formValues["client_secret"] == "acceptance-client-secret"
                        && formValues["scope"] == "acceptance-scope"
                        && formValues["grant_type"] == "client_credentials"),
                    It.IsAny<CancellationToken>()),
                        Times.Once);
        }

        /// <summary>
        /// A request the foundation rejects must come back as a 400 through the whole pipeline.
        /// The NHS number is left blank rather than malformed because a blank one is what a caller
        /// who submitted an empty form actually sends.
        /// </summary>
        [Fact]
        public async Task ShouldReturnBadRequestOnPostGetStructuredRecordIfNhsNumberIsBlankAsync()
        {
            // given
            StructuredRecordRequest inputStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            inputStructuredRecordRequest.NhsNumber = string.Empty;

            // when
            HttpResponseMessage actualResponse =
                await this.apiBroker.PostGetStructuredRecordAsync(inputStructuredRecordRequest);

            // then
            actualResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            string actualBody = await actualResponse.Content.ReadAsStringAsync(
                TestContext.Current.CancellationToken);

            actualBody.Should().Contain("nhsNumber");

            // Nothing goes out. A request this host already knows is unusable must not cost a
            // token exchange, let alone a patient lookup.
            this.apiBroker.HttpBrokerMock.Verify(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()),
                        Times.Never);
        }

        [Fact]
        public async Task ShouldReturnBadRequestOnPostGetStructuredRecordIfDateOfBirthIsMalformedAsync()
        {
            // given
            StructuredRecordRequest inputStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            inputStructuredRecordRequest.DateOfBirth = "01-10-2002";

            // when
            HttpResponseMessage actualResponse =
                await this.apiBroker.PostGetStructuredRecordAsync(inputStructuredRecordRequest);

            // then
            actualResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            string actualBody = await actualResponse.Content.ReadAsStringAsync(
                TestContext.Current.CancellationToken);

            actualBody.Should().Contain("dateOfBirth");
        }

        /// <summary>
        /// A request with only an NHS number is valid - the date of birth narrows a trace rather
        /// than being required to make one - and a model binding rule inferred from the bound type
        /// would reject it before the service ever ran. This is the test that catches that.
        /// </summary>
        [Fact]
        public async Task ShouldReturnOkOnPostGetStructuredRecordIfOnlyNhsNumberIsSuppliedAsync()
        {
            // given
            var inputStructuredRecordRequest = new StructuredRecordRequest
            {
                NhsNumber = GetRandomNhsNumber()
            };

            SetupSuccessfulCall(
                CreateTokenResponse(GetRandomString()),
                CreateStructuredRecordResponse(inputStructuredRecordRequest.NhsNumber));

            // when
            HttpResponseMessage actualResponse =
                await this.apiBroker.PostGetStructuredRecordAsync(inputStructuredRecordRequest);

            // then
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ShouldReturnInternalServerErrorOnPostGetStructuredRecordIfProviderFailsAsync()
        {
            // given
            StructuredRecordRequest inputStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            string failureBody =
                CreateIdentifiableRefusalBody(inputStructuredRecordRequest.NhsNumber);

            this.apiBroker.HttpBrokerMock.Setup(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(CreateTokenResponse(GetRandomString()));

            this.apiBroker.HttpBrokerMock.Setup(broker =>
                broker.PostJsonContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new HttpResponseException(
                            message: "Response status code does not indicate success: 502 " +
                                "(Bad Gateway).",
                            statusCode: HttpStatusCode.BadGateway,
                            responseBody: failureBody));

            // when
            HttpResponseMessage actualResponse =
                await this.apiBroker.PostGetStructuredRecordAsync(inputStructuredRecordRequest);

            // then
            actualResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

            ProblemDetails actualProblemDetails = await ReadProblemDetailsAsync(actualResponse);
            actualProblemDetails.Detail.Should().Be(failureBody);
        }

        [Fact]
        public async Task ShouldReturnInternalServerErrorOnPostGetStructuredRecordIfTokenIsUnusableAsync()
        {
            // given
            StructuredRecordRequest inputStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            this.apiBroker.HttpBrokerMock.Setup(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync("{\"token_type\":\"Bearer\"}");

            // when
            HttpResponseMessage actualResponse =
                await this.apiBroker.PostGetStructuredRecordAsync(inputStructuredRecordRequest);

            // then
            actualResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

            this.apiBroker.HttpBrokerMock.Verify(broker =>
                broker.PostJsonContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                        Times.Never);
        }

        /// <summary>
        /// The scenario this screen exists for: an operator tries a consumer's credentials and the
        /// token endpoint rejects them. That is the caller's to fix, so it has to come back as a
        /// 400 rather than a 500 inviting them to contact support.
        /// </summary>
        [Fact]
        public async Task ShouldReturnBadRequestOnPostGetStructuredRecordIfUpstreamRejectsCredentialsAsync()
        {
            // given
            StructuredRecordRequest inputStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            string refusalBody =
                "{\"error\":\"invalid_client\",\"error_description\":\"AADSTS7000215: " +
                    "Invalid client secret provided.\"}";

            this.apiBroker.HttpBrokerMock.Setup(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new HttpResponseException(
                            message: "Response status code does not indicate success: 401 " +
                                "(Unauthorized).",
                            statusCode: HttpStatusCode.Unauthorized,
                            responseBody: refusalBody));

            // when
            HttpResponseMessage actualResponse =
                await this.apiBroker.PostGetStructuredRecordAsync(inputStructuredRecordRequest);

            // then
            actualResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // The whole point of the endpoint: the operator sees the token endpoint's own words,
            // not a summary of them.
            ProblemDetails actualProblemDetails = await ReadProblemDetailsAsync(actualResponse);
            actualProblemDetails.Detail.Should().Be(refusalBody);

            this.apiBroker.HttpBrokerMock.Verify(broker =>
                broker.PostJsonContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                        Times.Never);
        }

        /// <summary>
        /// The id has to survive the hop. The Api answers with X-Correlation-Id, this host reads
        /// it off a response it then disposes, and the page needs it to link an operator on to the
        /// comparisons that same call produced - so if it is dropped anywhere in between, the link
        /// silently goes nowhere.
        /// </summary>
        [Fact]
        public async Task ShouldEchoTheUpstreamCorrelationIdOnPostGetStructuredRecordAsync()
        {
            // given
            StructuredRecordRequest inputStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            SetupSuccessfulCall(
                CreateTokenResponse(GetRandomString()),
                CreateStructuredRecordResponse(inputStructuredRecordRequest.NhsNumber));

            // when
            HttpResponseMessage actualResponse =
                await this.apiBroker.PostGetStructuredRecordAsync(inputStructuredRecordRequest);

            // then
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            actualResponse.Headers.GetValues("X-Correlation-Id")
                .Should().ContainSingle().Which.Should().Be(acceptanceCorrelationId);
        }

        /// <summary>
        /// An older Api sends no such header, and an empty one reads like an id the caller failed
        /// to parse - so the header is left off entirely rather than sent blank.
        /// </summary>
        [Fact]
        public async Task ShouldOmitTheCorrelationIdHeaderWhenTheUpstreamSentNoneAsync()
        {
            // given
            StructuredRecordRequest inputStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            this.apiBroker.HttpBrokerMock.Setup(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(CreateTokenResponse(GetRandomString()));

            this.apiBroker.HttpBrokerMock.Setup(broker =>
                broker.PostJsonContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new HttpContentResponse(
                            CreateStructuredRecordResponse(inputStructuredRecordRequest.NhsNumber),
                            string.Empty));

            // when
            HttpResponseMessage actualResponse =
                await this.apiBroker.PostGetStructuredRecordAsync(inputStructuredRecordRequest);

            // then
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            actualResponse.Headers.Contains("X-Correlation-Id").Should().BeFalse();
        }

        /// <summary>
        /// Fixed rather than random so a test can name it in an assertion. It is the value the
        /// mocked broker reports the Api answered with, and the controller is expected to echo it
        /// onto its own response header untouched.
        /// </summary>
        private const string acceptanceCorrelationId = "9f2c41be7a0d4e5bb6c8d3117e42a905";

        private static async ValueTask<ProblemDetails> ReadProblemDetailsAsync(
            HttpResponseMessage httpResponseMessage)
        {
            string responseBody = await httpResponseMessage.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ProblemDetails>(
                responseBody,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }

        /// <summary>
        /// A refusal shaped the way a provider actually refuses - an OperationOutcome naming the
        /// patient. Using a realistic one matters: the assertions are about where this text is
        /// allowed to travel.
        /// </summary>
        private static string CreateIdentifiableRefusalBody(string nhsNumber) =>
            "{\"resourceType\":\"OperationOutcome\",\"issue\":[{\"severity\":\"error\"," +
                "\"diagnostics\":\"Patient " + nhsNumber + " not found\"}]}";

        private void SetupSuccessfulCall(string tokenResponse, string structuredRecordResponse)
        {
            this.apiBroker.HttpBrokerMock.Setup(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(tokenResponse);

            this.apiBroker.HttpBrokerMock.Setup(broker =>
                broker.PostJsonContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new HttpContentResponse(structuredRecordResponse, acceptanceCorrelationId));
        }
    }
}
