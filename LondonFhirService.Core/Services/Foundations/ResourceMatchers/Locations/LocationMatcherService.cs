// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers.Locations
{
    public partial class LocationMatcherService : ResourceMatcherServiceBase, IResourceMatcherService
    {
        public LocationMatcherService(ILoggingBroker loggingBroker)
            : base(loggingBroker)
        { }

        public override string ResourceType => "Location";
        private const string OdsSiteCodeSystem = "https://fhir.nhs.uk/Id/ods-site-code";

        public override ValueTask<string> GetMatchKeyAsync(
            JsonElement resource, Dictionary<string, JsonElement> resourceIndex) =>
        TryCatch(async () =>
        {
            ValidateOnGetMatchKeyArguments(resource, resourceIndex);

            return InternalGetMatchKey(resource, resourceIndex);
        });

        public override ValueTask<ResourceMatch> MatchAsync(
            List<JsonElement> source1Resources,
            List<JsonElement> source2Resources,
            Dictionary<string, JsonElement> source1ResourceIndex,
            Dictionary<string, JsonElement> source2ResourceIndex) =>
        TryCatch(async () =>
        {
            ValidateOnMatchArguments(source1Resources, source2Resources, source1ResourceIndex, source2ResourceIndex);

            return new ResourceMatch();
        });

        internal virtual string InternalGetMatchKey(JsonElement resource, Dictionary<string, JsonElement> resourceIndex)
        {
            return null;
        }
    }
}
