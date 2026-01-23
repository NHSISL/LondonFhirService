// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Attrify.InvisibleApi.Models;
using LondonFhirService.Core.Brokers.Fhirs.STU3;
using LondonFhirService.Core.Clients.Audits;
using LondonFhirService.Core.Models.Foundations.Patients;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using FhirStu3Abstractions = LondonFhirService.Providers.FHIR.STU3.Abstractions;

namespace LondonFhirService.Api.Tests.Acceptance.Brokers
{
    // Non-generic – we always host Program
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
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Make sure the app runs in a predictable test environment
            builder.UseEnvironment("Test");

            builder.ConfigureServices((context, services) =>
            {
                OverrideSecurityForTesting(services);
                OverrideFhirProvidersForTesting(services);
                MockExternalClientsForTesting(services);
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

        private static void OverrideFhirProvidersForTesting(IServiceCollection services)
        {
            //var fhirR4AbstractionProviderDescriptor = services
            //    .FirstOrDefault(d => d.ServiceType == typeof(FhirR4Abstractions.IFhirAbstractionProvider));

            //if (fhirR4AbstractionProviderDescriptor != null)
            //{
            //    services.Remove(fhirR4AbstractionProviderDescriptor);
            //}

            var fhirStu3AbstractionProviderDescriptor = services
                .FirstOrDefault(d => d.ServiceType == typeof(FhirStu3Abstractions.IFhirAbstractionProvider));

            if (fhirStu3AbstractionProviderDescriptor != null)
            {
                services.Remove(fhirStu3AbstractionProviderDescriptor);
            }

            //var fhirR4BrokerDescriptor = services
            //    .FirstOrDefault(d => d.ServiceType == typeof(IR4FhirBroker));

            //if (fhirR4BrokerDescriptor != null)
            //{
            //    services.Remove(fhirR4BrokerDescriptor);
            //}

            var fhirStu3BrokerDescriptor = services
                .FirstOrDefault(d => d.ServiceType == typeof(IStu3FhirBroker));

            if (fhirStu3BrokerDescriptor != null)
            {
                services.Remove(fhirStu3BrokerDescriptor);
            }

            var patientServiceConfigDescriptor = services
                .FirstOrDefault(d => d.ServiceType == typeof(PatientServiceConfig));

            if (patientServiceConfigDescriptor != null)
            {
                services.Remove(patientServiceConfigDescriptor);
            }

            //var testR4Providers = new List<FhirR4Abstractions.IFhirProvider>
            //{
            //    TestR4FhirProviderFactory.CreateTestProvider("DDS"),
            //    TestR4FhirProviderFactory.CreateTestProvider("LDS")
            //};

            //services.AddSingleton<FhirR4Abstractions.IFhirAbstractionProvider>(
            //    new FhirR4Abstractions.FhirAbstractionProvider(testR4Providers));

            //services.AddTransient<IR4FhirBroker, R4FhirBroker>();

            var testStu3Providers = new List<FhirStu3Abstractions.IFhirProvider>
            {
                TestStu3FhirProviderFactory.CreateTestProvider("DDS"),
                TestStu3FhirProviderFactory.CreateTestProvider("LDS")
            };

            services.AddSingleton<FhirStu3Abstractions.IFhirAbstractionProvider>(
                new FhirStu3Abstractions.FhirAbstractionProvider(testStu3Providers));

            services.AddTransient<IStu3FhirBroker, Stu3FhirBroker>();

            services.AddSingleton(new PatientServiceConfig
            {
                MaxProviderWaitTimeMilliseconds = 30000
            });
        }

        private static void MockExternalClientsForTesting(IServiceCollection services)
        {
            var auditClientDescriptor = services
               .FirstOrDefault(d => d.ServiceType == typeof(IAuditClient));

            if (auditClientDescriptor != null)
            {
                services.Remove(auditClientDescriptor);
            }

            var mockAuditClient = new Mock<IAuditClient>();
            services.AddTransient<IAuditClient>(_ => mockAuditClient.Object);
        }
    }
}
