// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LondonFhirService.Manage
{
    public partial class Program
    {
        internal static Action<WebApplicationBuilder>? TestConfigurationOverrides { get; set; } = null;
        internal static bool ExcludeAppInsightsForTesting { get; set; } = false;

        internal static void ConfigurationOverridesForTesting(WebApplicationBuilder builder)
        {
            TestConfigurationOverrides?.Invoke(builder);
        }

        internal static void ConfigureApplicationInsightsTelemetry(WebApplicationBuilder builder)
        {
            if (ExcludeAppInsightsForTesting == false)
            {
                builder.Services.AddApplicationInsightsTelemetry();
            }
        }

        private static bool IsApplicationInsightsAdded(IServiceCollection services)
        {
            return services.Any((ServiceDescriptor service) => service.ServiceType == typeof(TelemetryClient));
        }

        private static IServiceCollection AddApplicationInsightsTelemetry(this IServiceCollection services)
        {
            try
            {
                if (!IsApplicationInsightsAdded(services))
                {
                    services.AddOptions<ApplicationInsightsServiceOptions>().Configure(delegate (ApplicationInsightsServiceOptions options, IConfiguration config)
                    {
                        AddTelemetryConfiguration(config, options);
                    });
                    services.AddOpenTelemetry().WithApplicationInsights().UseApplicationInsightsTelemetry();
                    AddTelemetryConfigAndClient(services, "shc" + VersionUtils.GetVersion(typeof(ApplicationInsightsExtensions)));
                    services.AddSingleton<IJavaScriptSnippet, JavaScriptSnippet>();
                    services.AddSingleton<JavaScriptSnippet>();
                }

                return services;
            }
            catch (Exception exception)
            {
                AspNetCoreEventSource.Instance.FailedToAddTelemetry(exception.ToInvariantString());
                return services;
            }
        }
    }
}
