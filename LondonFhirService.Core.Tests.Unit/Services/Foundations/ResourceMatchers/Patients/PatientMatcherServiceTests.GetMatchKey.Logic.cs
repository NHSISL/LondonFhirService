// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Patients
{
    public partial class PatientMatcherServiceTests
    {
        [Fact]
        public async Task ShouldReturnNhsNumberAsMatchKeyForPatientWithNhsNumberAsync()
        {
            // given
            string expectedNhsNumber = "9000000009";
            JsonElement patientResource = CreatePatientWithNhsNumber(expectedNhsNumber);
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.patientMatcherService.GetMatchKeyAsync(
                    patientResource,
                    resourceIndex);

            // then
            actualMatchKey.Should().Be(expectedNhsNumber);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullMatchKeyForPatientWithoutIdentifiersAsync()
        {
            // given
            JsonElement patientResource = CreatePatientWithoutIdentifier();
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.patientMatcherService.GetMatchKeyAsync(
                    patientResource,
                    resourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullMatchKeyForPatientWithNonNhsNumberIdentifierAsync()
        {
            // given
            JsonElement patientResource = CreatePatientWithNonNhsIdentifier();
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.patientMatcherService.GetMatchKeyAsync(
                    patientResource,
                    resourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
