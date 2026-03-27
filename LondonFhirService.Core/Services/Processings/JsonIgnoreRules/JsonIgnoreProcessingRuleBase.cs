// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Core.Services.Processings.JsonIgnoreRules
{
    using System.Text.Json;
    using System.Threading.Tasks;
    using LondonFhirService.Core.Brokers.Loggings;
    using LondonFhirService.Core.Services.Foundations.JsonElements;

    public abstract partial class JsonIgnoreProcessingRuleBase : IJsonIgnoreProcessingRule
    {
        protected readonly JsonElementService jsonElementService;
        protected readonly LoggingBroker loggingBroker;

        protected JsonIgnoreProcessingRuleBase(
            JsonElementService jsonElementService,
            LoggingBroker loggingBroker)
        {
            this.jsonElementService = jsonElementService;
            this.loggingBroker = loggingBroker;
        }

        public abstract ValueTask<bool> ShouldIgnore(JsonElement element, string path);

        public abstract ValueTask<JsonElement> GetReplacement(JsonElement element);
    }
}
