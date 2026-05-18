// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Observations
{
    public partial class ObservationMatcherServiceTests
    {
        [Fact]
        public async Task ShouldMatchObservationsWhenBothSourcesHaveResourcesWithSameKeyAsync()
        {
            // given
            string inputDdsIdentifierValue = "OBS-1";

            JsonElement source1Resource = CreateObservationResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: "observation-1");

            JsonElement source2Resource = CreateObservationResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: "observation-2");

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Matched.Add(
                new MatchedResource(source1Resource, source2Resource, inputDdsIdentifierValue));

            // when
            ResourceMatch actualResourceMatch = await this.observationMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldAddToUnmatchedFromSource1WhenOnlySource1HasObservationAsync()
        {
            // given
            string inputDdsIdentifierValue = "OBS-1";

            JsonElement source1Resource = CreateObservationResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: "observation-1");

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement>();
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Unmatched.Add(
                new UnmatchedResource(source1Resource, "Observation", inputDdsIdentifierValue, true));

            // when
            ResourceMatch actualResourceMatch = await this.observationMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldAddToUnmatchedFromSource2WhenOnlySource2HasObservationAsync()
        {
            // given
            string inputDdsIdentifierValue = "OBS-1";

            JsonElement source2Resource = CreateObservationResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: "observation-1");

            var source1Resources = new List<JsonElement>();
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Unmatched.Add(
                new UnmatchedResource(source2Resource, "Observation", inputDdsIdentifierValue, false));

            // when
            ResourceMatch actualResourceMatch = await this.observationMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldMatchComprehensiveObservationsWithMultipleIdentifierSystemsAsync()
        {
            // given
            string inputDdsIdentifierValue = "OBS-comprehensive-1";

            JsonElement source1Resource = CreateComprehensiveObservationResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: "observation-comprehensive-1");

            JsonElement source2Resource = CreateComprehensiveObservationResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: "observation-comprehensive-2");

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Matched.Add(
                new MatchedResource(source1Resource, source2Resource, inputDdsIdentifierValue));

            // when
            ResourceMatch actualResourceMatch = await this.observationMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldExcludeObservationsWithNoDdsIdentifierFromMatchResultsAsync()
        {
            // given
            JsonElement source1Resource = CreateNonDdsObservationResource(id: "observation-1");
            JsonElement source2Resource = CreateNonDdsObservationResource(id: "observation-2");
            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            // when
            ResourceMatch actualResourceMatch = await this.observationMatcherService.MatchAsync(
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
