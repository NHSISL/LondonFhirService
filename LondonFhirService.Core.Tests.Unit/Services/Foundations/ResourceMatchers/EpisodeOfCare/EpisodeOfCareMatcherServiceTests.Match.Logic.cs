// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.EpisodesOfCare
{
    public partial class EpisodeOfCareMatcherServiceTests
    {
        [Fact]
        public async Task ShouldMatchEpisodeOfCareResourcesFromBothSourcesAsync()
        {
            // given
            var periodStart = "2024-01-01";
            JsonElement source1Resource = CreateEpisodeOfCareResourceWithPeriodStart(periodStart);
            JsonElement source2Resource = CreateEpisodeOfCareResourceWithPeriodStart(periodStart);
            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Matched.Add(
                new MatchedResource(source1Resource, source2Resource, MatchKey: periodStart));

            // when
            ResourceMatch actualResourceMatch = await this.episodeOfCareMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnUnmatchedForSource1EpisodeOfCareNotInSource2Async()
        {
            // given
            var periodStart = "2024-01-01";
            JsonElement source1Resource = CreateEpisodeOfCareResourceWithPeriodStart(periodStart);
            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement>();
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Unmatched.Add(
                new UnmatchedResource(
                    Resource: source1Resource,
                    ResourceType: "EpisodeOfCare",
                    Identifier: periodStart,
                    IsFromSource1: true));

            // when
            ResourceMatch actualResourceMatch = await this.episodeOfCareMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnUnmatchedForSource2EpisodeOfCareNotInSource1Async()
        {
            // given
            var periodStart = "2024-01-01";
            JsonElement source2Resource = CreateEpisodeOfCareResourceWithPeriodStart(periodStart);
            var source1Resources = new List<JsonElement>();
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Unmatched.Add(
                new UnmatchedResource(
                    Resource: source2Resource,
                    ResourceType: "EpisodeOfCare",
                    Identifier: periodStart,
                    IsFromSource1: false));

            // when
            ResourceMatch actualResourceMatch = await this.episodeOfCareMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldMatchComprehensiveEpisodeOfCaresWhenIdsDifferAndPeriodStartsMatchAsync()
        {
            // given
            var periodStart = "2022-04-12";

            JsonElement source1Resource = CreateComprehensiveEpisodeOfCareResource(
                periodStart: periodStart,
                id: "episode-of-care-comprehensive-1");

            JsonElement source2Resource = CreateComprehensiveEpisodeOfCareResource(
                periodStart: periodStart,
                id: "episode-of-care-comprehensive-2");

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Matched.Add(
                new MatchedResource(source1Resource, source2Resource, MatchKey: periodStart));

            // when
            ResourceMatch actualResourceMatch = await this.episodeOfCareMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldExcludeEpisodeOfCareResourcesWithNoPeriodFromMatchingAsync()
        {
            // given
            JsonElement resourceWithNoPeriod = CreateEpisodeOfCareResourceWithNoPeriod();
            var source1Resources = new List<JsonElement> { resourceWithNoPeriod };
            var source2Resources = new List<JsonElement>();
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            // when
            ResourceMatch actualResourceMatch = await this.episodeOfCareMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnEmptyMatchWhenBothEpisodeOfCareSourcesAreEmptyAsync()
        {
            // given
            var source1Resources = new List<JsonElement>();
            var source2Resources = new List<JsonElement>();
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            // when
            ResourceMatch actualResourceMatch = await this.episodeOfCareMatcherService.MatchAsync(
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
