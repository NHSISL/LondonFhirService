// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.JsonElements;

namespace LondonFhirService.Core.Services.Processings.JsonIgnoreRules
{
    public partial class IdIgnoreProcessingRule : JsonIgnoreProcessingRuleBase, IJsonIgnoreProcessingRule
    {
        public IdIgnoreProcessingRule(JsonElementService jsonElementService, LoggingBroker loggingBroker)
            : base(jsonElementService, loggingBroker)
        { }

        public override ValueTask<bool> ShouldIgnoreAsync(JsonElement element, string path) =>
        TryCatch(async () =>
        {
            ValidateOnShouldIgnore(element, path);

            if (element.ValueKind != JsonValueKind.String)
                return false;

            var pathParts = path.Split('.');
            var lastPart = pathParts.LastOrDefault();

            return lastPart == "id" || path.EndsWith(".id");
        });

        public override ValueTask<JsonElement> GetReplacementAsync(JsonElement element) =>
        TryCatch(async () =>
        {
            ValidateOnGetReplacement(element);

            return await jsonElementService.CreateStringElement("<id>");
        });
    }
}
