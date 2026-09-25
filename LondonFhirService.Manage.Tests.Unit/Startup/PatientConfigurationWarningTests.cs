// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using FluentAssertions;
using LondonFhirService.Manage.Models.Foundations.Patients;

namespace LondonFhirService.Manage.Tests.Unit.Startup
{
    /// <summary>
    /// PatientConfiguration may be absent - the host starts without it and the structured record
    /// page answers with a 400 naming what is missing - so nothing here stops startup. These pin
    /// what is warned about, so a deployment with the page unconfigured says so in its first lines
    /// of log rather than to the first operator who tries it.
    /// </summary>
    public class PatientConfigurationWarningTests
    {
        private const string Placeholder =
            "override_this_in_your_appsettings.Development.json_file_or_environment_variables";

        private static PatientConfiguration CreateUsablePatientConfiguration() => new()
        {
            AuthUrl = "https://login.example.invalid/oauth2/v2.0/token",
            ClientId = string.Empty,
            ClientSecret = string.Empty,
            Scope = "api://lfs/.default",
            GrantType = "client_credentials",
            GetStructuredRecordUrl = "https://api.example.invalid/api/STU3/Patient/$getstructuredrecord"
        };

        /// <summary>
        /// Blank credentials are the shipped state: they are the fallback for what an operator
        /// types into the page, so they are not a mistake.
        /// </summary>
        [Fact]
        public void ShouldNotWarnAboutUsableSettingsOrBlankFallbackCredentials()
        {
            // given . when
            List<string> warnings =
                Program.FindPatientConfigurationWarnings(CreateUsablePatientConfiguration());

            // then
            warnings.Should().BeEmpty();
        }

        [Fact]
        public void ShouldWarnAboutEverySettingThePageCannotWorkWithout()
        {
            // given
            var patientConfiguration = new PatientConfiguration
            {
                AuthUrl = "login.example.invalid/token",
                ClientId = Placeholder,
                ClientSecret = Placeholder,
                Scope = Placeholder,
                GrantType = " ",
                GetStructuredRecordUrl = Placeholder
            };

            // when
            List<string> warnings = Program.FindPatientConfigurationWarnings(patientConfiguration);

            // then
            warnings.Should().HaveCount(6);
            warnings.Should().Contain(warning => warning.StartsWith("PatientConfiguration:AuthUrl "));
            warnings.Should().Contain(warning => warning.StartsWith("PatientConfiguration:GetStructuredRecordUrl "));
            warnings.Should().Contain(warning => warning.StartsWith("PatientConfiguration:Scope "));
            warnings.Should().Contain(warning => warning.StartsWith("PatientConfiguration:GrantType "));
            warnings.Should().Contain(warning => warning.StartsWith("PatientConfiguration:ClientId "));
            warnings.Should().Contain(warning => warning.StartsWith("PatientConfiguration:ClientSecret "));
        }

        /// <summary>
        /// An absent section binds to an all-null instance. That must be reported, not thrown on.
        /// </summary>
        [Fact]
        public void ShouldWarnRatherThanThrowForAnAbsentSection()
        {
            // given . when
            List<string> warnings = Program.FindPatientConfigurationWarnings(new PatientConfiguration());

            // then
            warnings.Should().HaveCount(4);
        }
    }
}
