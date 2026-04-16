// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.MedicationStatements
{
    public partial class MedicationStatementMatcherServiceTests
    {
        [Fact]
        public async Task ShouldReturnEmptyMatchWhenBothSourcesAreEmptyAsync()
        {
            // given
            List<JsonElement> inputSource1Resources = new();
            List<JsonElement> inputSource2Resources = new();
            Dictionary<string, JsonElement> inputSource1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> inputSource2ResourceIndex = CreateResourceIndex();

            // when
            ResourceMatch actualResourceMatch =
                await this.medicationStatementMatcherService.MatchAsync(
                    inputSource1Resources,
                    inputSource2Resources,
                    inputSource1ResourceIndex,
                    inputSource2ResourceIndex);

            // then
            actualResourceMatch.Matched.Should().BeEmpty();
            actualResourceMatch.Unmatched.Should().BeEmpty();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnMatchedResourceWhenBothSourcesHaveSameKeyAsync()
        {
            // given
            string randomSnomedCode = "123456789";
            string randomDateAsserted = "2024-06-15";
            string inputSnomedCode = randomSnomedCode;
            string inputDateAsserted = randomDateAsserted;

            JsonElement inputSource1Resource =
                CreateMedicationStatementWithCodeableConcept(
                    inputSnomedCode,
                    inputDateAsserted,
                    id: "stmt-s1");

            JsonElement inputSource2Resource =
                CreateMedicationStatementWithCodeableConcept(
                    inputSnomedCode,
                    inputDateAsserted,
                    id: "stmt-s2");

            List<JsonElement> inputSource1Resources = new() { inputSource1Resource };
            List<JsonElement> inputSource2Resources = new() { inputSource2Resource };
            Dictionary<string, JsonElement> inputSource1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> inputSource2ResourceIndex = CreateResourceIndex();
            string expectedMatchKey = $"{inputSnomedCode}|{inputDateAsserted}";

            // when
            ResourceMatch actualResourceMatch =
                await this.medicationStatementMatcherService.MatchAsync(
                    inputSource1Resources,
                    inputSource2Resources,
                    inputSource1ResourceIndex,
                    inputSource2ResourceIndex);

            // then
            actualResourceMatch.Matched.Should().HaveCount(1);
            actualResourceMatch.Matched[0].MatchKey.Should().Be(expectedMatchKey);

            actualResourceMatch.Matched[0].Source1.GetRawText()
                .Should().Be(inputSource1Resource.GetRawText());

            actualResourceMatch.Matched[0].Source2.GetRawText()
                .Should().Be(inputSource2Resource.GetRawText());

            actualResourceMatch.Unmatched.Should().BeEmpty();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnUnmatchedFromSource1WhenOnlySource1HasResourceAsync()
        {
            // given
            string randomSnomedCode = "123456789";
            string randomDateAsserted = "2024-06-15";
            string inputSnomedCode = randomSnomedCode;
            string inputDateAsserted = randomDateAsserted;

            JsonElement inputSource1Resource =
                CreateMedicationStatementWithCodeableConcept(
                    inputSnomedCode,
                    inputDateAsserted,
                    id: "stmt-s1-only");

            List<JsonElement> inputSource1Resources = new() { inputSource1Resource };
            List<JsonElement> inputSource2Resources = new();
            Dictionary<string, JsonElement> inputSource1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> inputSource2ResourceIndex = CreateResourceIndex();
            string expectedMatchKey = $"{inputSnomedCode}|{inputDateAsserted}";

            // when
            ResourceMatch actualResourceMatch =
                await this.medicationStatementMatcherService.MatchAsync(
                    inputSource1Resources,
                    inputSource2Resources,
                    inputSource1ResourceIndex,
                    inputSource2ResourceIndex);

            // then
            actualResourceMatch.Unmatched.Should().HaveCount(1);
            actualResourceMatch.Unmatched[0].Identifier.Should().Be(expectedMatchKey);
            actualResourceMatch.Unmatched[0].IsFromSource1.Should().BeTrue();
            actualResourceMatch.Unmatched[0].ResourceType.Should().Be("MedicationStatement");

            actualResourceMatch.Unmatched[0].Resource.GetRawText()
                .Should().Be(inputSource1Resource.GetRawText());

            actualResourceMatch.Matched.Should().BeEmpty();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnUnmatchedFromSource2WhenOnlySource2HasResourceAsync()
        {
            // given
            string randomSnomedCode = "987654321";
            string randomDateAsserted = "2024-07-20";
            string inputSnomedCode = randomSnomedCode;
            string inputDateAsserted = randomDateAsserted;

            JsonElement inputSource2Resource =
                CreateMedicationStatementWithCodeableConcept(
                    inputSnomedCode,
                    inputDateAsserted,
                    id: "stmt-s2-only");

            List<JsonElement> inputSource1Resources = new();
            List<JsonElement> inputSource2Resources = new() { inputSource2Resource };
            Dictionary<string, JsonElement> inputSource1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> inputSource2ResourceIndex = CreateResourceIndex();
            string expectedMatchKey = $"{inputSnomedCode}|{inputDateAsserted}";

            // when
            ResourceMatch actualResourceMatch =
                await this.medicationStatementMatcherService.MatchAsync(
                    inputSource1Resources,
                    inputSource2Resources,
                    inputSource1ResourceIndex,
                    inputSource2ResourceIndex);

            // then
            actualResourceMatch.Unmatched.Should().HaveCount(1);
            actualResourceMatch.Unmatched[0].Identifier.Should().Be(expectedMatchKey);
            actualResourceMatch.Unmatched[0].IsFromSource1.Should().BeFalse();
            actualResourceMatch.Unmatched[0].ResourceType.Should().Be("MedicationStatement");

            actualResourceMatch.Unmatched[0].Resource.GetRawText()
                .Should().Be(inputSource2Resource.GetRawText());

            actualResourceMatch.Matched.Should().BeEmpty();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldExcludeResourcesWithNullMatchKeyFromResultsAsync()
        {
            // given
            string randomDateAsserted = "2024-06-15";
            string inputDateAsserted = randomDateAsserted;

            JsonElement inputSource1Resource =
                CreateNonSnomedMedicationStatement(
                    inputDateAsserted,
                    id: "stmt-no-snomed");

            List<JsonElement> inputSource1Resources = new() { inputSource1Resource };
            List<JsonElement> inputSource2Resources = new();
            Dictionary<string, JsonElement> inputSource1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> inputSource2ResourceIndex = CreateResourceIndex();

            // when
            ResourceMatch actualResourceMatch =
                await this.medicationStatementMatcherService.MatchAsync(
                    inputSource1Resources,
                    inputSource2Resources,
                    inputSource1ResourceIndex,
                    inputSource2ResourceIndex);

            // then
            actualResourceMatch.Matched.Should().BeEmpty();
            actualResourceMatch.Unmatched.Should().BeEmpty();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnBothMatchedAndUnmatchedResourcesAsync()
        {
            // given
            string randomSnomedCodeA = "111111111";
            string randomSnomedCodeB = "222222222";
            string randomSnomedCodeC = "333333333";
            string randomSnomedCodeD = "444444444";
            string randomDateAsserted = "2024-06-15";

            JsonElement source1ResourceA =
                CreateMedicationStatementWithCodeableConcept(
                    randomSnomedCodeA,
                    randomDateAsserted,
                    id: "s1-a");

            JsonElement source1ResourceB =
                CreateMedicationStatementWithCodeableConcept(
                    randomSnomedCodeB,
                    randomDateAsserted,
                    id: "s1-b");

            JsonElement source1ResourceC =
                CreateMedicationStatementWithCodeableConcept(
                    randomSnomedCodeC,
                    randomDateAsserted,
                    id: "s1-c");

            JsonElement source2ResourceA =
                CreateMedicationStatementWithCodeableConcept(
                    randomSnomedCodeA,
                    randomDateAsserted,
                    id: "s2-a");

            JsonElement source2ResourceC =
                CreateMedicationStatementWithCodeableConcept(
                    randomSnomedCodeC,
                    randomDateAsserted,
                    id: "s2-c");

            JsonElement source2ResourceD =
                CreateMedicationStatementWithCodeableConcept(
                    randomSnomedCodeD,
                    randomDateAsserted,
                    id: "s2-d");

            List<JsonElement> inputSource1Resources =
                new() { source1ResourceA, source1ResourceB, source1ResourceC };

            List<JsonElement> inputSource2Resources =
                new() { source2ResourceA, source2ResourceC, source2ResourceD };

            Dictionary<string, JsonElement> inputSource1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> inputSource2ResourceIndex = CreateResourceIndex();

            // when
            ResourceMatch actualResourceMatch =
                await this.medicationStatementMatcherService.MatchAsync(
                    inputSource1Resources,
                    inputSource2Resources,
                    inputSource1ResourceIndex,
                    inputSource2ResourceIndex);

            // then
            actualResourceMatch.Matched.Should().HaveCount(2);
            actualResourceMatch.Unmatched.Should().HaveCount(2);

            actualResourceMatch.Matched.Should().ContainSingle(
                match => match.MatchKey == $"{randomSnomedCodeA}|{randomDateAsserted}");

            actualResourceMatch.Matched.Should().ContainSingle(
                match => match.MatchKey == $"{randomSnomedCodeC}|{randomDateAsserted}");

            actualResourceMatch.Unmatched.Should().ContainSingle(
                unmatched =>
                    unmatched.Identifier == $"{randomSnomedCodeB}|{randomDateAsserted}"
                    && unmatched.IsFromSource1);

            actualResourceMatch.Unmatched.Should().ContainSingle(
                unmatched =>
                    unmatched.Identifier == $"{randomSnomedCodeD}|{randomDateAsserted}"
                    && !unmatched.IsFromSource1);

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
