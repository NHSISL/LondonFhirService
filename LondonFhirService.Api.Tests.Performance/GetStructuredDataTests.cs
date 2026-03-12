// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Api.Tests.Performance.Models;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace LondonFhirService.Api.Tests.Performance
{
    public partial class GetStructuredDataTests
    {
        private readonly ITestOutputHelper output;
        private readonly IConfiguration configuration;
        private readonly ConsumerAccessTokenSettings consumerAccessTokenSettings;

        public GetStructuredDataTests(ITestOutputHelper output)
        {
            var testProjectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

            this.configuration = new ConfigurationBuilder()
                .SetBasePath(testProjectPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.Integration.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            consumerAccessTokenSettings =
                configuration
                    .GetSection("ConsumerAccessTokenSettings")
                    .Get<ConsumerAccessTokenSettings>()
                        ?? throw new InvalidOperationException(
                            "ConsumerAccessTokenSettings configuration section is missing or invalid.");

            this.output = output;
        }

        private async ValueTask<string> GetLfsAccessToken()
        {
            var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = consumerAccessTokenSettings.LfsClientId,
                ["client_secret"] = consumerAccessTokenSettings.LfsClientSecret,
                ["scope"] = consumerAccessTokenSettings.LfsScope,
                ["grant_type"] = consumerAccessTokenSettings.LfsGrantType
            });

            var tokenClient = new HttpClient();

            var response = await tokenClient
                .PostAsync(consumerAccessTokenSettings.LfsAuthorisationUrl, formContent)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var json = await response.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            string accessToken = root.GetProperty("access_token").GetString()
                ?? throw new InvalidOperationException("Access token is null");

            return accessToken;
        }

        private async ValueTask<string> GetDdsAccessToken()
        {
            var requestBody = new
            {
                clientId = consumerAccessTokenSettings.DdsClientId,
                clientSecret = consumerAccessTokenSettings.DdsClientSecret,
                grantType = consumerAccessTokenSettings.DdsGrantType
            };

            string jsonContent = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            var tokenClient = new HttpClient();

            var response = await tokenClient
                .PostAsync(consumerAccessTokenSettings.DdsAuthorisationUrl, content)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var json = await response.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            string accessToken = root.GetProperty("access_token").GetString()
                ?? throw new InvalidOperationException("Access token is null");

            return accessToken;
        }

        private static Parameters CreateGetStructuredRecordParameters(
            string nhsNumber,
            string dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null)
        {
            var parameters = new Parameters();

            parameters.Add(
                "patientNHSNumber",
                new Identifier
                {
                    System = "https://fhir.hl7.org.uk/Id/nhs-number",
                    Value = nhsNumber
                });

            if (!string.IsNullOrWhiteSpace(dateOfBirth))
            {
                parameters.Add(
                    "patientDOB",
                    new Identifier
                    {
                        System = "https://fhir.hl7.org.uk/Id/dob",
                        Value = dateOfBirth
                    });
            }

            if (demographicsOnly.HasValue)
            {
                parameters.Parameter.Add(new Parameters.ParameterComponent
                {
                    Name = "demographicsOnly",
                    Part =
                    {
                        new Parameters.ParameterComponent
                        {
                            Name = "includeDemographicsOnly",
                            Value = new FhirBoolean(demographicsOnly.Value)
                        }
                    }
                });
            }

            if (includeInactivePatients.HasValue)
            {
                parameters.Parameter.Add(new Parameters.ParameterComponent
                {
                    Name = "includeInactivePatients",
                    Part =
                    {
                        new Parameters.ParameterComponent
                        {
                            Name = "includeInactivePatients",
                            Value = new FhirBoolean(includeInactivePatients.Value)
                        }
                    }
                });
            }

            return parameters;
        }
    }
}

