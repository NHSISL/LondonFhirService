// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Attrify.InvisibleApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LondonFhirService.Manage.Tests.Acceptance.Brokers
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
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
            });
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
