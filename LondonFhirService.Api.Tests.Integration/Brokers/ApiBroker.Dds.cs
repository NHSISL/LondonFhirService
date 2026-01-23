// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using ModelInfo = Hl7.Fhir.Model.ModelInfo;

namespace LondonFhirService.Api.Tests.Integration.Brokers
{
    public partial class ApiBroker
    {
        private const string ddsAccessTokenUrl =
            "https://devauthentication.discoverydataservice.net/authenticate/$gettoken";

        private const string ddsPatientRelativeUrl =
            "https://devfhirapi.discoverydataservice.net:8443/fhirTestAPI/patient/$getstructuredrecord";

        public async ValueTask<(string, Bundle)> DdsGetStructuredRecordAsync(
            string grantType,
            string clientId,
            string clientSecret,
            string nhsNumber,
            Parameters parameters)
        {
            var options = new JsonSerializerOptions()
                .ForFhir(ModelInfo.ModelInspector);

            string url = ddsPatientRelativeUrl;
            string jsonContent = JsonSerializer.Serialize(parameters, options);

            using var content = new StringContent(
                jsonContent,
                Encoding.UTF8,
                "application/json");

            string accessToken = await GetAccessToken(grantType, clientId, clientSecret);
            string token = accessToken.Trim();

            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token["Bearer ".Length..].Trim();
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };

            request.Headers.Accept.Clear();
            // Matches Postman: Authorization: eyJhbGciOi...

            request.Headers.Remove("Authorization");
            request.Headers.Add("Authorization", accessToken.Trim());

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            HttpResponseMessage response = await this.ddsHttpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Request failed with status code {response.StatusCode}. " +
                    $"Response: {responseContent}");
            }

            FhirJsonDeserializer fhirJsonDeserializer = new();
            Bundle ddsBundle = fhirJsonDeserializer.Deserialize<Bundle>(responseContent);

            return (responseContent, ddsBundle);
        }

        private async ValueTask<string> GetAccessToken(
            string grantType,
            string clientId,
            string clientSecret,
            CancellationToken cancellationToken = default)
        {

            if (string.IsNullOrWhiteSpace(ddsAccessTokenUrl))
            {
                throw new InvalidOperationException("ddsAccessTokenUrl is missing.");
            }

            string trimmedUrl = ddsAccessTokenUrl.Trim();

            if (!Uri.TryCreate(trimmedUrl, UriKind.Absolute, out Uri? tokenUri) ||
                !string.Equals(tokenUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"ddsAccessTokenUrl must be an absolute HTTPS URL. Current value: '{ddsAccessTokenUrl}'.");
            }

            var body = new
            {
                grantType,
                clientId,
                clientSecret
            };

            string json = JsonSerializer.Serialize(body);

            using var request = new HttpRequestMessage(HttpMethod.Post, tokenUri)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            using HttpResponseMessage response =
                await this.ddsAuthHttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            string responseContent =
                await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Token request failed. Status: {(int)response.StatusCode} {response.StatusCode}. " +
                    $"Url: {tokenUri}. Response: {responseContent}");
            }

            // 4) Parse token
            using JsonDocument document = JsonDocument.Parse(responseContent);

            string? accessToken = document.RootElement.TryGetProperty("access_token", out JsonElement tokenElement)
                ? tokenElement.GetString()
                : null;

            return !string.IsNullOrWhiteSpace(accessToken)
                ? accessToken
                : throw new InvalidOperationException("access_token was missing or empty.");
        }
    }
}