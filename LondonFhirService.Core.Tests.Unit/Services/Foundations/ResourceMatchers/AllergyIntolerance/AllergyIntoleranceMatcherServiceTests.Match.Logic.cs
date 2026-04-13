// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.AllergyIntolerances
{
    public partial class AllergyIntoleranceMatcherServiceTests
    {
        [Fact]
        public async Task ShouldReturnMatchedResourcesWhenBothSourcesContainSameKeyAsync()
        {
            // given
            string randomSnomedCode = GetRandomSnomedCode();
            string randomOnsetDateTime = GetRandomDateTimeOffset().ToString();
            string expectedMatchKey = $"{randomSnomedCode}|{randomOnsetDateTime}";

            JsonElement source1Resource =
                CreateAllergyIntoleranceResource(randomSnomedCode, randomOnsetDateTime);

            JsonElement source2Resource =
                CreateAllergyIntoleranceResource(randomSnomedCode, randomOnsetDateTime);

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Matched.Add(
                new MatchedResource(source1Resource, source2Resource, expectedMatchKey));

            // when
            ResourceMatch actualResourceMatch =
                await this.allergyIntoleranceMatcherService.MatchAsync(
                    source1Resources,
                    source2Resources,
                    source1ResourceIndex,
                    source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnUnmatchedResourceFromSource1WhenOnlySource1HasResourceAsync()
        {
            // given
            string randomSnomedCode = GetRandomSnomedCode();
            string randomOnsetDateTime = GetRandomDateTimeOffset().ToString();
            string expectedMatchKey = $"{randomSnomedCode}|{randomOnsetDateTime}";
            JsonElement source1Resource = CreateAllergyIntoleranceResource(randomSnomedCode, randomOnsetDateTime);
            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement>();
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Unmatched.Add(
                new UnmatchedResource(source1Resource, "AllergyIntolerance", expectedMatchKey, true));

            // when
            ResourceMatch actualResourceMatch =
                await this.allergyIntoleranceMatcherService.MatchAsync(
                    source1Resources,
                    source2Resources,
                    source1ResourceIndex,
                    source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnUnmatchedResourceFromSource2WhenOnlySource2HasResourceAsync()
        {
            // given
            string randomSnomedCode = GetRandomSnomedCode();
            string randomOnsetDateTime = GetRandomDateTimeOffset().ToString();
            string expectedMatchKey = $"{randomSnomedCode}|{randomOnsetDateTime}";
            JsonElement source2Resource = CreateAllergyIntoleranceResource(randomSnomedCode, randomOnsetDateTime);
            var source1Resources = new List<JsonElement>();
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Unmatched.Add(
                new UnmatchedResource(source2Resource, "AllergyIntolerance", expectedMatchKey, false));

            // when
            ResourceMatch actualResourceMatch =
                await this.allergyIntoleranceMatcherService.MatchAsync(
                    source1Resources,
                    source2Resources,
                    source1ResourceIndex,
                    source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnEmptyMatchWhenBothSourcesAreEmptyAsync()
        {
            // given
            var source1Resources = new List<JsonElement>();
            var source2Resources = new List<JsonElement>();
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            // when
            ResourceMatch actualResourceMatch =
                await this.allergyIntoleranceMatcherService.MatchAsync(
                    source1Resources,
                    source2Resources,
                    source1ResourceIndex,
                    source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldSkipResourcesWithNoMatchKeyWhenMatchingAsync()
        {
            // given
            string randomSnomedCode = GetRandomSnomedCode();
            string randomOnsetDateTime = GetRandomDateTimeOffset().ToString();

            JsonElement nonSnomedResource =
                CreateNonSnomedAllergyIntoleranceResource(randomOnsetDateTime);

            JsonElement resourceWithoutOnsetDateTime = CreateResourceWithoutOnsetDateTime(randomSnomedCode);
            var source1Resources = new List<JsonElement> { nonSnomedResource };
            var source2Resources = new List<JsonElement> { resourceWithoutOnsetDateTime };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            // when
            ResourceMatch actualResourceMatch =
                await this.allergyIntoleranceMatcherService.MatchAsync(
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
