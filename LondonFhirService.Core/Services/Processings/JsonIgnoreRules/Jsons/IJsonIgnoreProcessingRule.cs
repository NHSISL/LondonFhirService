// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Text.Json;
using System.Threading.Tasks;

namespace LondonFhirService.Core.Services.Processings.JsonIgnoreRules.Jsons
{
    public interface IJsonIgnoreProcessingRule
    {
        ValueTask<bool> ShouldIgnoreAsync(JsonElement element, string path);
        ValueTask<JsonElement> GetReplacementAsync(JsonElement element);
    }
}
