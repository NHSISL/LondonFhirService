// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

extern alias FhirR4;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using ModelInfo = FhirR4::Hl7.Fhir.Model.ModelInfo;

namespace LondonFhirService.Api.Tests.Integration.Brokers
{
    public partial class ApiBroker
    {
        private const string R4PatientRelativeUrl = "api/R4/R4Patient";

        public async ValueTask<Bundle> EverythingR4Async(string id, Parameters parameters)
        {
            var options = new JsonSerializerOptions()
                       .ForFhir(ModelInfo.ModelInspector);

            string url = $"{R4PatientRelativeUrl}/{id}/$everything";
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

            var bundle = JsonSerializer.Deserialize<Bundle>(responseContent, options);

            return bundle;
        }
    }
}