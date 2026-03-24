// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using LondonFhirService.Core.Services.Foundations.AllergyIntolerances;

namespace LondonFhirService.Core.Services.Processings.ResourceMatchings;

public class AllergyIntoleranceProcessingService : IAllergyIntoleranceProcessingService
{
    private readonly Dictionary<string, IAllergyIntoleranceService> matchers;

    public AllergyIntoleranceProcessingService(IEnumerable<IAllergyIntoleranceService> matchers)
    {
        this.matchers = matchers.ToDictionary(
            matcher => matcher.ResourceType,
            matcher => matcher,
            StringComparer.OrdinalIgnoreCase);
    }

    public IAllergyIntoleranceService? GetMatcher(string resourceType)
    {
        return matchers.TryGetValue(resourceType, out var matcher) ? matcher : null;
    }

    public bool HasMatcher(string resourceType)
    {
        return matchers.ContainsKey(resourceType);
    }
}
