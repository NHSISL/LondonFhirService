// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Attrify.InvisibleApi.Models;
using LondonFhirService.Manage.Brokers.Https;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace LondonFhirService.Manage.Tests.Acceptance.Brokers
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        /// <summary>
        /// Stands in for every outbound call PatientService makes. The structured record endpoint
        /// talks to an authorisation server and to a provider, neither of which exists in a test
        /// run, so the transport is replaced rather than the service - which leaves the whole
        /// pipeline under test: model binding, authorisation, the controller's exception mapping
        /// and the service's own validation all still run for real.
        ///
        /// Owned by the factory rather than created per test because the factory is a collection
        /// fixture. Tests within a collection run one at a time, so each replaces it before use
        /// via ResetHttpBroker - a fresh mock rather than a cleared one, so no setup or recorded
        /// invocation can survive into the next test.
        /// </summary>
        public Mock<IHttpBroker> HttpBrokerMock { get; private set; } = new Mock<IHttpBroker>();

        public void ResetHttpBroker() =>
            HttpBrokerMock = new Mock<IHttpBroker>();

        static TestWebApplicationFactory()
        {
            // Configure configuration *before* the app’s builder is used
            Program.TestConfigurationOverrides = builder =>
            {
                var testProjectPath =
                    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

                // This runs inside Program.cs right after CreateBuilder(...)
                // This lets us override any configuration values for testing
                builder.Configuration
                    .AddJsonFile(
                        Path.Combine(testProjectPath, "appsettings.json"),
                        optional: true)
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        // Put your strong overrides here
                        //["AzureAd:TenantId"] = "TEST-TENANT",
                        //["AzureAd:Instance"] = "https://login.microsoftonline.com/",
                        //["AzureAd:Scopes"]   = "api://test/.default"
                    });
            };

            Program.ExcludeAppInsightsForTesting = true;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Make sure the app runs in a predictable test environment
            builder.UseEnvironment("Test");

            builder.ConfigureServices((context, services) =>
            {
                OverrideSecurityForTesting(services);
                OverrideHttpBrokerForTesting(services);
            });
        }

        private void OverrideHttpBrokerForTesting(IServiceCollection services)
        {
            // The host registers IHttpBroker as a typed HttpClient, so both that descriptor and
            // the one HttpClientFactory adds for the concrete type have to go - leaving either
            // behind lets a real socket be opened by whichever one resolves first.
            List<ServiceDescriptor> httpBrokerDescriptors = services
                .Where(descriptor => descriptor.ServiceType == typeof(IHttpBroker)
                    || descriptor.ServiceType == typeof(HttpBroker))
                .ToList();

            foreach (ServiceDescriptor httpBrokerDescriptor in httpBrokerDescriptors)
            {
                services.Remove(httpBrokerDescriptor);
            }

            // Transient and resolved through the property, not captured. A singleton would pin
            // whichever mock existed when the container first handed one out, and every later
            // test would be configuring an object nothing resolves.
            services.AddTransient<IHttpBroker>(serviceProvider => HttpBrokerMock.Object);
        }

        private static void OverrideSecurityForTesting(IServiceCollection services)
        {
            var invisibleApiKeyDescriptor = services
                .FirstOrDefault(d => d.ServiceType == typeof(InvisibleApiKey));

            InvisibleApiKey invisibleApiKey = null;

            if (invisibleApiKeyDescriptor != null)
            {
                using (var serviceProvider = services.BuildServiceProvider())
                {
                    invisibleApiKey = serviceProvider.GetService<InvisibleApiKey>();
                }
            }

            // Remove existing authentication and authorization
            var authenticationDescriptor = services
                .FirstOrDefault(d => d.ServiceType == typeof(IAuthenticationSchemeProvider));

            if (authenticationDescriptor != null)
            {
                services.Remove(authenticationDescriptor);
            }

            // Override authentication and authorization
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
            })
            .AddScheme<CustomAuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options =>
            {
                options.InvisibleApiKey = invisibleApiKey;
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("TestPolicy", policy => policy.RequireAssertion(_ => true));
            });
        }
    }
}
