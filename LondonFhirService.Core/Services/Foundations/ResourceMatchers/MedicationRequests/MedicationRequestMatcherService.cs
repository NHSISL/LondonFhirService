// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers.MedicationRequests
{
    public partial class MedicationRequestMatcherService : ResourceMatcherServiceBase, IResourceMatcherService
    {
        public MedicationRequestMatcherService(ILoggingBroker loggingBroker)
            : base(loggingBroker)
        { }

        public override string ResourceType => "MedicationRequest";
        private const string DdsIdentifierSystem = "https://fhir.hl7.org.uk/Id/dds";

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
            var resourceMatch = new ResourceMatch();

            var source1ByKey = source1Resources
                .Select(r => new { Resource = r, Key = InternalGetMatchKey(r, source1ResourceIndex) })
                .Where(x => x.Key != null)
                .ToDictionary(x => x.Key!, x => x.Resource);

            var source2ByKey = source2Resources
                .Select(r => new { Resource = r, Key = InternalGetMatchKey(r, source2ResourceIndex) })
                .Where(x => x.Key != null)
                .ToDictionary(x => x.Key!, x => x.Resource);

            var allKeys = source1ByKey.Keys.Union(source2ByKey.Keys).ToList();

            foreach (var key in allKeys)
            {
                var hasSource1 = source1ByKey.TryGetValue(key, out var source1Resource);
                var hasSource2 = source2ByKey.TryGetValue(key, out var source2Resource);

                if (hasSource1 && hasSource2)
                {
                    resourceMatch.Matched.Add(new MatchedResource(source1Resource!, source2Resource!, key));
                }
                else if (hasSource1)
                {
                    resourceMatch.Unmatched.Add(new UnmatchedResource(source1Resource!, ResourceType, key, true));
                }
                else if (hasSource2)
                {
                    resourceMatch.Unmatched.Add(new UnmatchedResource(source2Resource!, ResourceType, key, false));
                }
            }

            return resourceMatch;
        });

        internal virtual string InternalGetMatchKey(JsonElement resource, Dictionary<string, JsonElement> resourceIndex)
        {
            if (!resource.TryGetProperty("identifier", out var identifiers))
                return null;

            foreach (var identifierElement in identifiers.EnumerateArray())
            {
                if (!identifierElement.TryGetProperty("system", out var system))
                    continue;

                var systemValue = system.GetString();
                if (systemValue == DdsIdentifierSystem)
                {
                    if (identifierElement.TryGetProperty("value", out var value))
                    {
                        return value.GetString();
                    }
                }
            }

            return null;
        }
    }
}
