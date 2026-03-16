// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Services.Processings.ResourceMatchings;

public class ResourceMatcherProcessingService : IResourceMatcherProcessingService
{
    private readonly Dictionary<string, IResourceMatcherService> matchers;

    public ResourceMatcherProcessingService(IEnumerable<IResourceMatcherService> matchers)
    {
        this.matchers = matchers.ToDictionary(
            matcher => matcher.ResourceType,
            matcher => matcher,
            StringComparer.OrdinalIgnoreCase);
    }

    public IResourceMatcherService? GetMatcher(string resourceType)
    {
        return matchers.TryGetValue(resourceType, out var matcher) ? matcher : null;
    }

    public bool HasMatcher(string resourceType)
    {
        return matchers.ContainsKey(resourceType);
    }
}
