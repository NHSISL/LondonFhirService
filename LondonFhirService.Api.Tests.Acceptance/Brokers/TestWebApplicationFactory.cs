// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Attrify.InvisibleApi.Models;
using LondonFhirService.Core.Brokers.Fhirs.R4;
using LondonFhirService.Core.Clients.Audits;
using LondonFhirService.Core.Models.Foundations.Patients;
using LondonFhirService.Providers.FHIR.R4.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace LondonFhirService.Api.Tests.Acceptance.Brokers
{
    public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                var testProjectPath =
                    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

                config
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddJsonFile("appsettings.Development.json", optional: true)
                    .AddJsonFile("appsettings.Acceptance.json", optional: true)
                    .AddJsonFile(Path.Combine(testProjectPath, "appsettings.json"), optional: true)
                    .AddJsonFile(Path.Combine(testProjectPath, "appsettings.Development.json"), optional: true)
                    .AddEnvironmentVariables();
            });

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
            var fhirAbstractionProviderDescriptor = services
                .FirstOrDefault(d => d.ServiceType == typeof(IFhirAbstractionProvider));

            if (fhirAbstractionProviderDescriptor != null)
            {
                services.Remove(fhirAbstractionProviderDescriptor);
            }

            var fhirBrokerDescriptor = services
                .FirstOrDefault(d => d.ServiceType == typeof(IR4FhirBroker));

            if (fhirBrokerDescriptor != null)
            {
                services.Remove(fhirBrokerDescriptor);
            }

            var patientServiceConfigDescriptor = services
                .FirstOrDefault(d => d.ServiceType == typeof(PatientServiceConfig));

            if (patientServiceConfigDescriptor != null)
            {
                services.Remove(patientServiceConfigDescriptor);
            }

            var testProviders = new List<IFhirProvider>
            {
                TestFhirProviderFactory.CreateTestProvider("DDS"),
                TestFhirProviderFactory.CreateTestProvider("LDS")
            };

            services.AddSingleton<IFhirAbstractionProvider>(
                new FhirAbstractionProvider(testProviders));

            services.AddTransient<IR4FhirBroker, R4FhirBroker>();

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
