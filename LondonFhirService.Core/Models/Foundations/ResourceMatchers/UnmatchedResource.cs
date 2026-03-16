// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Text.Json;

namespace LondonFhirService.Core.Models.Foundations.ResourceMatchers
{
    public record UnmatchedResource(
        JsonElement Resource,
        string ResourceType,
        string? Identifier,
        bool IsFromSource1
    );
}
