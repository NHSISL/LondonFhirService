// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.AllergyIntolerances;

public partial class AllergyIntoleranceMatcherServiceTests
{
    [Fact]
    public void ShouldMatchAllergyIntoleranceResources()
    {
        // given
        JsonElement source1MatchedResource =
            CreateAllergyIntoleranceResource(
                snomedCode: "91936005",
                onsetDateTime: "2024-01-01",
                id: "source1-match");

        JsonElement source1UnmatchedResource =
            CreateAllergyIntoleranceResource(
                snomedCode: "300916003",
                onsetDateTime: "2024-02-01",
                id: "source1-unmatched");

        JsonElement source2MatchedResource =
            CreateAllergyIntoleranceResource(
                snomedCode: "91936005",
                onsetDateTime: "2024-01-01",
                id: "source2-match");

        JsonElement source2UnmatchedResource =
            CreateAllergyIntoleranceResource(
                snomedCode: "227493005",
                onsetDateTime: "2024-03-01",
                id: "source2-unmatched");

        List<JsonElement> source1Resources =
            [source1MatchedResource, source1UnmatchedResource];

        List<JsonElement> source2Resources =
            [source2MatchedResource, source2UnmatchedResource];

        Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
        Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

        ResourceMatch expectedResourceMatch = new()
        {
            Matched =
            [
                new MatchedResource(
                    Source1: source1MatchedResource,
                    Source2: source2MatchedResource,
                    MatchKey: "91936005|2024-01-01")
            ],
            Unmatched =
            [
                new UnmatchedResource(
                    Resource: source1UnmatchedResource,
                    ResourceType: "AllergyIntolerance",
                    Identifier: "300916003|2024-02-01",
                    IsFromSource1: true),

                new UnmatchedResource(
                    Resource: source2UnmatchedResource,
                    ResourceType: "AllergyIntolerance",
                    Identifier: "227493005|2024-03-01",
                    IsFromSource1: false)
            ]
        };

        // when
        ResourceMatch actualResourceMatch = this.allergyIntoleranceMatcherService.Match(
            source1Resources,
            source2Resources,
            source1ResourceIndex,
            source2ResourceIndex);

        // then
        actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
    }

    [Fact]
    public void ShouldIgnoreResourcesThatDoNotProduceMatchKeys()
    {
        // given
        JsonElement source1ResourceWithoutMatchKey =
            CreateResourceWithoutOnsetDateTime(snomedCode: "91936005");

        List<JsonElement> source1Resources = [source1ResourceWithoutMatchKey];
        List<JsonElement> source2Resources = [];
        Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
        Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

        // when
        ResourceMatch actualResourceMatch = this.allergyIntoleranceMatcherService.Match(
            source1Resources,
            source2Resources,
            source1ResourceIndex,
            source2ResourceIndex);

        // then
        actualResourceMatch.Matched.Should().BeEmpty();
        actualResourceMatch.Unmatched.Should().BeEmpty();
    }
}
