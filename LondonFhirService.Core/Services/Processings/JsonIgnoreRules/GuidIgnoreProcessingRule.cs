// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.JsonElements;

namespace LondonFhirService.Core.Services.Processings.JsonIgnoreRules
{
    public partial class GuidIgnoreProcessingRule : JsonIgnoreProcessingRuleBase, IJsonIgnoreProcessingRule
    {
        public GuidIgnoreProcessingRule(JsonElementService jsonElementService, LoggingBroker loggingBroker)
            : base(jsonElementService, loggingBroker)
        { }

        public override ValueTask<bool> ShouldIgnoreAsync(JsonElement element, string path) =>
        TryCatch(async () =>
        {
            ValidateOnShouldIgnore(element, path);

            if (element.ValueKind != JsonValueKind.String)
                return false;

            var value = element.GetString();

            return !string.IsNullOrEmpty(value) && GuidPattern.IsMatch(value);
        });

        public override ValueTask<JsonElement> GetReplacementAsync(JsonElement element) =>
        TryCatch(async () =>
        {
            ValidateOnGetReplacement(element);

            return await jsonElementService.CreateStringElement("<GUID>");
        });

        private static readonly Regex GuidPattern = new(
            @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$",
            RegexOptions.Compiled);
    }
}
