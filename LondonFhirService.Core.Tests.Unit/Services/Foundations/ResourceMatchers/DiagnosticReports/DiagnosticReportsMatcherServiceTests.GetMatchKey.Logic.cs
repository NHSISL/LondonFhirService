// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.DiagnosticReports
{
    public partial class DiagnosticReportMatcherServiceTests
    {
        [Fact]
        public async Task ShouldGetMatchKeyWhenDiagnosticReportHasDdsIdentifierAsync()
        {
            // given
            string inputDdsIdentifierValue = GetRandomDdsIdentifierValue();
            string randomId = GetRandomString();

            JsonElement diagnosticReportResource = CreateDiagnosticReportResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: randomId);

            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
            string expectedMatchKey = inputDdsIdentifierValue;

            // when
            string actualMatchKey =
                await this.diagnosticReportMatcherService.GetMatchKeyAsync(diagnosticReportResource, resourceIndex);

            // then
            actualMatchKey.Should().Be(expectedMatchKey);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullOnGetMatchKeyWhenDiagnosticReportHasNoIdentifierPropertyAsync()
        {
            // given
            string randomId = GetRandomString();

            JsonElement diagnosticReportResource =
                CreateDiagnosticReportResourceWithoutIdentifierProperty(id: randomId);

            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.diagnosticReportMatcherService.GetMatchKeyAsync(diagnosticReportResource, resourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullOnGetMatchKeyWhenDiagnosticReportHasNonDdsIdentifierAsync()
        {
            // given
            string randomId = GetRandomString();
            JsonElement diagnosticReportResource = CreateNonDdsDiagnosticReportResource(id: randomId);
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.diagnosticReportMatcherService.GetMatchKeyAsync(diagnosticReportResource, resourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
