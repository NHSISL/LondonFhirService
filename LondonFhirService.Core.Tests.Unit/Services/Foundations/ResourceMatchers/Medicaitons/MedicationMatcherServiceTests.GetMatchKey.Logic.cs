// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Medications
{
    public partial class MedicationMatcherServiceTests
    {
        [Fact]
        public async Task ShouldReturnSnomedCodeAsMatchKeyAsync()
        {
            // given
            string randomSnomedCode = GetRandomSnomedCode();
            JsonElement inputResource = CreateMedicationResource(randomSnomedCode, onsetDateTime: "2024-01-01");
            Dictionary<string, JsonElement> inputResourceIndex = CreateResourceIndex();
            string expectedMatchKey = randomSnomedCode;

            // when
            string actualMatchKey = await this.medicationMatcherService.GetMatchKeyAsync(
                inputResource,
                inputResourceIndex);

            // then
            actualMatchKey.Should().Be(expectedMatchKey);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullMatchKeyWhenResourceHasNoCodePropertyAsync()
        {
            // given
            JsonElement inputResource = CreateMedicationResourceWithNoCodeProperty();
            Dictionary<string, JsonElement> inputResourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey = await this.medicationMatcherService.GetMatchKeyAsync(
                inputResource,
                inputResourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullMatchKeyWhenCodeHasNoCodingPropertyAsync()
        {
            // given
            JsonElement inputResource = CreateMedicationResourceWithNoCodingProperty();
            Dictionary<string, JsonElement> inputResourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey = await this.medicationMatcherService.GetMatchKeyAsync(
                inputResource,
                inputResourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullMatchKeyWhenNoCodingHasSnomedSystemAsync()
        {
            // given
            JsonElement inputResource = CreateNonSnomedMedicationResource(onsetDateTime: "2024-01-01");
            Dictionary<string, JsonElement> inputResourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey = await this.medicationMatcherService.GetMatchKeyAsync(
                inputResource,
                inputResourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
