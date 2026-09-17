// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Manage.Models.Foundations.Patients;
using Moq;

namespace LondonFhirService.Manage.Tests.Unit.Services.Foundations.Patients
{
    public partial class PatientServiceTests
    {
        [Fact]
        public async Task ShouldReturnStructuredRecordOnGetStructuredRecordAsync()
        {
            // given
            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;
            string randomAccessToken = GetRandomString();
            string tokenResponse = CreateTokenResponse(randomAccessToken);
            string randomStructuredRecord = GetRandomString();
            string expectedStructuredRecord = randomStructuredRecord;

            // Matched exactly on both hops rather than with It.IsAny, so the caller's token has to
            // reach the broker unchanged. With It.IsAny the service could drop it, or substitute
            // default, and this test would still pass - which would leave a request that nobody is
            // waiting for running to completion against a provider.
            CancellationToken inputCancellationToken = TestContext.Current.CancellationToken;

            Dictionary<string, string> expectedFormValues = CreateExpectedFormValues(
                clientId: inputStructuredRecordRequest.ClientId,
                clientSecret: inputStructuredRecordRequest.ClientSecret,
                scope: inputStructuredRecordRequest.Scope,
                grantType: inputStructuredRecordRequest.GrantType);

            this.httpBrokerMock.Setup(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    this.patientConfiguration.AuthUrl,
                    It.Is<IDictionary<string, string>>(formValues =>
                        SameFormValuesAs(formValues, expectedFormValues)),
                    inputCancellationToken))
                        .ReturnsAsync(tokenResponse);

            this.httpBrokerMock.Setup(broker =>
                broker.PostJsonContentAsync(
                    this.patientConfiguration.GetStructuredRecordUrl,
                    It.Is<string>(requestBody =>
                        requestBody.Contains("\"resourceType\":\"Parameters\"")),
                    "application/fhir+json",
                    randomAccessToken,
                    inputCancellationToken))
                        .ReturnsAsync(randomStructuredRecord);

            // when
            string actualStructuredRecord = await this.patientService.GetStructuredRecordAsync(
                inputStructuredRecordRequest,
                inputCancellationToken);

            // then
            actualStructuredRecord.Should().BeEquivalentTo(expectedStructuredRecord);

            this.httpBrokerMock.Verify(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    this.patientConfiguration.AuthUrl,
                    It.Is<IDictionary<string, string>>(formValues =>
                        SameFormValuesAs(formValues, expectedFormValues)),
                    inputCancellationToken),
                        Times.Once);

            this.httpBrokerMock.Verify(broker =>
                broker.PostJsonContentAsync(
                    this.patientConfiguration.GetStructuredRecordUrl,
                    It.Is<string>(requestBody =>
                        requestBody.Contains("\"resourceType\":\"Parameters\"")),

                    // Pinned, not It.IsAny. A FHIR server is entitled to answer plain
                    // application/json with a 415, and every other caller of this operation in
                    // the solution sends application/fhir+json.
                    "application/fhir+json",

                    randomAccessToken,
                    inputCancellationToken),
                        Times.Once);

            this.httpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// The point of the four credential inputs on the page: a blank one uses what the host is
        /// configured with, a filled one overrides it. Both halves are asserted in one test
        /// because the behaviour is the pairing, not either side on its own.
        /// </summary>
        [Fact]
        public async Task ShouldFallBackToConfiguredCredentialsOnGetStructuredRecordIfRequestCredentialsAreBlankAsync()
        {
            // given
            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;
            string suppliedClientId = inputStructuredRecordRequest.ClientId;
            inputStructuredRecordRequest.ClientSecret = string.Empty;
            inputStructuredRecordRequest.Scope = null;
            inputStructuredRecordRequest.GrantType = "   ";
            string randomAccessToken = GetRandomString();
            string tokenResponse = CreateTokenResponse(randomAccessToken);
            string randomStructuredRecord = GetRandomString();
            string expectedStructuredRecord = randomStructuredRecord;
            CancellationToken inputCancellationToken = TestContext.Current.CancellationToken;

            Dictionary<string, string> expectedFormValues = CreateExpectedFormValues(
                clientId: suppliedClientId,
                clientSecret: this.patientConfiguration.ClientSecret,
                scope: this.patientConfiguration.Scope,
                grantType: this.patientConfiguration.GrantType);

            this.httpBrokerMock.Setup(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    this.patientConfiguration.AuthUrl,
                    It.Is<IDictionary<string, string>>(formValues =>
                        SameFormValuesAs(formValues, expectedFormValues)),
                    inputCancellationToken))
                        .ReturnsAsync(tokenResponse);

            this.httpBrokerMock.Setup(broker =>
                broker.PostJsonContentAsync(
                    this.patientConfiguration.GetStructuredRecordUrl,
                    It.Is<string>(requestBody =>
                        requestBody.Contains("\"resourceType\":\"Parameters\"")),
                    "application/fhir+json",
                    randomAccessToken,
                    inputCancellationToken))
                        .ReturnsAsync(randomStructuredRecord);

            // when
            string actualStructuredRecord = await this.patientService.GetStructuredRecordAsync(
                inputStructuredRecordRequest,
                inputCancellationToken);

            // then
            actualStructuredRecord.Should().BeEquivalentTo(expectedStructuredRecord);

            this.httpBrokerMock.Verify(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    this.patientConfiguration.AuthUrl,
                    It.Is<IDictionary<string, string>>(formValues =>
                        SameFormValuesAs(formValues, expectedFormValues)),
                    inputCancellationToken),
                        Times.Once);

            this.httpBrokerMock.Verify(broker =>
                broker.PostJsonContentAsync(
                    this.patientConfiguration.GetStructuredRecordUrl,
                    It.Is<string>(requestBody =>
                        requestBody.Contains("\"resourceType\":\"Parameters\"")),
                    "application/fhir+json",
                    randomAccessToken,
                    inputCancellationToken),
                        Times.Once);

            this.httpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldSendCareConnectParametersOnGetStructuredRecordAsync()
        {
            // given
            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;
            inputStructuredRecordRequest.DemographicsOnly = true;
            string randomAccessToken = GetRandomString();
            string tokenResponse = CreateTokenResponse(randomAccessToken);
            string capturedRequestBody = null;
            CancellationToken inputCancellationToken = TestContext.Current.CancellationToken;

            Dictionary<string, string> expectedFormValues = CreateExpectedFormValues(
                clientId: inputStructuredRecordRequest.ClientId,
                clientSecret: inputStructuredRecordRequest.ClientSecret,
                scope: inputStructuredRecordRequest.Scope,
                grantType: inputStructuredRecordRequest.GrantType);

            this.httpBrokerMock.Setup(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    this.patientConfiguration.AuthUrl,
                    It.Is<IDictionary<string, string>>(formValues =>
                        SameFormValuesAs(formValues, expectedFormValues)),
                    inputCancellationToken))
                        .ReturnsAsync(tokenResponse);

            this.httpBrokerMock.Setup(broker =>
                broker.PostJsonContentAsync(
                    this.patientConfiguration.GetStructuredRecordUrl,
                    It.Is<string>(requestBody =>
                        requestBody.Contains("\"resourceType\":\"Parameters\"")),
                    "application/fhir+json",
                    randomAccessToken,
                    inputCancellationToken))
                        .Callback((
                            string url,
                            string jsonContent,
                            string mediaType,
                            string bearerToken,
                            CancellationToken cancellationToken) => capturedRequestBody = jsonContent)
                        .ReturnsAsync(GetRandomString());

            // when
            await this.patientService.GetStructuredRecordAsync(
                inputStructuredRecordRequest,
                inputCancellationToken);

            // then
            using JsonDocument actualRequestBody = JsonDocument.Parse(capturedRequestBody);
            JsonElement root = actualRequestBody.RootElement;
            root.GetProperty("resourceType").GetString().Should().Be("Parameters");

            root.GetProperty("meta").GetProperty("profile")[0].GetString().Should().Be(
                "https://fhir.hl7.org.uk/STU3/OperationDefinition/" +
                    "CareConnect-GetStructuredRecord-Operation-1");

            JsonElement parameters = root.GetProperty("parameter");
            parameters.GetArrayLength().Should().Be(4);

            JsonElement nhsNumberParameter = parameters[0];
            nhsNumberParameter.GetProperty("name").GetString().Should().Be("patientNHSNumber");

            nhsNumberParameter.GetProperty("valueIdentifier").GetProperty("system").GetString()
                .Should().Be("https://fhir.hl7.org.uk/Id/nhs-number");

            nhsNumberParameter.GetProperty("valueIdentifier").GetProperty("value").GetString()
                .Should().Be(inputStructuredRecordRequest.NhsNumber);

            JsonElement demographicsOnlyParameter = parameters[1];
            demographicsOnlyParameter.GetProperty("name").GetString().Should().Be("demographicsOnly");

            demographicsOnlyParameter.GetProperty("part")[0].GetProperty("valueBoolean")
                .GetBoolean().Should().BeTrue();

            JsonElement includeInactivePatientsParameter = parameters[2];

            includeInactivePatientsParameter.GetProperty("name").GetString()
                .Should().Be("includeInactivePatients");

            includeInactivePatientsParameter.GetProperty("part")[0].GetProperty("valueBoolean")
                .GetBoolean().Should().BeFalse();

            JsonElement dateOfBirthParameter = parameters[3];
            dateOfBirthParameter.GetProperty("name").GetString().Should().Be("patientDOB");

            dateOfBirthParameter.GetProperty("valueIdentifier").GetProperty("system").GetString()
                .Should().Be("https://fhir.hl7.org.uk/Id/dob");

            dateOfBirthParameter.GetProperty("valueIdentifier").GetProperty("value").GetString()
                .Should().Be(inputStructuredRecordRequest.DateOfBirth);

            this.httpBrokerMock.Verify(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    this.patientConfiguration.AuthUrl,
                    It.Is<IDictionary<string, string>>(formValues =>
                        SameFormValuesAs(formValues, expectedFormValues)),
                    inputCancellationToken),
                        Times.Once);

            this.httpBrokerMock.Verify(broker =>
                broker.PostJsonContentAsync(
                    this.patientConfiguration.GetStructuredRecordUrl,
                    It.Is<string>(requestBody =>
                        requestBody.Contains("\"resourceType\":\"Parameters\"")),
                    "application/fhir+json",
                    randomAccessToken,
                    inputCancellationToken),
                        Times.Once);

            this.httpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Validation parses the date from a trimmed copy, so a padded value passed the checks and
        /// was then sent with its whitespace intact for the provider to reject. The values on the
        /// wire are now trimmed too.
        /// </summary>
        [Fact]
        public async Task ShouldTrimPatientValuesOnGetStructuredRecordBeforeSendingAsync()
        {
            // given
            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;
            string expectedNhsNumber = inputStructuredRecordRequest.NhsNumber;
            string expectedDateOfBirth = inputStructuredRecordRequest.DateOfBirth;
            inputStructuredRecordRequest.NhsNumber = $"  {expectedNhsNumber} ";
            inputStructuredRecordRequest.DateOfBirth = $" {expectedDateOfBirth}   ";
            string capturedRequestBody = null;
            string randomAccessToken = GetRandomString();
            CancellationToken inputCancellationToken = TestContext.Current.CancellationToken;

            Dictionary<string, string> expectedFormValues = CreateExpectedFormValues(
                clientId: inputStructuredRecordRequest.ClientId,
                clientSecret: inputStructuredRecordRequest.ClientSecret,
                scope: inputStructuredRecordRequest.Scope,
                grantType: inputStructuredRecordRequest.GrantType);

            this.httpBrokerMock.Setup(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    this.patientConfiguration.AuthUrl,
                    It.Is<IDictionary<string, string>>(formValues =>
                        SameFormValuesAs(formValues, expectedFormValues)),
                    inputCancellationToken))
                        .ReturnsAsync(CreateTokenResponse(randomAccessToken));

            this.httpBrokerMock.Setup(broker =>
                broker.PostJsonContentAsync(
                    this.patientConfiguration.GetStructuredRecordUrl,
                    It.Is<string>(requestBody =>
                        requestBody.Contains("\"resourceType\":\"Parameters\"")),
                    "application/fhir+json",
                    randomAccessToken,
                    inputCancellationToken))
                        .Callback((
                            string url,
                            string jsonContent,
                            string mediaType,
                            string bearerToken,
                            CancellationToken cancellationToken) => capturedRequestBody = jsonContent)
                        .ReturnsAsync(GetRandomString());

            // when
            await this.patientService.GetStructuredRecordAsync(
                inputStructuredRecordRequest,
                inputCancellationToken);

            // then
            using JsonDocument actualRequestBody = JsonDocument.Parse(capturedRequestBody);
            JsonElement parameters = actualRequestBody.RootElement.GetProperty("parameter");

            parameters[0].GetProperty("valueIdentifier").GetProperty("value").GetString()
                .Should().Be(expectedNhsNumber);

            parameters[3].GetProperty("valueIdentifier").GetProperty("value").GetString()
                .Should().Be(expectedDateOfBirth);

            this.httpBrokerMock.Verify(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    this.patientConfiguration.AuthUrl,
                    It.Is<IDictionary<string, string>>(formValues =>
                        SameFormValuesAs(formValues, expectedFormValues)),
                    inputCancellationToken),
                        Times.Once);

            this.httpBrokerMock.Verify(broker =>
                broker.PostJsonContentAsync(
                    this.patientConfiguration.GetStructuredRecordUrl,
                    It.Is<string>(requestBody =>
                        requestBody.Contains("\"resourceType\":\"Parameters\"")),
                    "application/fhir+json",
                    randomAccessToken,
                    inputCancellationToken),
                        Times.Once);

            this.httpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// A trace on the NHS number alone is a valid call, and the parameter is omitted rather
        /// than sent empty - an empty patientDOB identifier is not the same request as no
        /// patientDOB at all.
        /// </summary>
        [Fact]
        public async Task ShouldOmitDateOfBirthParameterOnGetStructuredRecordIfDateOfBirthIsBlankAsync()
        {
            // given
            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;
            inputStructuredRecordRequest.DateOfBirth = string.Empty;
            string randomAccessToken = GetRandomString();
            string tokenResponse = CreateTokenResponse(randomAccessToken);
            string capturedRequestBody = null;
            CancellationToken inputCancellationToken = TestContext.Current.CancellationToken;

            Dictionary<string, string> expectedFormValues = CreateExpectedFormValues(
                clientId: inputStructuredRecordRequest.ClientId,
                clientSecret: inputStructuredRecordRequest.ClientSecret,
                scope: inputStructuredRecordRequest.Scope,
                grantType: inputStructuredRecordRequest.GrantType);

            this.httpBrokerMock.Setup(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    this.patientConfiguration.AuthUrl,
                    It.Is<IDictionary<string, string>>(formValues =>
                        SameFormValuesAs(formValues, expectedFormValues)),
                    inputCancellationToken))
                        .ReturnsAsync(tokenResponse);

            this.httpBrokerMock.Setup(broker =>
                broker.PostJsonContentAsync(
                    this.patientConfiguration.GetStructuredRecordUrl,
                    It.Is<string>(requestBody =>
                        requestBody.Contains("\"resourceType\":\"Parameters\"")),
                    "application/fhir+json",
                    randomAccessToken,
                    inputCancellationToken))
                        .Callback((
                            string url,
                            string jsonContent,
                            string mediaType,
                            string bearerToken,
                            CancellationToken cancellationToken) => capturedRequestBody = jsonContent)
                        .ReturnsAsync(GetRandomString());

            // when
            await this.patientService.GetStructuredRecordAsync(
                inputStructuredRecordRequest,
                inputCancellationToken);

            // then
            using JsonDocument actualRequestBody = JsonDocument.Parse(capturedRequestBody);
            JsonElement parameters = actualRequestBody.RootElement.GetProperty("parameter");
            parameters.GetArrayLength().Should().Be(3);

            foreach (JsonElement parameter in parameters.EnumerateArray())
            {
                parameter.GetProperty("name").GetString().Should().NotBe("patientDOB");
            }

            this.httpBrokerMock.Verify(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    this.patientConfiguration.AuthUrl,
                    It.Is<IDictionary<string, string>>(formValues =>
                        SameFormValuesAs(formValues, expectedFormValues)),
                    inputCancellationToken),
                        Times.Once);

            this.httpBrokerMock.Verify(broker =>
                broker.PostJsonContentAsync(
                    this.patientConfiguration.GetStructuredRecordUrl,
                    It.Is<string>(requestBody =>
                        requestBody.Contains("\"resourceType\":\"Parameters\"")),
                    "application/fhir+json",
                    randomAccessToken,
                    inputCancellationToken),
                        Times.Once);

            this.httpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
