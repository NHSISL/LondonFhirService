// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LondonFhirService.Core.Models.Orchestrations.Accesses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LondonFhirService.Api.Tests.Unit.Startup
{
    /// <summary>
    /// The access check's switch is read from ConsumerAccessConfiguration, beside the endpoint it
    /// turns on. These register the providers from real configuration rather than handing the
    /// validation a model, so they would fail if the binding went back to reading a section of its
    /// own - the old AccessConfigurations section is given the opposite value to prove which one
    /// was read.
    /// </summary>
    public class AccessConfigurationRegistrationTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldRegisterCheckAccessPermissionsFromConsumerAccessConfiguration(
            bool consumerAccessCheckAccessPermissions)
        {
            // given
            Dictionary<string, string> settings = CreateProviderSettings();

            settings["ConsumerAccessConfiguration:checkAccessPermissions"] =
                consumerAccessCheckAccessPermissions.ToString();

            settings["AccessConfigurations:checkAccessPermissions"] =
                (consumerAccessCheckAccessPermissions is false).ToString();

            // when
            AccessConfigurations actualAccessConfigurations = RegisterAccessConfigurations(settings);

            // then
            actualAccessConfigurations.CheckAccessPermissions
                .Should().Be(consumerAccessCheckAccessPermissions);
        }

        /// <summary>
        /// When no configuration source sets the key, the model's own default checks access. The
        /// shipped appsettings.json does set it - to false - so in the running API this default
        /// applies only if that base value is removed; see the next test for what actually runs.
        /// </summary>
        [Fact]
        public void ShouldCheckAccessPermissionsIfConsumerAccessConfigurationOmitsTheKey()
        {
            // given
            Dictionary<string, string> settings = CreateProviderSettings();
            settings["AccessConfigurations:checkAccessPermissions"] = "false";

            // when
            AccessConfigurations actualAccessConfigurations = RegisterAccessConfigurations(settings);

            // then
            actualAccessConfigurations.CheckAccessPermissions.Should().BeTrue();
        }

        /// <summary>
        /// What an environment that has not migrated gets. appsettings.json is loaded before the
        /// environment variables and sets checkAccessPermissions to false, so an environment that
        /// still sets only the old AccessConfigurations__checkAccessPermissions=true runs with the
        /// access check OFF - the old key is ignored and the base value wins. Deployments must move
        /// the setting to ConsumerAccessConfiguration__checkAccessPermissions.
        /// </summary>
        [Fact]
        public void ShouldNotCheckAccessPermissionsIfAnEnvironmentStillSetsOnlyTheOldKey()
        {
            // given
            Dictionary<string, string> baseSettings = CreateProviderSettings();
            baseSettings["ConsumerAccessConfiguration:checkAccessPermissions"] = "false";

            var environmentSettings = new Dictionary<string, string>
            {
                ["AccessConfigurations:checkAccessPermissions"] = "true"
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(baseSettings)
                .AddInMemoryCollection(environmentSettings)
                .Build();

            var services = new ServiceCollection();

            // when
            Program.AddProviders(services, configuration);

            // then
            services
                .Single(descriptor => descriptor.ServiceType == typeof(AccessConfigurations))
                .ImplementationInstance
                .Should().BeOfType<AccessConfigurations>()
                .Which.CheckAccessPermissions.Should().BeFalse();
        }

        /// <summary>
        /// An old AccessConfigurations section does not stand in for a missing
        /// ConsumerAccessConfiguration: startup stops and names the section it needs.
        /// </summary>
        [Fact]
        public void ShouldFailStartupIfConsumerAccessConfigurationIsMissing()
        {
            // given
            Dictionary<string, string> settings = CreateProviderSettings();

            foreach (string key in settings.Keys
                .Where(key => key.StartsWith("ConsumerAccessConfiguration:"))
                .ToList())
            {
                settings.Remove(key);
            }

            settings["AccessConfigurations:checkAccessPermissions"] = "true";

            // when
            Action register = () => RegisterAccessConfigurations(settings);

            // then
            register.Should().Throw<InvalidOperationException>()
                .Which.Message.Should().Contain("ConsumerAccessConfiguration is missing.");
        }

        private static AccessConfigurations RegisterAccessConfigurations(
            Dictionary<string, string> settings)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            var services = new ServiceCollection();
            Program.AddProviders(services, configuration);

            return services
                .Single(descriptor => descriptor.ServiceType == typeof(AccessConfigurations))
                .ImplementationInstance
                .Should().BeOfType<AccessConfigurations>().Subject;
        }

        /// <summary>
        /// Everything else AddProviders needs to register, valid enough that the access settings
        /// are the only thing under test. The consumer section carries its url and scope but not
        /// the switch, which each test sets or leaves out.
        /// </summary>
        private static Dictionary<string, string> CreateProviderSettings() =>
            new Dictionary<string, string>
            {
                ["PatientServiceConfig:MaxProviderWaitTimeMilliseconds"] = "120000",
                ["DdsConfigurations:clientId"] = "fhir-api",
                ["DdsConfigurations:clientSecret"] = "secret",
                ["DdsConfigurations:authorisationUrl"] = "https://auth.example.invalid/token",
                ["DdsConfigurations:baseUrl"] = "https://dds.example.invalid/",
                ["DdsConfigurations:getStructuredRecordRelativeUrl"] = "patient/$getstructuredrecord",
                ["DdsConfigurations:timeoutSeconds"] = "120",
                ["LdsConfigurations:scope"] = "api://lds/.default",
                ["LdsConfigurations:baseUrl"] = "https://lds.example.invalid/",
                ["LdsConfigurations:getStructuredRecordRelativeUrl"] = "api/patient/$getstructuredrecord",
                ["LdsConfigurations:timeoutSeconds"] = "120",
                ["ConsumerAccessConfiguration:url"] = "https://consumer-access.example.invalid/",
                ["ConsumerAccessConfiguration:scope"] = "api://consumer-access/.default",
                ["FakeCaptchaProviderMode"] = "true"
            };
    }
}
