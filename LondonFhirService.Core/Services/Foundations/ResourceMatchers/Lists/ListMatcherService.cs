// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers.Lists
{
    public partial class ListMatcherService : ResourceMatcherServiceBase, IResourceMatcherService
    {
        public ListMatcherService(ILoggingBroker loggingBroker)
            : base(loggingBroker)
        { }

        public override string ResourceType => "List";

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

            Dictionary<string, List<JsonElement>> source1ByKey = source1Resources
                .Select(resource =>
                    new { Resource = resource, Key = InternalGetMatchKey(resource, source1ResourceIndex) })
                .Where(x => x.Key != null)
                .GroupBy(x => x.Key!)
                .ToDictionary(group => group.Key, group => group.Select(x => x.Resource).ToList());

            Dictionary<string, List<JsonElement>> source2ByKey = source2Resources
                .Select(resource =>
                    new { Resource = resource, Key = InternalGetMatchKey(resource, source2ResourceIndex) })
                .Where(x => x.Key != null)
                .GroupBy(x => x.Key!)
                .ToDictionary(group => group.Key, group => group.Select(x => x.Resource).ToList());

            List<string> allKeys = source1ByKey.Keys.Union(source2ByKey.Keys).ToList();

            foreach (string key in allKeys)
            {
                List<JsonElement> source1Group =
                    source1ByKey.TryGetValue(key, out List<JsonElement> s1) ? s1 : new List<JsonElement>();

                List<JsonElement> source2Group =
                    source2ByKey.TryGetValue(key, out List<JsonElement> s2) ? s2 : new List<JsonElement>();

                int maxCount = Math.Max(source1Group.Count, source2Group.Count);

                for (int index = 0; index < maxCount; index++)
                {
                    bool hasSource1 = index < source1Group.Count;
                    bool hasSource2 = index < source2Group.Count;

                    if (hasSource1 && hasSource2)
                    {
                        resourceMatch.Matched.Add(
                            new MatchedResource(source1Group[index], source2Group[index], key));
                    }
                    else if (hasSource1)
                    {
                        resourceMatch.Unmatched.Add(
                            new UnmatchedResource(source1Group[index], ResourceType, key, IsFromSource1: true));
                    }
                    else
                    {
                        resourceMatch.Unmatched.Add(
                            new UnmatchedResource(source2Group[index], ResourceType, key, IsFromSource1: false));
                    }
                }
            }

            return resourceMatch;
        });

        internal virtual string InternalGetMatchKey(JsonElement resource, Dictionary<string, JsonElement> resourceIndex)
        {
            if (!resource.TryGetProperty("title", out var title))
                return null;

            return title.GetString();
        }
    }
}
