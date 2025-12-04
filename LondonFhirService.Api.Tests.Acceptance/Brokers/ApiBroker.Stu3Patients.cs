// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

extern alias FhirSTU3;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FhirSTU3::Hl7.Fhir.Serialization;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using ModelInfo = FhirSTU3::Hl7.Fhir.Model.ModelInfo;

namespace LondonFhirService.Api.Tests.Acceptance.Brokers
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

        public async ValueTask<Bundle> GetStructuredDataStu3Async(string nhsNumber, Parameters parameters)
        {
            var options = new JsonSerializerOptions()
                .ForFhir(ModelInfo.ModelInspector);

            string url = $"{Stu3PatientRelativeUrl}/{nhsNumber}/$getstructuredrecord";
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
    }
}