// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Lists;

public partial class ListMatcherServiceTests
{
    [Fact]
    public void ShouldMatchListResources()
    {
        JsonElement source1MatchedResource = CreateListResource("Problems", "source1-match");
        JsonElement source1UnmatchedResource = CreateListResource("Medications", "source1-unmatched");
        JsonElement source2MatchedResource = CreateListResource("Problems", "source2-match");
        JsonElement source2UnmatchedResource = CreateListResource("Allergies", "source2-unmatched");

        List<JsonElement> source1Resources = [source1MatchedResource, source1UnmatchedResource];
        List<JsonElement> source2Resources = [source2MatchedResource, source2UnmatchedResource];
        Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
        Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

        ResourceMatch expectedResourceMatch = new()
        {
            Matched =
            [
                new MatchedResource(source1MatchedResource, source2MatchedResource, "Problems")
            ],
            Unmatched =
            [
                new UnmatchedResource(source1UnmatchedResource, "List", "Medications", true),
                new UnmatchedResource(source2UnmatchedResource, "List", "Allergies", false)
            ]
        };

        ResourceMatch actualResourceMatch = this.listMatcherService.Match(
            source1Resources,
            source2Resources,
            source1ResourceIndex,
            source2ResourceIndex);

        actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
    }

    [Fact]
    public void ShouldIgnoreResourcesThatDoNotProduceMatchKeys()
    {
        JsonElement source1ResourceWithoutMatchKey = CreateListResourceWithoutTitle();

        List<JsonElement> source1Resources = [source1ResourceWithoutMatchKey];
        List<JsonElement> source2Resources = [];
        Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
        Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

        ResourceMatch actualResourceMatch = this.listMatcherService.Match(
            source1Resources,
            source2Resources,
            source1ResourceIndex,
            source2ResourceIndex);

        actualResourceMatch.Matched.Should().BeEmpty();
        actualResourceMatch.Unmatched.Should().BeEmpty();
    }
}