// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Core.Services.Processings.JsonIgnoreRules
{
    using System.Collections.Generic;
    using System.Text.Json;
    using LondonFhirService.Core.Models.Foundations.ResourceMatchers;
    using LondonFhirService.Core.Services.Foundations.ResourceMatchers;

    public abstract partial class ResourceMatcherServiceBase : IResourceMatcherService
    {
        public abstract string ResourceType { get; }

        public abstract string GetMatchKey(
            JsonElement resource,
            Dictionary<string, JsonElement> resourceIndex);

        public abstract ResourceMatch Match(
            List<JsonElement> source1Resources,
            List<JsonElement> source2Resources,
            Dictionary<string, JsonElement> source1ResourceIndex,
            Dictionary<string, JsonElement> source2ResourceIndex);
    }
}
