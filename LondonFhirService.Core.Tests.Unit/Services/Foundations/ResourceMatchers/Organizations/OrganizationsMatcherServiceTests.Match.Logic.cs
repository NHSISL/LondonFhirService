// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Organizations
{
    public partial class OrganizationMatcherServiceTests
    {
        [Fact]
        public async Task ShouldMatchOrganizationsWhenBothSourcesHaveResourcesWithSameKeyAsync()
        {
            // given
            string inputOdsOrganizationCode = "A82817";

            JsonElement source1Resource = CreateOrganizationResource(
                odsOrganizationCode: inputOdsOrganizationCode,
                id: "organization-1");

            JsonElement source2Resource = CreateOrganizationResource(
                odsOrganizationCode: inputOdsOrganizationCode,
                id: "organization-2");

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Matched.Add(
                new MatchedResource(
                    Source1: source1Resource,
                    Source2: source2Resource,
                    MatchKey: inputOdsOrganizationCode));

            // when
            ResourceMatch actualResourceMatch = await this.organizationMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldAddToUnmatchedFromSource1WhenOnlySource1HasOrganizationAsync()
        {
            // given
            string inputOdsOrganizationCode = "A82817";

            JsonElement source1Resource = CreateOrganizationResource(
                odsOrganizationCode: inputOdsOrganizationCode,
                id: "organization-1");

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement>();
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Unmatched.Add(
                new UnmatchedResource(
                    Resource: source1Resource,
                    ResourceType: "Organization",
                    Identifier: inputOdsOrganizationCode,
                    IsFromSource1: true));

            // when
            ResourceMatch actualResourceMatch = await this.organizationMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldAddToUnmatchedFromSource2WhenOnlySource2HasOrganizationAsync()
        {
            // given
            string inputOdsOrganizationCode = "A82817";

            JsonElement source2Resource = CreateOrganizationResource(
                odsOrganizationCode: inputOdsOrganizationCode,
                id: "organization-1");

            var source1Resources = new List<JsonElement>();
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Unmatched.Add(
                new UnmatchedResource(
                    Resource: source2Resource,
                    ResourceType: "Organization",
                    Identifier: inputOdsOrganizationCode,
                    IsFromSource1: false));

            // when
            ResourceMatch actualResourceMatch = await this.organizationMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldMatchComprehensiveOrganizationsWithMultipleIdentifierSystemsAsync()
        {
            // given
            string inputOdsOrganizationCode = "A82817";

            JsonElement source1Resource = CreateComprehensiveOrganizationResource(
                odsOrganizationCode: inputOdsOrganizationCode,
                id: "organization-comprehensive-1");

            JsonElement source2Resource = CreateComprehensiveOrganizationResource(
                odsOrganizationCode: inputOdsOrganizationCode,
                id: "organization-comprehensive-2");

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Matched.Add(
                new MatchedResource(
                    Source1: source1Resource,
                    Source2: source2Resource,
                    MatchKey: inputOdsOrganizationCode));

            // when
            ResourceMatch actualResourceMatch = await this.organizationMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldExcludeOrganizationsWithNoOdsOrganizationCodeFromMatchResultsAsync()
        {
            // given
            JsonElement source1Resource = CreateNonOdsOrganizationResource(id: "organization-1");
            JsonElement source2Resource = CreateNonOdsOrganizationResource(id: "organization-2");
            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            // when
            ResourceMatch actualResourceMatch = await this.organizationMatcherService.MatchAsync(
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
