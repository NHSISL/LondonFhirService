// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Manage.Brokers.Https;
using LondonFhirService.Manage.Tests.Acceptance.Models.Patients;
using Moq;

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
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(expectedStructuredRecord);

            // when
            HttpResponseMessage actualResponse =
                await this.apiBroker.PostGetStructuredRecordAsync(inputStructuredRecordRequest);

            // then
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // MVC's StringOutputFormatter writes a string action result verbatim as text/plain
            // rather than quoting it as a JSON string, so the provider's payload reaches the page
            // byte for byte. That is the contract the page's broker reads, so it is asserted here
            // rather than assumed.
            actualResponse.Content.Headers.ContentType?.MediaType.Should().Be("text/plain");

            string actualStructuredRecord = await actualResponse.Content.ReadAsStringAsync(
                TestContext.Current.CancellationToken);

            actualStructuredRecord.Should().Be(expectedStructuredRecord);

            // The token travels to the record call rather than being fetched and dropped.
            this.apiBroker.HttpBrokerMock.Verify(broker =>
                broker.PostJsonContentAsync(
                    "https://acceptance.example.nhs.uk/patient/$getstructuredrecord",
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
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new HttpRequestException(GetRandomString()));

            // when
            HttpResponseMessage actualResponse =
                await this.apiBroker.PostGetStructuredRecordAsync(inputStructuredRecordRequest);

            // then
            actualResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
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
                    It.IsAny<CancellationToken>()),
                        Times.Never);
        }

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
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(structuredRecordResponse);
        }
    }
}
