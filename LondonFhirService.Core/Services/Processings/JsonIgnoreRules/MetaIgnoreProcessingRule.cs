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
    public partial class MetaIgnoreProcessingRule : JsonIgnoreProcessingRuleBase, IJsonIgnoreProcessingRule
    {
        public MetaIgnoreProcessingRule(JsonElementService jsonElementService, LoggingBroker loggingBroker)
            : base(jsonElementService, loggingBroker)
        { }

        public override ValueTask<bool> ShouldIgnore(JsonElement element, string path) =>
        TryCatch(async () =>
        {
            var pathParts = path.Split('.');
            var lastPart = pathParts.LastOrDefault();

            return lastPart == "meta" || path.EndsWith(".meta");
        });

        public override ValueTask<JsonElement> GetReplacement(JsonElement element) =>
        TryCatch(async () =>
        {
            return await jsonElementService.CreateStringElement("<meta-ignored>");
        });
    }
}
