// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Api.Tests.Integration.Apis.Patient;
using LondonFhirService.Api.Tests.Integration.Models;
using Microsoft.Extensions.Configuration;

namespace LondonFhirService.Api.Tests.Integration.Brokers.Authentications
{
    public partial class AuthApiBroker
    {
        private readonly AuthWebApplicationFactory webApplicationFactory;
        private readonly IConfiguration configuration;
        private readonly ConsumerAccessTokenSettings consumerAccessTokenSettings;
        private readonly HttpClient httpClient;
        private readonly HttpClient tokenHttp;
        private readonly SemaphoreSlim tokenGate = new(1, 1);
        private string accessToken = string.Empty;
        private DateTimeOffset tokenExpiry = DateTimeOffset.MinValue;

        internal AuthWebApplicationFactory WebApplicationFactory => webApplicationFactory;

        public AuthApiBroker()
        {
            webApplicationFactory = new AuthWebApplicationFactory();
            httpClient = webApplicationFactory.CreateClient();
            this.tokenHttp = new HttpClient();

            var testProjectPath =
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

            configuration = new ConfigurationBuilder()
                .SetBasePath(testProjectPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.Integration.json", optional: true, reloadOnChange: false)
                .Build();

            consumerAccessTokenSettings =
                configuration
                    .GetSection("ConsumerAccessTokenSettings")
                    .Get<ConsumerAccessTokenSettings>()
                        ?? throw new InvalidOperationException(
                            "ConsumerAccessTokenSettings configuration section is missing or invalid.");
        }

        private async ValueTask EnsureAccessTokenAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(this.accessToken)
                && DateTimeOffset.UtcNow < this.tokenExpiry)
            {
                return;
            }

            await this.tokenGate.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                if (string.IsNullOrEmpty(this.accessToken)
                    || DateTimeOffset.UtcNow >= this.tokenExpiry)
                {
                    await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
                    this.httpClient.DefaultRequestHeaders.Clear();

                    this.httpClient.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/fhir+json"));

                    this.httpClient.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    this.httpClient.DefaultRequestHeaders.Remove("Authorization");
                    this.httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {this.accessToken}");
                }
            }
            finally
            {
                this.tokenGate.Release();
            }
        }

        private async ValueTask GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            var requestBody = new
            {
                client_id = consumerAccessTokenSettings.ClientId,
                client_secret = consumerAccessTokenSettings.ClientSecret,
                scope = consumerAccessTokenSettings.Scope,
                grant_type = consumerAccessTokenSettings.GrantType
            };

            var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = consumerAccessTokenSettings.ClientId,
                ["client_secret"] = consumerAccessTokenSettings.ClientSecret,
                ["scope"] = consumerAccessTokenSettings.Scope,
                ["grant_type"] = consumerAccessTokenSettings.GrantType
            });

            var response = await this.tokenHttp
                .PostAsync(consumerAccessTokenSettings.AuthorisationUrl, formContent, cancellationToken)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var json = await response.Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            this.accessToken = root.GetProperty("access_token").GetString()
                ?? throw new InvalidOperationException("Access token is null");

            if (!root.GetProperty("expires_in").TryGetInt32(out int expiresIn))
            {
                throw new InvalidOperationException("Invalid expires_in value");
            }

            this.tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(expiresIn - 30);
        }
    }
}