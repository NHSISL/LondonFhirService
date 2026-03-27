// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace LondonFhirService.Core.Services.Foundations.JsonElements
{
    public interface IJsonElementService
    {
        ValueTask<JsonElement> CreateStringElement(string value);
        ValueTask<JsonElement> CreateArrayElement(List<JsonElement> elements);
        ValueTask<JsonElement> CreateObjectElement(Dictionary<string, JsonElement> properties);
    }
}