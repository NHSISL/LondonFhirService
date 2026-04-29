// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.PractitionerRoles
{
    public partial class PractitionerRoleMatcherServiceTests
    {
        [Fact]
        public async Task ShouldMatchPractitionerRolesWhenBothSourcesHaveResourcesWithSameKeyAsync()
        {
            // given
            string inputSdsRoleProfileId = "999999000123-A1";

            JsonElement source1Resource = CreatePractitionerRoleResource(
                sdsRoleProfileId: inputSdsRoleProfileId,
                id: "practitioner-role-1");

            JsonElement source2Resource = CreatePractitionerRoleResource(
                sdsRoleProfileId: inputSdsRoleProfileId,
                id: "practitioner-role-2");

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Matched.Add(
                new MatchedResource(source1Resource, source2Resource, inputSdsRoleProfileId));

            // when
            ResourceMatch actualResourceMatch = await this.practitionerRoleMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldAddToUnmatchedFromSource1WhenOnlySource1HasPractitionerRoleAsync()
        {
            // given
            string inputSdsRoleProfileId = "999999000123-A1";

            JsonElement source1Resource = CreatePractitionerRoleResource(
                sdsRoleProfileId: inputSdsRoleProfileId,
                id: "practitioner-role-1");

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement>();
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Unmatched.Add(
                new UnmatchedResource(source1Resource, "PractitionerRole", inputSdsRoleProfileId, true));

            // when
            ResourceMatch actualResourceMatch = await this.practitionerRoleMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldAddToUnmatchedFromSource2WhenOnlySource2HasPractitionerRoleAsync()
        {
            // given
            string inputSdsRoleProfileId = "999999000123-A1";

            JsonElement source2Resource = CreatePractitionerRoleResource(
                sdsRoleProfileId: inputSdsRoleProfileId,
                id: "practitioner-role-1");

            var source1Resources = new List<JsonElement>();
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Unmatched.Add(
                new UnmatchedResource(source2Resource, "PractitionerRole", inputSdsRoleProfileId, false));

            // when
            ResourceMatch actualResourceMatch = await this.practitionerRoleMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldExcludePractitionerRolesWithNoSdsRoleProfileIdFromMatchResultsAsync()
        {
            // given
            JsonElement source1Resource = CreateNonSdsPractitionerRoleResource(id: "practitioner-role-1");
            JsonElement source2Resource = CreateNonSdsPractitionerRoleResource(id: "practitioner-role-2");
            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            // when
            ResourceMatch actualResourceMatch = await this.practitionerRoleMatcherService.MatchAsync(
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
