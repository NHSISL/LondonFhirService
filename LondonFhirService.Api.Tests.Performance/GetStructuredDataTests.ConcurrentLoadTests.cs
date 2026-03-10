// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Api.Tests.Performance
{
    public partial class GetStructuredDataTests
    {
        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(50)]
        [InlineData(75)]
        [InlineData(100)]
        public async Task ShouldPerformConcurrentLoadTestAgainstLondonFhirServiceAsync(int requests)
        {
            // given
            string accessToken = await GetLfsAccessToken();
            int numberOfRequests = requests;

            List<Task<(bool isSuccess, TimeSpan elapsedTime, string? error)>> tasks = new();

            // when
            for (int index = 0; index < numberOfRequests; index++)
            {
                tasks.Add(SendLondonFhirServiceRequestAsync(accessToken));
            }

            var results = await Task.WhenAll(tasks);

            // then
            AssertLoadTestResults(
                systemName: "London FHIR Service",
                numberOfRequests: numberOfRequests,
                results: results);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(50)]
        [InlineData(75)]
        [InlineData(100)]
        public async Task ShouldPerformConcurrentLoadTestAgainstDdsAsync(int requests)
        {
            // given
            string accessToken = await GetDdsAccessToken();
            int numberOfRequests = requests;

            List<Task<(bool isSuccess, TimeSpan elapsedTime, string? error)>> tasks = new();

            // when
            for (int index = 0; index < numberOfRequests; index++)
            {
                tasks.Add(SendDdsRequestAsync(accessToken));
            }

            var results = await Task.WhenAll(tasks);

            // then
            AssertLoadTestResults(
                systemName: "DDS",
                numberOfRequests: numberOfRequests,
                results: results);
        }

        private async Task<(bool isSuccess, TimeSpan elapsedTime, string? error)> SendLondonFhirServiceRequestAsync(
            string accessToken)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                string inputNhsNumber = consumerAccessTokenSettings.NhsNumber;
                string? inputDateOfBirth = consumerAccessTokenSettings.DateOfBirth;
                bool? inputDemographicsOnly = consumerAccessTokenSettings.DemographicsOnly;
                bool? inputIncludeInactivePatients = consumerAccessTokenSettings.IncludeInactivePatients;

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

                HttpClient httpClient = new HttpClient();

                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    consumerAccessTokenSettings.LfsGetStructuredRecordUrl)
                {
                    Content = content
                };

                request.Headers.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/fhir+json"));

                request.Headers.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage response = await httpClient.SendAsync(request);

                string responseContent = await response.Content.ReadAsStringAsync();

                stopwatch.Stop();

                return response.IsSuccessStatusCode
                    ? (true, stopwatch.Elapsed, null)
                    : (false, stopwatch.Elapsed,
                        $"Status code: {response.StatusCode}. Response: {responseContent}");
            }
            catch (Exception exception)
            {
                stopwatch.Stop();
                return (false, stopwatch.Elapsed, exception.Message);
            }
        }

        private async Task<(bool isSuccess, TimeSpan elapsedTime, string? error)> SendDdsRequestAsync(
            string accessToken)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                string inputNhsNumber = consumerAccessTokenSettings.NhsNumber;
                string? inputDateOfBirth = consumerAccessTokenSettings.DateOfBirth;
                bool? inputDemographicsOnly = consumerAccessTokenSettings.DemographicsOnly;
                bool? inputIncludeInactivePatients = consumerAccessTokenSettings.IncludeInactivePatients;

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

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Clear();

                httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                httpClient.DefaultRequestHeaders.Add("Authorization", accessToken);

                HttpResponseMessage response =
                    await httpClient.PostAsync(
                        consumerAccessTokenSettings.DdsGetStructuredRecordUrl,
                        content);

                string responseContent = await response.Content.ReadAsStringAsync();

                stopwatch.Stop();

                return response.IsSuccessStatusCode
                    ? (true, stopwatch.Elapsed, null)
                    : (false, stopwatch.Elapsed,
                        $"Status code: {response.StatusCode}. Response: {responseContent}");
            }
            catch (Exception exception)
            {
                stopwatch.Stop();
                return (false, stopwatch.Elapsed, exception.Message);
            }
        }

        private void AssertLoadTestResults(
            string systemName,
            int numberOfRequests,
            (bool isSuccess, TimeSpan elapsedTime, string? error)[] results)
        {
            int successCount = results.Count(result => result.isSuccess);
            int failureCount = results.Count(result => !result.isSuccess);

            List<TimeSpan> elapsedTimes = results
                .Select(result => result.elapsedTime)
                .ToList();

            TimeSpan minTime = elapsedTimes.Min();
            TimeSpan maxTime = elapsedTimes.Max();
            TimeSpan averageTime =
                TimeSpan.FromMilliseconds(elapsedTimes.Average(time => time.TotalMilliseconds));

            double requestsPerSecond =
                maxTime.TotalSeconds > 0
                    ? numberOfRequests / maxTime.TotalSeconds
                    : 0;

            output.WriteLine($"Load Test Results for {systemName} - {numberOfRequests} concurrent requests:");
            output.WriteLine($"Successful Requests: {successCount}");
            output.WriteLine($"Failed Requests: {failureCount}");
            output.WriteLine($"Minimum Time: {minTime}");
            output.WriteLine($"Maximum Time: {maxTime}");
            output.WriteLine($"Average Time: {averageTime}");
            output.WriteLine($"Requests Per Second: {requestsPerSecond:F2}");

            foreach (string error in results
                .Where(result => !result.isSuccess && !string.IsNullOrWhiteSpace(result.error))
                .Select(result => result.error!)
                .Take(10))
            {
                output.WriteLine($"Failure: {error}");
            }

            successCount.Should().BeGreaterThan(
                expected: (int)(numberOfRequests * 0.9),
                because: $"at least 90% of {systemName} requests should succeed");
        }
    }
}
