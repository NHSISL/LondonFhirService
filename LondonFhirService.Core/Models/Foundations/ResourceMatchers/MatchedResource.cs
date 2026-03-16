// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Text.Json;

namespace LondonFhirService.Core.Models.Foundations.ResourceMatchers
{
    public record MatchedResource(
        JsonElement Source1,
        JsonElement Source2,
        string MatchKey
    );
}
