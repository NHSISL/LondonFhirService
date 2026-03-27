// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.JsonElements;

namespace LondonFhirService.Core.Services.Processings.JsonIgnoreRules
{
    public partial class ArrayOrderIgnoreProcessingRule : JsonIgnoreProcessingRuleBase, IJsonIgnoreProcessingRule
    {
        public ArrayOrderIgnoreProcessingRule(JsonElementService jsonElementService, LoggingBroker loggingBroker)
            : base(jsonElementService, loggingBroker)
        { }

        public override ValueTask<bool> ShouldIgnore(JsonElement element, string path) =>
        TryCatch(async () =>
        {
            ValidateOnShouldIgnore(element, path);

            return element.ValueKind == JsonValueKind.Array;
        });

        public override ValueTask<JsonElement> GetReplacement(JsonElement element) =>
        TryCatch(async () =>
        {
            ValidateOnGetReplacement(element);

            if (element.ValueKind != JsonValueKind.Array)
            {
                return element;
            }

            var processedItems = new List<JsonElement>();

            foreach (JsonElement item in element.EnumerateArray())
            {
                JsonElement sortedItem = await SortJsonElement(item);
                processedItems.Add(sortedItem);
            }

            List<JsonElement> sortedArray = processedItems
                .OrderBy(item => GetSortKey(item))
                .ToList();

            return await jsonElementService.CreateArrayElement(sortedArray);
        });

        private async ValueTask<JsonElement> SortJsonElement(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                var properties = element
                    .EnumerateObject()
                    .OrderBy(p => p.Name);

                Dictionary<string, JsonElement> sortedProperties = new();

                foreach (var property in properties)
                {
                    JsonElement sortedValue = await SortJsonElement(property.Value);
                    sortedProperties[property.Name] = sortedValue;
                }

                return await jsonElementService.CreateObjectElement(sortedProperties);
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                List<JsonElement> sortedArray = new();

                foreach (JsonElement item in element.EnumerateArray())
                {
                    JsonElement sortedItem = await SortJsonElement(item);
                    sortedArray.Add(sortedItem);
                }

                return await jsonElementService.CreateArrayElement(sortedArray);
            }

            return element;
        }

        private string GetSortKey(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                var resourceType = element.TryGetProperty("resourceType", out var rt)
                    ? rt.GetString()
                    : "unknown";
                return resourceType ?? "unknown";
            }

            return element.GetRawText();
        }
    }
}
