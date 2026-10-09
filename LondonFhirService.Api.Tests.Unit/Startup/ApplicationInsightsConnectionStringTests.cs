// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LondonFhirService.Api.Tests.Unit.Startup
{
    /// <summary>
    /// The connection string can arrive under either key: ApplicationInsights:ConnectionString
    /// (an ApplicationInsights__ConnectionString app setting, or a developer's own
    /// appsettings.Development.json) or the APPLICATIONINSIGHTS_CONNECTION_STRING the App Service
    /// sets, which is the only one any deployed environment sets today. appsettings.json carries a
    /// placeholder under the first, so it is only taken when it holds a real connection string.
    /// Telemetry and the startup failure report both use the one chosen here.
    /// </summary>
    public class ApplicationInsightsConnectionStringTests
    {
        private const string ConfiguredConnectionString =
            "InstrumentationKey=11111111-1111-1111-1111-111111111111;"
                + "IngestionEndpoint=https://configured.example.invalid/";

        private const string AppServiceConnectionString =
            "InstrumentationKey=22222222-2222-2222-2222-222222222222;"
                + "IngestionEndpoint=https://app-service.example.invalid/";

        private const string Placeholder = "override in appsettings.Development.json";

        [Fact]
        public void ShouldUseTheConfiguredConnectionStringIfItIsAConnectionString()
        {
            // given
            IConfiguration configuration = CreateConfiguration(
                configured: ConfiguredConnectionString,
                appService: AppServiceConnectionString);

            // when
            string actualConnectionString =
                Program.GetApplicationInsightsConnectionString(configuration);

            // then
            actualConnectionString.Should().Be(ConfiguredConnectionString);
        }

        /// <summary>
        /// Every deployed environment: the placeholder from appsettings.json is never null, so it
        /// must not stop the App Service's connection string being read.
        /// </summary>
        [Fact]
        public void ShouldUseTheAppServiceConnectionStringIfTheConfiguredOneIsAPlaceholder()
        {
            // given
            IConfiguration configuration = CreateConfiguration(
                configured: Placeholder,
                appService: AppServiceConnectionString);

            // when
            string actualConnectionString =
                Program.GetApplicationInsightsConnectionString(configuration);

            // then
            actualConnectionString.Should().Be(AppServiceConnectionString);
        }

        [Theory]
        [InlineData("InstrumentationKey=")]
        [InlineData("InstrumentationKey=not-a-guid")]
        [InlineData("IngestionEndpoint=https://configured.example.invalid/;NotInstrumentationKey=11111111-1111-1111-1111-111111111111")]
        public void ShouldUseTheAppServiceConnectionStringIfTheConfiguredOneIsMalformed(
            string malformedConnectionString)
        {
            // given
            IConfiguration configuration = CreateConfiguration(
                configured: malformedConnectionString,
                appService: AppServiceConnectionString);

            // when
            string actualConnectionString =
                Program.GetApplicationInsightsConnectionString(configuration);

            // then
            actualConnectionString.Should().Be(AppServiceConnectionString);
        }

        [Fact]
        public void ShouldUseTheAppServiceConnectionStringIfNoneIsConfigured()
        {
            // given
            IConfiguration configuration = CreateConfiguration(
                configured: null,
                appService: AppServiceConnectionString);

            // when
            string actualConnectionString =
                Program.GetApplicationInsightsConnectionString(configuration);

            // then
            actualConnectionString.Should().Be(AppServiceConnectionString);
        }

        [Fact]
        public void ShouldHaveNoConnectionStringIfNeitherKeyHoldsOne()
        {
            // given
            IConfiguration configuration = CreateConfiguration(
                configured: Placeholder,
                appService: null);

            // when
            string actualConnectionString =
                Program.GetApplicationInsightsConnectionString(configuration);

            // then
            actualConnectionString.Should().BeNull();
        }

        [Fact]
        public void ShouldRegisterTelemetryWithTheConnectionStringItChose()
        {
            // given
            WebApplicationBuilder builder = CreateBuilder(
                configured: Placeholder,
                appService: AppServiceConnectionString);

            // when
            Program.ConfigureApplicationInsightsTelemetry(builder);

            // then
            using ServiceProvider serviceProvider = builder.Services.BuildServiceProvider();

            serviceProvider
                .GetRequiredService<IOptions<ApplicationInsightsServiceOptions>>()
                .Value.ConnectionString
                .Should().Be(AppServiceConnectionString);
        }

        /// <summary>
        /// Left to itself the SDK rejects the placeholder and the host fails to start. Without a
        /// connection string there is nothing to send to, so telemetry is left out and the
        /// MetricTelemetryPublisher idles.
        /// </summary>
        [Fact]
        public void ShouldNotRegisterTelemetryIfNeitherKeyHoldsAConnectionString()
        {
            // given
            WebApplicationBuilder builder = CreateBuilder(
                configured: Placeholder,
                appService: null);

            // when
            Program.ConfigureApplicationInsightsTelemetry(builder);

            // then
            builder.Services
                .Any(descriptor => descriptor.ServiceType == typeof(TelemetryClient))
                .Should().BeFalse();
        }

        private static IConfiguration CreateConfiguration(string configured, string appService) =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(CreateSettings(configured, appService))
                .Build();

        /// <summary>
        /// Empty, so nothing the machine running the tests has set - an
        /// APPLICATIONINSIGHTS_CONNECTION_STRING of its own - reaches the configuration.
        /// </summary>
        private static WebApplicationBuilder CreateBuilder(string configured, string appService)
        {
            WebApplicationBuilder builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions());
            builder.Configuration.AddInMemoryCollection(CreateSettings(configured, appService));

            return builder;
        }

        private static Dictionary<string, string> CreateSettings(string configured, string appService) =>
            new Dictionary<string, string>
            {
                ["ApplicationInsights:ConnectionString"] = configured,
                ["APPLICATIONINSIGHTS_CONNECTION_STRING"] = appService
            };
    }
}
