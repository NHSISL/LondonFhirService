// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Medications
{
    public partial class MedicationMatcherServiceTests
    {
        [Fact]
        public async Task ShouldReturnMatchedResourcesWhenBothSourcesHaveSameKeyAsync()
        {
            // given
            string randomSnomedCode = GetRandomSnomedCode();

            JsonElement source1Resource = CreateMedicationResource(
                randomSnomedCode,
                onsetDateTime: "2024-01-01",
                id: "source1");

            JsonElement source2Resource = CreateMedicationResource(
                randomSnomedCode,
                onsetDateTime: "2024-01-01",
                id: "source2");

            var inputSource1Resources = new List<JsonElement> { source1Resource };
            var inputSource2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> inputSource1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> inputSource2ResourceIndex = CreateResourceIndex();
            string expectedMatchKey = randomSnomedCode;

            // when
            ResourceMatch actualResourceMatch = await this.medicationMatcherService.MatchAsync(
                inputSource1Resources,
                inputSource2Resources,
                inputSource1ResourceIndex,
                inputSource2ResourceIndex);

            // then
            actualResourceMatch.Matched.Should().HaveCount(1);
            actualResourceMatch.Unmatched.Should().BeEmpty();
            actualResourceMatch.Matched[0].MatchKey.Should().Be(expectedMatchKey);
            actualResourceMatch.Matched[0].Source1.GetRawText().Should().Be(source1Resource.GetRawText());
            actualResourceMatch.Matched[0].Source2.GetRawText().Should().Be(source2Resource.GetRawText());
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnUnmatchedForSource1WhenOnlySource1HasMatchKeyAsync()
        {
            // given
            string randomSnomedCode = GetRandomSnomedCode();
            JsonElement source1Resource = CreateMedicationResource(randomSnomedCode, onsetDateTime: "2024-01-01");
            var inputSource1Resources = new List<JsonElement> { source1Resource };
            var inputSource2Resources = new List<JsonElement>();
            Dictionary<string, JsonElement> inputSource1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> inputSource2ResourceIndex = CreateResourceIndex();
            string expectedMatchKey = randomSnomedCode;

            // when
            ResourceMatch actualResourceMatch = await this.medicationMatcherService.MatchAsync(
                inputSource1Resources,
                inputSource2Resources,
                inputSource1ResourceIndex,
                inputSource2ResourceIndex);

            // then
            actualResourceMatch.Matched.Should().BeEmpty();
            actualResourceMatch.Unmatched.Should().HaveCount(1);
            actualResourceMatch.Unmatched[0].Identifier.Should().Be(expectedMatchKey);
            actualResourceMatch.Unmatched[0].ResourceType.Should().Be("Medication");
            actualResourceMatch.Unmatched[0].IsFromSource1.Should().BeTrue();
            actualResourceMatch.Unmatched[0].Resource.GetRawText().Should().Be(source1Resource.GetRawText());
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnUnmatchedForSource2WhenOnlySource2HasMatchKeyAsync()
        {
            // given
            string randomSnomedCode = GetRandomSnomedCode();
            JsonElement source2Resource = CreateMedicationResource(randomSnomedCode, onsetDateTime: "2024-01-01");
            var inputSource1Resources = new List<JsonElement>();
            var inputSource2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> inputSource1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> inputSource2ResourceIndex = CreateResourceIndex();
            string expectedMatchKey = randomSnomedCode;

            // when
            ResourceMatch actualResourceMatch = await this.medicationMatcherService.MatchAsync(
                inputSource1Resources,
                inputSource2Resources,
                inputSource1ResourceIndex,
                inputSource2ResourceIndex);

            // then
            actualResourceMatch.Matched.Should().BeEmpty();
            actualResourceMatch.Unmatched.Should().HaveCount(1);
            actualResourceMatch.Unmatched[0].Identifier.Should().Be(expectedMatchKey);
            actualResourceMatch.Unmatched[0].ResourceType.Should().Be("Medication");
            actualResourceMatch.Unmatched[0].IsFromSource1.Should().BeFalse();
            actualResourceMatch.Unmatched[0].Resource.GetRawText().Should().Be(source2Resource.GetRawText());
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
