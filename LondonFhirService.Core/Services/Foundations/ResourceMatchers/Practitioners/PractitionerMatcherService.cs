// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers.Practitioners
{
    public partial class PractitionerMatcherService : ResourceMatcherServiceBase, IResourceMatcherService
    {
        public PractitionerMatcherService(ILoggingBroker loggingBroker)
            : base(loggingBroker)
        { }

        public override string ResourceType => "Practitioner";
        private const string SdsUserIdSystem = "https://fhir.nhs.uk/Id/sds-user-id";

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
                .Select(resource => new
                {
                    Resource = resource,
                    Key = InternalGetMatchKey(resource, source1ResourceIndex)
                })
                .Where(keyedResource => keyedResource.Key != null)
                .ToDictionary(
                    keyedResource => keyedResource.Key!,
                    keyedResource => keyedResource.Resource);

            var source2ByKey = source2Resources
                .Select(resource => new
                {
                    Resource = resource,
                    Key = InternalGetMatchKey(resource, source2ResourceIndex)
                })
                .Where(keyedResource => keyedResource.Key != null)
                .ToDictionary(
                    keyedResource => keyedResource.Key!,
                    keyedResource => keyedResource.Resource);

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
                if (systemValue == SdsUserIdSystem)
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
