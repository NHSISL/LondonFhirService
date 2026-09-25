// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Patients;
using LondonFhirService.Core.Models.Orchestrations.Accesses;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Brokers.DdsHttp;
using LondonFhirService.Providers.FHIR.STU3.LondonDataService.Models.Brokers.LdsHttp;

namespace LondonFhirService.Api.Tests.Unit.Startup
{
    /// <summary>
    /// The provider settings decide whether the host starts at all, because both STU3 providers
    /// are built unconditionally. These pin which values stop it - and that every such problem is
    /// reported in one message - and which are only warned about, because an environment that
    /// does not use a provider yet legitimately ships placeholders for it.
    /// </summary>
    public class ProviderConfigurationValidationTests
    {
        private const string Placeholder =
            "override_this_in_your_appsettings.Development.json_file_or_environment_variables";

        private static DdsConfigurations CreateValidDdsConfigurations() => new()
        {
            ClientId = "fhir-api",
            ClientSecret = "secret",
            AuthorisationUrl = "https://auth.example.invalid/token",
            BaseUrl = "https://dds.example.invalid/",
            GetStructuredRecordRelativeUrl = "patient/$getstructuredrecord",
            TimeoutSeconds = 120
        };

        private static LdsConfigurations CreateValidLdsConfigurations() => new()
        {
            Scope = "api://lds/.default",
            ManagedIdentityClientId = string.Empty,
            BaseUrl = "https://lds.example.invalid/",
            GetStructuredRecordRelativeUrl = "api/patient/$getstructuredrecord",
            TimeoutSeconds = 120
        };

        [Fact]
        public void ShouldAcceptUsableProviderConfigurations()
        {
            // given . when
            Action validate = () => Program.ValidateProviderConfigurations(
                new PatientServiceConfig(),
                CreateValidDdsConfigurations(),
                CreateValidLdsConfigurations(),
                new AccessConfigurations());

            // then
            validate.Should().NotThrow();
        }

        [Fact]
        public void ShouldReportEveryMissingSectionInOneMessage()
        {
            // given . when
            Action validate = () => Program.ValidateProviderConfigurations(
                patientServiceConfig: null,
                ddsConfig: null,
                ldsConfig: null,
                accessConfig: null);

            // then
            string message = validate.Should().Throw<InvalidOperationException>().Which.Message;
            message.Should().Contain("PatientServiceConfig is missing.");
            message.Should().Contain("DdsConfigurations is missing.");
            message.Should().Contain("LdsConfigurations is missing.");
            message.Should().Contain("AccessConfigurations is missing.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(2147484)]
        public void ShouldRejectATimeoutHttpClientCannotTake(int invalidTimeoutSeconds)
        {
            // given
            DdsConfigurations ddsConfigurations = CreateValidDdsConfigurations();
            ddsConfigurations.TimeoutSeconds = invalidTimeoutSeconds;
            LdsConfigurations ldsConfigurations = CreateValidLdsConfigurations();
            ldsConfigurations.TimeoutSeconds = invalidTimeoutSeconds;

            // when
            Action validate = () => Program.ValidateProviderConfigurations(
                new PatientServiceConfig(),
                ddsConfigurations,
                ldsConfigurations,
                new AccessConfigurations());

            // then
            string message = validate.Should().Throw<InvalidOperationException>().Which.Message;
            message.Should().Contain("DdsConfigurations:TimeoutSeconds must be between 1 and 2147483");
            message.Should().Contain("LdsConfigurations:TimeoutSeconds must be between 1 and 2147483");
        }

        [Theory]
        [InlineData("")]
        [InlineData(Placeholder)]
        [InlineData("localhost:7284/fhir")]
        [InlineData("ftp://dds.example.invalid/")]
        [InlineData("/relative/only")]
        public void ShouldRejectAUrlAProviderCannotDial(string invalidUrl)
        {
            // given
            DdsConfigurations ddsConfigurations = CreateValidDdsConfigurations();
            ddsConfigurations.BaseUrl = invalidUrl;
            ddsConfigurations.AuthorisationUrl = invalidUrl;
            LdsConfigurations ldsConfigurations = CreateValidLdsConfigurations();
            ldsConfigurations.BaseUrl = invalidUrl;

            // when
            Action validate = () => Program.ValidateProviderConfigurations(
                new PatientServiceConfig(),
                ddsConfigurations,
                ldsConfigurations,
                new AccessConfigurations());

            // then
            string message = validate.Should().Throw<InvalidOperationException>().Which.Message;
            message.Should().Contain("DdsConfigurations:BaseUrl");
            message.Should().Contain("DdsConfigurations:AuthorisationUrl");
            message.Should().Contain("LdsConfigurations:BaseUrl");
        }

        [Fact]
        public void ShouldNotFailStartupOverValuesOnlyNeededWhenAProviderIsDialled()
        {
            // given
            DdsConfigurations ddsConfigurations = CreateValidDdsConfigurations();
            ddsConfigurations.ClientId = Placeholder;
            ddsConfigurations.ClientSecret = string.Empty;
            LdsConfigurations ldsConfigurations = CreateValidLdsConfigurations();
            ldsConfigurations.Scope = Placeholder;

            // when
            Action validate = () => Program.ValidateProviderConfigurations(
                new PatientServiceConfig(),
                ddsConfigurations,
                ldsConfigurations,
                new AccessConfigurations());

            // then
            validate.Should().NotThrow();
        }

        [Fact]
        public void ShouldWarnAboutEveryUnsetValueAProviderNeedsToBeDialled()
        {
            // given
            DdsConfigurations ddsConfigurations = CreateValidDdsConfigurations();
            ddsConfigurations.ClientId = Placeholder;
            ddsConfigurations.ClientSecret = " ";
            ddsConfigurations.GetStructuredRecordRelativeUrl = string.Empty;
            LdsConfigurations ldsConfigurations = CreateValidLdsConfigurations();
            ldsConfigurations.Scope = Placeholder;
            ldsConfigurations.GetStructuredRecordRelativeUrl = Placeholder;
            ldsConfigurations.ManagedIdentityClientId = Placeholder;

            // when
            List<string> warnings =
                Program.FindProviderConfigurationWarnings(ddsConfigurations, ldsConfigurations);

            // then
            warnings.Should().HaveCount(6);
            warnings.Should().Contain(warning => warning.StartsWith("DdsConfigurations:ClientId "));
            warnings.Should().Contain(warning => warning.StartsWith("DdsConfigurations:ClientSecret "));
            warnings.Should().Contain(warning => warning.StartsWith("DdsConfigurations:GetStructuredRecordRelativeUrl "));
            warnings.Should().Contain(warning => warning.StartsWith("LdsConfigurations:Scope "));
            warnings.Should().Contain(warning => warning.StartsWith("LdsConfigurations:GetStructuredRecordRelativeUrl "));
            warnings.Should().Contain(warning => warning.StartsWith("LdsConfigurations:ManagedIdentityClientId "));
        }

        /// <summary>
        /// A blank managed identity client id means the system assigned identity, so only the
        /// shipped placeholder is a mistake worth a warning.
        /// </summary>
        [Fact]
        public void ShouldNotWarnAboutUsableValuesOrABlankManagedIdentityClientId()
        {
            // given
            LdsConfigurations ldsConfigurations = CreateValidLdsConfigurations();
            ldsConfigurations.ManagedIdentityClientId = string.Empty;

            // when
            List<string> warnings = Program.FindProviderConfigurationWarnings(
                CreateValidDdsConfigurations(),
                ldsConfigurations);

            // then
            warnings.Should().BeEmpty();
        }
    }
}
