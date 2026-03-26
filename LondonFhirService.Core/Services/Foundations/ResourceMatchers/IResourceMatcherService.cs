// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using LondonFhirService.Core.Models.Foundations.AllergyIntolerances;

namespace LondonFhirService.Core.Services.Foundations.AllergyIntolerances
{
    public interface IResourceMatcherService
    {
        string ResourceType { get; }
        string? GetMatchKey(JsonElement resource, Dictionary<string, JsonElement> resourceIndex);

        ResourceMatch Match(
            List<JsonElement> source1Resources,
            List<JsonElement> source2Resources,
            Dictionary<string, JsonElement> source1ResourceIndex,
            Dictionary<string, JsonElement> source2ResourceIndex);
    }
}
