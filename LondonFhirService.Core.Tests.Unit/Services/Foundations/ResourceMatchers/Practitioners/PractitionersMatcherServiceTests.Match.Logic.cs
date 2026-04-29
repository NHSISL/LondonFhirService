// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Practitioners
{
    public partial class PractitionerMatcherServiceTests
    {
        [Fact]
        public async Task ShouldMatchPractitionersWhenBothSourcesHaveResourcesWithSameKeyAsync()
        {
            // given
            string inputSdsUserId = "999999000123";

            JsonElement source1Resource = CreatePractitionerResource(
                sdsUserId: inputSdsUserId,
                id: "practitioner-1");

            JsonElement source2Resource = CreatePractitionerResource(
                sdsUserId: inputSdsUserId,
                id: "practitioner-2");

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Matched.Add(
                new MatchedResource(source1Resource, source2Resource, inputSdsUserId));

            // when
            ResourceMatch actualResourceMatch = await this.practitionerMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldAddToUnmatchedFromSource1WhenOnlySource1HasPractitionerAsync()
        {
            // given
            string inputSdsUserId = "999999000123";

            JsonElement source1Resource = CreatePractitionerResource(
                sdsUserId: inputSdsUserId,
                id: "practitioner-1");

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement>();
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Unmatched.Add(
                new UnmatchedResource(source1Resource, "Practitioner", inputSdsUserId, true));

            // when
            ResourceMatch actualResourceMatch = await this.practitionerMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldAddToUnmatchedFromSource2WhenOnlySource2HasPractitionerAsync()
        {
            // given
            string inputSdsUserId = "999999000123";

            JsonElement source2Resource = CreatePractitionerResource(
                sdsUserId: inputSdsUserId,
                id: "practitioner-1");

            var source1Resources = new List<JsonElement>();
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Unmatched.Add(
                new UnmatchedResource(source2Resource, "Practitioner", inputSdsUserId, false));

            // when
            ResourceMatch actualResourceMatch = await this.practitionerMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldExcludePractitionersWithNoSdsUserIdFromMatchResultsAsync()
        {
            // given
            JsonElement source1Resource = CreateNonSdsPractitionerResource(id: "practitioner-1");
            JsonElement source2Resource = CreateNonSdsPractitionerResource(id: "practitioner-2");
            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            // when
            ResourceMatch actualResourceMatch = await this.practitionerMatcherService.MatchAsync(
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
