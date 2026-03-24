// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using LondonFhirService.Core.Models.Foundations.AllergyIntolerances.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.AllergyIntolerances.AllergyIntolerances;

public partial class AllergyIntoleranceMatcherService
{
    private static void ValidateGetMatchKeyArguments(
        JsonElement resource,
        Dictionary<string, JsonElement> resourceIndex)
    {
        Validate(
            () => new InvalidAllergyIntoleranceServiceException(),
            (Rule: IsInvalid(resource), Parameter: nameof(resource)),
            (Rule: IsInvalid(resourceIndex), Parameter: nameof(resourceIndex)));
    }

    private static void ValidateMatchArguments(
        List<JsonElement> source1Resources,
        List<JsonElement> source2Resources,
        Dictionary<string, JsonElement> source1ResourceIndex,
        Dictionary<string, JsonElement> source2ResourceIndex)
    {
        Validate(
            () => new InvalidAllergyIntoleranceServiceException(),
            (Rule: IsInvalid(source1Resources), Parameter: nameof(source1Resources)),
            (Rule: IsInvalid(source2Resources), Parameter: nameof(source2Resources)),
            (Rule: IsInvalid(source1ResourceIndex), Parameter: nameof(source1ResourceIndex)),
            (Rule: IsInvalid(source2ResourceIndex), Parameter: nameof(source2ResourceIndex)));
    }

    private static dynamic IsInvalid(JsonElement resource) => new
    {
        Condition = resource.ValueKind == JsonValueKind.Undefined ||
                    resource.ValueKind == JsonValueKind.Null,
        Message = "Value is required"
    };

    private static dynamic IsInvalid(List<JsonElement> resources) => new
    {
        Condition = resources is null,
        Message = "Value is required"
    };

    private static dynamic IsInvalid(Dictionary<string, JsonElement> resourceIndex) => new
    {
        Condition = resourceIndex is null,
        Message = "Value is required"
    };

    private static void Validate<T>(
        Func<T> createException,
        params (dynamic Rule, string Parameter)[] validations)
        where T : Xeption
    {
        T invalidDataException = createException();

        foreach ((dynamic rule, string parameter) in validations)
        {
            if (rule.Condition)
            {
                invalidDataException.UpsertDataList(
                    key: parameter,
                    value: rule.Message);
            }
        }

        invalidDataException.ThrowIfContainsErrors();
    }
}