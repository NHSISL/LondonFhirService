// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Api.Tests.Performance
{
    public partial class GetStructuredDataTests
    {
        [Theory]
        [InlineData("9355272538")]
        [InlineData("9999888224")]
        [InlineData("9355272536")]
        [InlineData("8751482452")]
        public async Task ShouldGetStructuredRecordFromLondonFhirServiceAsync(string nhsNumber)
        {
            // given
            string inputNhsNumber = nhsNumber;
            string? inputDateOfBirth = null;
            bool? inputDemographicsOnly = null;
            bool? inputIncludeInactivePatients = null;
            string accessToken = await GetLfsAccessToken();

            Parameters inputParameters = CreateGetStructuredRecordParameters(
                nhsNumber: inputNhsNumber,
                dateOfBirth: inputDateOfBirth,
                demographicsOnly: inputDemographicsOnly,
                includeInactivePatients: inputIncludeInactivePatients);

            var options = new JsonSerializerOptions()
                .ForFhir(ModelInfo.ModelInspector);

            string jsonContent = JsonSerializer.Serialize(inputParameters, options);

            using var content = new StringContent(
                jsonContent,
                Encoding.UTF8,
                "application/fhir+json");

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();

            httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/fhir+json"));

            httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var stopwatch = Stopwatch.StartNew();
            HttpResponseMessage response = await httpClient.PostAsync(consumerAccessTokenSettings.LfsGetStructuredRecordUrl, content);
            string responseContent = await response.Content.ReadAsStringAsync();
            stopwatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Request failed with status code {response.StatusCode}. " +
                    $"Response: {responseContent}");
            }

            string text = responseContent;
            output.WriteLine($"Time taken to get structured record: {stopwatch.ElapsedMilliseconds} ms");
            output.WriteLine("");
            output.WriteLine("");
            string outputDirectory = Path.Combine(AppContext.BaseDirectory, "StructuredRecords");
            Directory.CreateDirectory(outputDirectory);
            string filePath = Path.Combine(outputDirectory, $"{nhsNumber}-LFS.txt");
            await File.WriteAllTextAsync(filePath, text);
            output.WriteLine($"Structured record written to: {filePath}");
        }

        [Theory]
        [InlineData("9355272538")]
        [InlineData("9999888224")]
        [InlineData("9355272536")]
        [InlineData("8751482452")]
        public async Task ShouldGetStructuredRecordFromDdsAsync(string nhsNumber)
        {
            // given
            string inputNhsNumber = nhsNumber;
            string? inputDateOfBirth = null;
            bool? inputDemographicsOnly = null;
            bool? inputIncludeInactivePatients = null;
            string accessToken = await GetDdsAccessToken();

            Parameters inputParameters = CreateGetStructuredRecordParameters(
                nhsNumber: inputNhsNumber,
                dateOfBirth: inputDateOfBirth,
                demographicsOnly: inputDemographicsOnly,
                includeInactivePatients: inputIncludeInactivePatients);

            var options = new JsonSerializerOptions()
                .ForFhir(ModelInfo.ModelInspector);

            string jsonContent = JsonSerializer.Serialize(inputParameters, options);

            using var content = new StringContent(
                jsonContent,
                Encoding.UTF8,
                "application/json");

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();

            httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"{accessToken}");
            var stopwatch = Stopwatch.StartNew();
            HttpResponseMessage response = await httpClient.PostAsync(consumerAccessTokenSettings.DdsGetStructuredRecordUrl, content);
            string responseContent = await response.Content.ReadAsStringAsync();
            stopwatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Request failed with status code {response.StatusCode}. " +
                    $"Response: {responseContent}");
            }

            string text = responseContent;
            output.WriteLine($"Time taken to get structured record: {stopwatch.ElapsedMilliseconds} ms");
            output.WriteLine("");
            output.WriteLine("");
            string outputDirectory = Path.Combine(AppContext.BaseDirectory, "StructuredRecords");
            Directory.CreateDirectory(outputDirectory);
            string filePath = Path.Combine(outputDirectory, $"{nhsNumber}-DDS.txt");
            await File.WriteAllTextAsync(filePath, text);
            output.WriteLine($"Structured record written to: {filePath}");
        }
    }
}
