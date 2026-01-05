// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

extern alias FhirSTU3;

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FhirSTU3::Hl7.Fhir.Serialization;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using ModelInfo = FhirSTU3::Hl7.Fhir.Model.ModelInfo;

namespace LondonFhirService.Api.Tests.Integration.Brokers
{
    public partial class ApiBroker
    {
        private const string ddsAccessTokenUrl =
            "https://devauthentication.discoverydataservice.net/authenticate/$gettoken";

        private const string ddsPatientRelativeUrl =
            "https://devfhirapi.discoverydataservice.net:8443/fhirTestAPI/patient/$getstructuredrecord";

        public async ValueTask<Bundle> DdsGetStructuredRecordAsync(
            string grantType,
            string clientId,
            string clientSecret,
            string nhsNumber,
            Parameters parameters)
        {
            var options = new JsonSerializerOptions()
                .ForFhir(ModelInfo.ModelInspector);

            string url = $"{Stu3PatientRelativeUrl}/$getstructuredrecord";
            string jsonContent = JsonSerializer.Serialize(parameters, options);

            using var content = new StringContent(
                jsonContent,
                Encoding.UTF8,
                "application/fhir+json");

            string accessToken = await GetAccessToken(grantType, clientId, clientSecret);

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };

            // Matches Postman: Authorization: eyJhbGciOi...
            request.Headers.Add("Authorization", accessToken);

            HttpResponseMessage response = await this.httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Request failed with status code {response.StatusCode}. " +
                    $"Response: {responseContent}");
            }

            FhirJsonDeserializer fhirJsonDeserializer = new();
            return fhirJsonDeserializer.Deserialize<Bundle>(responseContent);
        }

        private async ValueTask<string> GetAccessToken(string grantType, string clientId, string clientSecret)
        {
            var body = new
            {
                grantType,
                clientId,
                clientSecret
            };

            string json = JsonSerializer.Serialize(body);

            HttpResponseMessage response = await this.httpClient.PostAsync(
                ddsAccessTokenUrl,
                new StringContent(json, Encoding.UTF8, "application/json"));


            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Request failed with status code {response.StatusCode}. " +
                    $"Response: {responseContent}");
            }

            using JsonDocument document = JsonDocument.Parse(responseContent);

            string accessToken = document.RootElement
                .GetProperty("access_token")
                .GetString()
                ?? throw new InvalidOperationException("access_token was missing.");

            return accessToken;
        }
    }
}