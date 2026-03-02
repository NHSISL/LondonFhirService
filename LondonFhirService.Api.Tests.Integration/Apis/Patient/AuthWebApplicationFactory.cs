// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace LondonFhirService.Api.Tests.Integration.Apis.Patient
{
    public class AuthWebApplicationFactory : WebApplicationFactory<Program>
    {
        static AuthWebApplicationFactory()
        {
            // Configure configuration *before* the app’s builder is used
            Program.TestConfigurationOverrides = builder =>
            {
                var testProjectPath =
                    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

                builder.Configuration
                    .AddJsonFile(Path.Combine(testProjectPath, "appsettings.json"), optional: true)
                    .AddJsonFile(Path.Combine(testProjectPath, "appsettings.Integration.json"), optional: true)
                    .AddInMemoryCollection(new Dictionary<string, string?>
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
            builder.ConfigureServices((context, services) =>
            {
                //OverrideSecurityForTesting(services);
            });
        }
    }
}
