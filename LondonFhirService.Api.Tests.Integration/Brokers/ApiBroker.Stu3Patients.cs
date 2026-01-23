// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using ModelInfo = Hl7.Fhir.Model.ModelInfo;

namespace LondonFhirService.Api.Tests.Integration.Brokers
{
    public partial class ApiBroker
    {
        private const string Stu3PatientRelativeUrl = "api/STU3/Patient";

        public async ValueTask<Bundle> EverythingStu3Async(string id, Parameters parameters)
        {
            var options = new JsonSerializerOptions()
                .ForFhir(ModelInfo.ModelInspector);

            string url = $"{Stu3PatientRelativeUrl}/{id}/$everything";
            string jsonContent = JsonSerializer.Serialize(parameters, options);

            using var content = new StringContent(
                jsonContent,
                Encoding.UTF8,
                "application/fhir+json");

            HttpResponseMessage response = await this.httpClient.PostAsync(url, content);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Request failed with status code {response.StatusCode}. " +
                    $"Response: {responseContent}");
            }

            FhirJsonDeserializer fhirJsonDeserializer = new();
            Bundle bundle = fhirJsonDeserializer.Deserialize<Bundle>(responseContent);

            return bundle;
        }

        public async ValueTask<(string, Bundle)> GetStructuredRecordStu3Async(string nhsNumber, Parameters parameters)
        {
            var options = new JsonSerializerOptions()
                .ForFhir(ModelInfo.ModelInspector);

            string url = $"{Stu3PatientRelativeUrl}/$getstructuredrecord";
            string jsonContent = JsonSerializer.Serialize(parameters, options);

            using var content = new StringContent(
                jsonContent,
                Encoding.UTF8,
                "application/fhir+json");

            HttpResponseMessage response = await this.httpClient.PostAsync(url, content);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Request failed with status code {response.StatusCode}. " +
                    $"Response: {responseContent}");
            }

            FhirJsonDeserializer fhirJsonDeserializer = new();
            Bundle bundle = fhirJsonDeserializer.Deserialize<Bundle>(responseContent);
            string text = responseContent;


            return (text, bundle);
        }

        private async ValueTask<string> GetAccessTokenAsync()
        {
            var credential = new DefaultAzureCredential();
            var tokenRequestContext = new TokenRequestContext(new[] { "https://graph.microsoft.com/.default" });
            AccessToken accessToken = await credential.GetTokenAsync(tokenRequestContext);

            return accessToken.Token;
        }
    }
}