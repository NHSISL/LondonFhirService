// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Core.Services.Processings.JsonIgnoreRules
{
    using System.Text.Json;
    using System.Threading.Tasks;
    using LondonFhirService.Core.Brokers.Loggings;
    using LondonFhirService.Core.Services.Foundations.JsonElements;
    using LondonFhirService.Core.Services.Processings.JsonIgnoreRules.Jsons;

    public abstract partial class JsonIgnoreProcessingRuleBase : IJsonIgnoreProcessingRule
    {
        protected readonly JsonElementService jsonElementService;
        protected readonly ILoggingBroker loggingBroker;

        protected JsonIgnoreProcessingRuleBase(
            JsonElementService jsonElementService,
            ILoggingBroker loggingBroker)
        {
            this.jsonElementService = jsonElementService;
            this.loggingBroker = loggingBroker;
        }

        public abstract ValueTask<bool> ShouldIgnoreAsync(JsonElement element, string path);

        public abstract ValueTask<JsonElement> GetReplacementAsync(JsonElement element);
    }
}
