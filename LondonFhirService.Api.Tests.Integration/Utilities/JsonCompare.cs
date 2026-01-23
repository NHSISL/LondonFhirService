// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Text.Json;
using System.Text.Json.JsonDiffPatch;
using System.Text.Json.JsonDiffPatch.Diffs.Formatters;
using System.Text.Json.Nodes;

namespace LondonFhirService.Api.Tests.Integration.Utilities
{
    public static class JsonCompare
    {
        public static string GetPatch(string jsonA, string jsonB)
        {
            var a = JsonNode.Parse(jsonA)!;
            var b = JsonNode.Parse(jsonB)!;

            // RFC6902 operations: add / remove / replace
            var patch = a.Diff(b, new JsonPatchDeltaFormatter());

            return patch is null
                ? string.Empty
                : patch.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }
    }

}
