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
    internal partial class PractitionerMatcherService : ResourceMatcherServiceBase, IResourceMatcherService
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
                .ToDictionaryFirstWins(
                    keyedResource => keyedResource.Key!,
                    keyedResource => keyedResource.Resource,
                    onDuplicate: (key, resource) => resourceMatch.Unmatched.Add(
                        new UnmatchedResource(resource, ResourceType, key, true)));

            var source2ByKey = source2Resources
                .Select(resource => new
                {
                    Resource = resource,
                    Key = InternalGetMatchKey(resource, source2ResourceIndex)
                })
                .Where(keyedResource => keyedResource.Key != null)
                .ToDictionaryFirstWins(
                    keyedResource => keyedResource.Key!,
                    keyedResource => keyedResource.Resource,
                    onDuplicate: (key, resource) => resourceMatch.Unmatched.Add(
                        new UnmatchedResource(resource, ResourceType, key, false)));

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

        /// <summary>
        /// The SDS user id when the practitioner carries one, and the practice-qualified DDS
        /// identifier when it does not.
        ///
        /// The fallback exists because the SDS id is not reliably populated: feeds have been seen
        /// carrying the identifier entry with its system and no value at all. Keyed on SDS alone
        /// those practitioners produced no key, and a resource with no key is dropped before
        /// matching - not paired, not reported as unmatched, simply absent from the comparison.
        /// Every practitioner in a record could go uncompared and the result would still read
        /// clean.
        ///
        /// SDS is tried first because it is a national identifier and so means the same thing to
        /// both providers, where a DDS identifier is local to the feed that issued it. The two key
        /// spaces cannot collide: the DDS form is always prefixed with its practice and a
        /// separator, which an SDS id never contains.
        /// </summary>
        internal virtual string InternalGetMatchKey(JsonElement resource, Dictionary<string, JsonElement> resourceIndex)
        {
            string sdsUserId = GetSdsUserId(resource);

            if (!string.IsNullOrWhiteSpace(sdsUserId))
            {
                return sdsUserId;
            }

            return PracticeQualifiedDdsMatchKey.Build(resource);
        }

        private static string GetSdsUserId(JsonElement resource)
        {
            if (!resource.TryGetProperty("identifier", out JsonElement identifiers)
                || identifiers.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            foreach (JsonElement identifierElement in identifiers.EnumerateArray())
            {
                if (identifierElement.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                if (identifierElement.TryGetProperty("system", out JsonElement system)
                    && system.ValueKind == JsonValueKind.String
                    && system.GetString() == SdsUserIdSystem
                    && identifierElement.TryGetProperty("value", out JsonElement value)
                    && value.ValueKind == JsonValueKind.String)
                {
                    return value.GetString();
                }
            }

            return null;
        }
    }
}
