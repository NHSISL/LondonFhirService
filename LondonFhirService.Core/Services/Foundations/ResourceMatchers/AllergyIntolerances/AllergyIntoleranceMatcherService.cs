// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers.AllergyIntolerances;

public partial class AllergyIntoleranceMatcherService : IResourceMatcherService
{
    private const string SnomedSystem = "http://snomed.info/sct";

    public string ResourceType => "AllergyIntolerance";

    public string? GetMatchKey(JsonElement resource, Dictionary<string, JsonElement> resourceIndex) =>
    TryCatch(() =>
    {
        ValidateGetMatchKeyArguments(resource, resourceIndex);

        string? snomedCode = GetSnomedCode(resource);
        string? onsetDateTime = GetOnsetDateTime(resource);

        if (string.IsNullOrWhiteSpace(snomedCode) || string.IsNullOrWhiteSpace(onsetDateTime))
        {
            return null;
        }

        return $"{snomedCode}|{onsetDateTime}";
    });

    public ResourceMatch Match(
        List<JsonElement> source1Resources,
        List<JsonElement> source2Resources,
        Dictionary<string, JsonElement> source1ResourceIndex,
        Dictionary<string, JsonElement> source2ResourceIndex) =>
    TryCatch(() =>
    {
        ValidateMatchArguments(
            source1Resources, 
            source2Resources, 
            source1ResourceIndex, 
            source2ResourceIndex);

        var resourceMatch = new ResourceMatch();

        Dictionary<string, JsonElement> source1ByKey = source1Resources
            .Select(resource => new
            {
                Resource = resource,
                Key = GetMatchKey(resource, source1ResourceIndex)
            })
            .Where(entry => entry.Key is not null)
            .ToDictionary(entry => entry.Key!, entry => entry.Resource);

        Dictionary<string, JsonElement> source2ByKey = source2Resources
            .Select(resource => new
            {
                Resource = resource,
                Key = GetMatchKey(resource, source2ResourceIndex)
            })
            .Where(entry => entry.Key is not null)
            .ToDictionary(entry => entry.Key!, entry => entry.Resource);

        List<string> allKeys = source1ByKey.Keys.Union(source2ByKey.Keys).ToList();

        foreach (string key in allKeys)
        {
            bool hasSource1 = source1ByKey.TryGetValue(key, out JsonElement source1Resource);
            bool hasSource2 = source2ByKey.TryGetValue(key, out JsonElement source2Resource);

            if (hasSource1 && hasSource2)
            {
                resourceMatch.Matched.Add(new MatchedResource(source1Resource, source2Resource, key));
            }
            else if (hasSource1)
            {
                resourceMatch.Unmatched.Add(new UnmatchedResource(source1Resource, this.ResourceType, key, true));
            }
            else if (hasSource2)
            {
                resourceMatch.Unmatched.Add(new UnmatchedResource(source2Resource, this.ResourceType, key, false));
            }
        }

        return resourceMatch;
    });

    private static string? GetSnomedCode(JsonElement resource)
    {
        if (!resource.TryGetProperty("code", out JsonElement code))
        {
            return null;
        }

        if (!code.TryGetProperty("coding", out JsonElement coding))
        {
            return null;
        }

        foreach (JsonElement codingElement in coding.EnumerateArray())
        {
            if (!codingElement.TryGetProperty("system", out JsonElement system))
            {
                continue;
            }

            string? systemValue = system.GetString();

            if (systemValue == SnomedSystem &&
                codingElement.TryGetProperty("code", out JsonElement codeValue))
            {
                return codeValue.GetString();
            }
        }

        return null;
    }

    private static string? GetOnsetDateTime(JsonElement resource)
    {
        if (!resource.TryGetProperty("onsetDateTime", out JsonElement onsetDateTime))
        {
            return null;
        }

        return onsetDateTime.GetString();
    }
}
