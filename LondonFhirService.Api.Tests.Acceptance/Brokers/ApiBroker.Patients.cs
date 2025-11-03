// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace LondonFhirService.Api.Tests.Acceptance.Brokers
{
    public partial class ApiBroker
    {
        private const string PatientRelativeUrl = "api/R4/Patient";

        public async ValueTask<Bundle> EverythingAsync(string id, Parameters parameters)
        {
            string url = $"{PatientRelativeUrl}/{id}/$everything";
            var fhirJsonSerializer = new FhirJsonSerializer();
            string jsonContent = await fhirJsonSerializer.SerializeToStringAsync(parameters);

            var content = new StringContent(
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

            var fhirJsonParser = new FhirJsonParser();

            return fhirJsonParser.Parse<Bundle>(responseContent);
        }
    }
}