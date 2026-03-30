// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers
{
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading.Tasks;
    using LondonFhirService.Core.Brokers.Loggings;
    using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

    public abstract partial class ResourceMatcherServiceBase : IResourceMatcherService
    {
        protected readonly ILoggingBroker loggingBroker;

        protected ResourceMatcherServiceBase(ILoggingBroker loggingBroker)
        {
            this.loggingBroker = loggingBroker;
        }

        public abstract string ResourceType { get; }

        public abstract ValueTask<string> GetMatchKeyAsync(
            JsonElement resource,
            Dictionary<string, JsonElement> resourceIndex);

        public abstract ValueTask<ResourceMatch> MatchAsync(
            List<JsonElement> source1Resources,
            List<JsonElement> source2Resources,
            Dictionary<string, JsonElement> source1ResourceIndex,
            Dictionary<string, JsonElement> source2ResourceIndex);
    }
}
