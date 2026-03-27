// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Text.Json;
using System.Threading.Tasks;

namespace LondonFhirService.Core.Services.Processings.JsonIgnoreRules
{
    public interface IJsonIgnoreProcessingRule
    {
        ValueTask<bool> ShouldIgnore(JsonElement element, string path);
        ValueTask<JsonElement> GetReplacement(JsonElement element);
    }
}
