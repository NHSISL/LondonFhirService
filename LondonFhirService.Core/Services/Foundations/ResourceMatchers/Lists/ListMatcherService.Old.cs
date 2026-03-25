// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers.Lists;

public class ListMatcherServiceOld : IResourceMatcherService
{
    public string ResourceType => "List";

    public string? GetMatchKey(JsonElement resource, Dictionary<string, JsonElement> resourceIndex)
    {
        if (!resource.TryGetProperty("title", out var title))
            return null;

        return title.GetString();
    }

    public ResourceMatch Match(
        List<JsonElement> source1Resources,
        List<JsonElement> source2Resources,
        Dictionary<string, JsonElement> source1ResourceIndex,
        Dictionary<string, JsonElement> source2ResourceIndex)
    {
        var resourceMatch = new ResourceMatch();

        var source1ByKey = source1Resources
            .Select(r => new { Resource = r, Key = GetMatchKey(r, source1ResourceIndex) })
            .Where(x => x.Key != null)
            .ToDictionary(x => x.Key!, x => x.Resource);

        var source2ByKey = source2Resources
            .Select(r => new { Resource = r, Key = GetMatchKey(r, source2ResourceIndex) })
            .Where(x => x.Key != null)
            .ToDictionary(x => x.Key!, x => x.Resource);

        var allKeys = source1ByKey.Keys.Union(source2ByKey.Keys).ToList();

        foreach (var key in allKeys)
        {
            var hasSource1 = source1ByKey.TryGetValue(key, out var source1Resource);
            var hasSource2 = source2ByKey.TryGetValue(key, out var source2Resource);

            if (hasSource1 && hasSource2)
            {
                resourceMatch.Matched.Add(new MatchedResource(source1Resource!, source2Resource!, key));
            }
            else if (hasSource1)
            {
                resourceMatch.Unmatched.Add(new UnmatchedResource(source1Resource!, ResourceType, key, true));
            }
            else if (hasSource2)
            {
                resourceMatch.Unmatched.Add(new UnmatchedResource(source2Resource!, ResourceType, key, false));
            }
        }

        return resourceMatch;
    }
}
