// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Lists
{
    public partial class ListMatcherServiceTests
    {
        [Fact]
        public async Task ShouldReturnMatchedResourcesOnMatchAsync()
        {
            // given
            string randomTitle = GetRandomString();
            JsonElement source1Resource = CreateListResourceWithTitle(randomTitle, id: "list-1");
            JsonElement source2Resource = CreateListResourceWithTitle(randomTitle, id: "list-2");
            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Matched.Add(
                new MatchedResource(source1Resource, source2Resource, MatchKey: randomTitle));

            // when
            ResourceMatch actualResourceMatch = await this.listMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnUnmatchedSource1ResourceOnMatchAsync()
        {
            // given
            string randomTitle = GetRandomString();
            JsonElement source1Resource = CreateListResourceWithTitle(randomTitle);
            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement>();
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Unmatched.Add(
                new UnmatchedResource(
                    source1Resource,
                    ResourceType: "List",
                    Identifier: randomTitle,
                    IsFromSource1: true));

            // when
            ResourceMatch actualResourceMatch = await this.listMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnUnmatchedSource2ResourceOnMatchAsync()
        {
            // given
            string randomTitle = GetRandomString();
            JsonElement source2Resource = CreateListResourceWithTitle(randomTitle);
            var source1Resources = new List<JsonElement>();
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Unmatched.Add(
                new UnmatchedResource(
                    source2Resource,
                    ResourceType: "List",
                    Identifier: randomTitle,
                    IsFromSource1: false));

            // when
            ResourceMatch actualResourceMatch = await this.listMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnEmptyResultWhenBothSourcesAreEmptyOnMatchAsync()
        {
            // given
            var source1Resources = new List<JsonElement>();
            var source2Resources = new List<JsonElement>();
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            // when
            ResourceMatch actualResourceMatch = await this.listMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldSkipResourcesWithMissingTitleOnMatchAsync()
        {
            // given
            JsonElement source1Resource = CreateListResourceWithoutTitle(id: "list-1");
            JsonElement source2Resource = CreateListResourceWithoutTitle(id: "list-2");
            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            // when
            ResourceMatch actualResourceMatch = await this.listMatcherService.MatchAsync(
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
