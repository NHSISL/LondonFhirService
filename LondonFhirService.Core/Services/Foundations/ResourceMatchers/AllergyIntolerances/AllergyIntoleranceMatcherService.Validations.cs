// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers.AllergyIntolerances;

public partial class AllergyIntoleranceMatcherService
{
    private static void ValidateResourceIsNotNull(JsonElement resource, string parameterName = nameof(resource))
    {
        if (resource.ValueKind == JsonValueKind.Undefined ||
            resource.ValueKind == JsonValueKind.Null)
        {
            throw new NullAllergyIntoleranceMatcherException(parameterName);
        }
    }

    private static void ValidateResourceIndexIsNotNull(
        Dictionary<string, JsonElement> resourceIndex,
        string parameterName = nameof(resourceIndex))
    {
        if (resourceIndex is null)
        {
            throw new NullResourceMatcherException(parameterName);
        }
    }

    private static void ValidateResourcesListIsNotNull(
        List<JsonElement> resources,
        string parameterName)
    {
        if (resources is null)
        {
            throw new NullResourceMatcherException(parameterName);
        }
    }
}
