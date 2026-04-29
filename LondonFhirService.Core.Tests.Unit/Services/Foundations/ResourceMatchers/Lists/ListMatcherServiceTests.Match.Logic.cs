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
        public async Task ShouldReturnMatchedResourcesWhenBothSourcesContainSameTitleAsync()
        {
            // given
            string randomTitle = GetRandomTitle();

            JsonElement source1Resource = CreateListResourceWithTitle(randomTitle);
            JsonElement source2Resource = CreateListResourceWithTitle(randomTitle);
            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Matched.Add(
                new MatchedResource(source1Resource, source2Resource, randomTitle));

            // when
            ResourceMatch actualResourceMatch =
                await this.listMatcherService.MatchAsync(
                    source1Resources,
                    source2Resources,
                    source1ResourceIndex,
                    source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnUnmatchedResourceFromSource1WhenOnlySource1HasListAsync()
        {
            // given
            string randomTitle = GetRandomTitle();
            JsonElement source1Resource = CreateListResourceWithTitle(randomTitle);
            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement>();
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Unmatched.Add(
                new UnmatchedResource(source1Resource, "List", randomTitle, IsFromSource1: true));

            // when
            ResourceMatch actualResourceMatch =
                await this.listMatcherService.MatchAsync(
                    source1Resources,
                    source2Resources,
                    source1ResourceIndex,
                    source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnUnmatchedResourceFromSource2WhenOnlySource2HasListAsync()
        {
            // given
            string randomTitle = GetRandomTitle();
            JsonElement source2Resource = CreateListResourceWithTitle(randomTitle);
            var source1Resources = new List<JsonElement>();
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Unmatched.Add(
                new UnmatchedResource(source2Resource, "List", randomTitle, IsFromSource1: false));

            // when
            ResourceMatch actualResourceMatch =
                await this.listMatcherService.MatchAsync(
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
                await this.listMatcherService.MatchAsync(
                    source1Resources,
                    source2Resources,
                    source1ResourceIndex,
                    source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldSkipResourcesWithNoTitleWhenMatchingAsync()
        {
            // given
            JsonElement resourceWithoutTitle = CreateListResourceWithoutTitle();
            var source1Resources = new List<JsonElement> { resourceWithoutTitle };
            var source2Resources = new List<JsonElement> { resourceWithoutTitle };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            // when
            ResourceMatch actualResourceMatch =
                await this.listMatcherService.MatchAsync(
                    source1Resources,
                    source2Resources,
                    source1ResourceIndex,
                    source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldMatchDuplicateTitlesInOrderWhenBothSourcesHaveDuplicatesAsync()
        {
            // given
            string randomTitle = GetRandomTitle();

            JsonElement source1Resource1 = CreateListResourceWithTitle(randomTitle, id: "list-1");
            JsonElement source1Resource2 = CreateListResourceWithTitle(randomTitle, id: "list-2");
            JsonElement source2Resource1 = CreateListResourceWithTitle(randomTitle, id: "list-3");
            JsonElement source2Resource2 = CreateListResourceWithTitle(randomTitle, id: "list-4");

            var source1Resources = new List<JsonElement> { source1Resource1, source1Resource2 };
            var source2Resources = new List<JsonElement> { source2Resource1, source2Resource2 };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Matched.Add(
                new MatchedResource(source1Resource1, source2Resource1, randomTitle));

            expectedResourceMatch.Matched.Add(
                new MatchedResource(source1Resource2, source2Resource2, randomTitle));

            // when
            ResourceMatch actualResourceMatch =
                await this.listMatcherService.MatchAsync(
                    source1Resources,
                    source2Resources,
                    source1ResourceIndex,
                    source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnUnmatchedWhenSource1HasMoreDuplicatesAsync()
        {
            // given
            string randomTitle = GetRandomTitle();

            JsonElement source1Resource1 = CreateListResourceWithTitle(randomTitle, id: "list-1");
            JsonElement source1Resource2 = CreateListResourceWithTitle(randomTitle, id: "list-2");
            JsonElement source2Resource1 = CreateListResourceWithTitle(randomTitle, id: "list-3");

            var source1Resources = new List<JsonElement> { source1Resource1, source1Resource2 };
            var source2Resources = new List<JsonElement> { source2Resource1 };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Matched.Add(
                new MatchedResource(source1Resource1, source2Resource1, randomTitle));

            expectedResourceMatch.Unmatched.Add(
                new UnmatchedResource(source1Resource2, "List", randomTitle, IsFromSource1: true));

            // when
            ResourceMatch actualResourceMatch =
                await this.listMatcherService.MatchAsync(
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
