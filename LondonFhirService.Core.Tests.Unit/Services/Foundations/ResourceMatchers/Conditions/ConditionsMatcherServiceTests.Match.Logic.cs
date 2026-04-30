// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Conditions
{
    public partial class ConditionMatcherServiceTests
    {
        [Fact]
        public async Task ShouldMatchConditionsWhenBothSourcesHaveResourcesWithSameKeyAsync()
        {
            // given
            string inputSnomedCode = "444814009";

            JsonElement source1Resource = CreateConditionResource(
                snomedCode: inputSnomedCode,
                onsetDateTime: "2024-01-01",
                id: "condition-1");

            JsonElement source2Resource = CreateConditionResource(
                snomedCode: inputSnomedCode,
                onsetDateTime: "2024-06-01",
                id: "condition-2");

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Matched.Add(
                new MatchedResource(source1Resource, source2Resource, inputSnomedCode));

            // when
            ResourceMatch actualResourceMatch = await this.conditionMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldAddToUnmatchedFromSource1WhenOnlySource1HasConditionAsync()
        {
            // given
            string inputSnomedCode = "444814009";

            JsonElement source1Resource = CreateConditionResource(
                snomedCode: inputSnomedCode,
                onsetDateTime: "2024-01-01",
                id: "condition-1");

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement>();
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Unmatched.Add(
                new UnmatchedResource(source1Resource, "Condition", inputSnomedCode, true));

            // when
            ResourceMatch actualResourceMatch = await this.conditionMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldAddToUnmatchedFromSource2WhenOnlySource2HasConditionAsync()
        {
            // given
            string inputSnomedCode = "444814009";

            JsonElement source2Resource = CreateConditionResource(
                snomedCode: inputSnomedCode,
                onsetDateTime: "2024-01-01",
                id: "condition-1");

            var source1Resources = new List<JsonElement>();
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Unmatched.Add(
                new UnmatchedResource(source2Resource, "Condition", inputSnomedCode, false));

            // when
            ResourceMatch actualResourceMatch = await this.conditionMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldMatchComprehensiveConditionsWithMultipleIdentifierSystemsAsync()
        {
            // given
            string inputSnomedCode = "44054006";
            string inputOnsetDateTime = "2022-04-01";

            JsonElement source1Resource = CreateComprehensiveConditionResource(
                snomedCode: inputSnomedCode,
                onsetDateTime: inputOnsetDateTime,
                id: "condition-comprehensive-1");

            JsonElement source2Resource = CreateComprehensiveConditionResource(
                snomedCode: inputSnomedCode,
                onsetDateTime: inputOnsetDateTime,
                id: "condition-comprehensive-2");

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Matched.Add(
                new MatchedResource(source1Resource, source2Resource, inputSnomedCode));

            // when
            ResourceMatch actualResourceMatch = await this.conditionMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldExcludeConditionsWithNoSnomedCodeFromMatchResultsAsync()
        {
            // given
            JsonElement source1Resource = CreateNonSnomedConditionResource(onsetDateTime: "2024-01-01");
            JsonElement source2Resource = CreateNonSnomedConditionResource(onsetDateTime: "2024-06-01");
            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            // when
            ResourceMatch actualResourceMatch = await this.conditionMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
