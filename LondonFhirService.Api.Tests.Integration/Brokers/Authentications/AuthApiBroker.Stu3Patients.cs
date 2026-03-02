// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using ModelInfo = Hl7.Fhir.Model.ModelInfo;

namespace LondonFhirService.Api.Tests.Integration.Brokers.Authentications
{
    public partial class AuthApiBroker
    {
        private const string Stu3PatientRelativeUrl = "api/STU3/Patient";

        public async ValueTask<(string, Bundle)> GetStructuredRecordStu3Async(string nhsNumber, Parameters parameters)
        {
            await EnsureAccessTokenAsync(default).ConfigureAwait(false);

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
            Bundle bundle = null;

            try
            {
                bundle = fhirJsonDeserializer.Deserialize<Bundle>(responseContent);
            }
            catch (System.Exception)
            { }

            string text = responseContent;

            return (text, bundle);
        }


    }
}