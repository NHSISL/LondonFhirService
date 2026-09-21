// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;
using LondonFhirService.Core.Services.Foundations.JsonElements;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers.Lists
{
    /// <summary>
    /// Matches Lists as groupings rather than as resources.
    ///
    /// How many List resources a provider uses to express one heading is a serialisation choice,
    /// not a clinical fact: one returns a single "Active Allergies" list, another splits the same
    /// allergies across two. Matching resource to resource made that choice significant - two
    /// lists sharing a title collided on the match key, the first won, and the rest were reported
    /// as manual-review-required against a reason that read "not found in Source2" when the truth
    /// was "this heading appears twice on this side".
    ///
    /// So every List under one heading is merged into a single grouping and the groupings are
    /// matched instead. A provider that splits and a provider that does not now compare equal,
    /// and what is left to disagree about is the membership - which allergies are filed under
    /// that heading - which is the clinical question.
    ///
    /// The merged grouping deliberately drops <c>date</c>. It is the timestamp of when a source
    /// assembled its list, so a merge of two lists has no honest value to carry, and keeping an
    /// arbitrary member's would differ between any two independent retrievals and report as a
    /// difference every time.
    ///
    /// Grouping keys on the title and the SNOMED code together, so "Active Allergies" and a
    /// resolved-allergies list are never folded into one another even if a provider titles them
    /// alike. The key exposed on the match stays the bare title, because that is what the
    /// management portal joins a difference back to its section on.
    /// </summary>
    internal partial class ListMatcherService : ResourceMatcherServiceBase, IResourceMatcherService
    {
        private readonly IJsonElementService jsonElementService;
        private const string SnomedSystem = "http://snomed.info/sct";

        public ListMatcherService(IJsonElementService jsonElementService, ILoggingBroker loggingBroker)
            : base(loggingBroker)
        {
            this.jsonElementService = jsonElementService;
        }

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

            Dictionary<string, ListGrouping> source1ByKey =
                await GroupAndMergeListsAsync(source1Resources, source1ResourceIndex);

            Dictionary<string, ListGrouping> source2ByKey =
                await GroupAndMergeListsAsync(source2Resources, source2ResourceIndex);

            List<string> allKeys = source1ByKey.Keys.Union(source2ByKey.Keys).ToList();

            foreach (string key in allKeys)
            {
                bool hasSource1 = source1ByKey.TryGetValue(key, out ListGrouping source1Grouping);
                bool hasSource2 = source2ByKey.TryGetValue(key, out ListGrouping source2Grouping);
                string title = hasSource1 ? source1Grouping.Title : source2Grouping.Title;

                if (hasSource1 && hasSource2)
                {
                    resourceMatch.Matched.Add(
                        new MatchedResource(source1Grouping.Merged, source2Grouping.Merged, title));
                }
                else if (hasSource1)
                {
                    resourceMatch.Unmatched.Add(
                        new UnmatchedResource(source1Grouping.Merged, ResourceType, title, true));
                }
                else
                {
                    resourceMatch.Unmatched.Add(
                        new UnmatchedResource(source2Grouping.Merged, ResourceType, title, false));
                }
            }

            return resourceMatch;
        });

        private async ValueTask<Dictionary<string, ListGrouping>> GroupAndMergeListsAsync(
            List<JsonElement> resources,
            Dictionary<string, JsonElement> resourceIndex)
        {
            var groupings = new Dictionary<string, ListGrouping>();

            // GroupBy keeps first-appearance order, so the template a grouping is built from is
            // the first list the bundle carried for that heading rather than an arbitrary one.
            var groups = resources
                .Select(resource => new
                {
                    Resource = resource,
                    Title = InternalGetMatchKey(resource, resourceIndex),
                    GroupKey = InternalGetGroupKey(resource, resourceIndex)
                })
                .Where(keyedResource => keyedResource.GroupKey != null)
                .GroupBy(keyedResource => keyedResource.GroupKey)
                .ToList();

            foreach (var group in groups)
            {
                List<JsonElement> groupedLists = group
                    .Select(keyedResource => keyedResource.Resource)
                    .ToList();

                groupings[group.Key] = new ListGrouping(
                    title: group.First().Title,
                    merged: await MergeListGroupAsync(groupedLists));
            }

            return groupings;
        }

        private async ValueTask<JsonElement> MergeListGroupAsync(List<JsonElement> groupedLists)
        {
            var properties = new Dictionary<string, JsonElement>();

            foreach (JsonProperty property in groupedLists[0].EnumerateObject())
            {
                if (property.Name == "entry" || property.Name == "date")
                {
                    continue;
                }

                properties[property.Name] = property.Value;
            }

            var entries = new List<JsonElement>();

            foreach (JsonElement groupedList in groupedLists)
            {
                if (groupedList.TryGetProperty("entry", out JsonElement entry)
                    && entry.ValueKind == JsonValueKind.Array)
                {
                    entries.AddRange(entry.EnumerateArray());
                }
            }

            // Always written, even when no list in the grouping had one, so a heading present on
            // both sides is shaped the same way on both and an absent entry array cannot read as
            // a difference in its own right. The count still differs when the membership does.
            properties["entry"] = await jsonElementService.CreateArrayElement(entries);

            return await jsonElementService.CreateObjectElement(properties);
        }

        internal virtual string InternalGetGroupKey(
            JsonElement resource,
            Dictionary<string, JsonElement> resourceIndex)
        {
            string title = InternalGetMatchKey(resource, resourceIndex);

            if (title is null)
            {
                return null;
            }

            // Unit separator: a title is free text and a SNOMED code is digits, so anything a
            // provider is likely to type cannot fabricate a collision between two headings.
            return $"{title}{GetSnomedCode(resource)}";
        }

        private static string GetSnomedCode(JsonElement resource)
        {
            if (!resource.TryGetProperty("code", out JsonElement code)
                || code.ValueKind != JsonValueKind.Object)
            {
                return string.Empty;
            }

            if (!code.TryGetProperty("coding", out JsonElement codings)
                || codings.ValueKind != JsonValueKind.Array)
            {
                return string.Empty;
            }

            foreach (JsonElement coding in codings.EnumerateArray())
            {
                if (coding.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                if (coding.TryGetProperty("system", out JsonElement system)
                    && system.ValueKind == JsonValueKind.String
                    && system.GetString() == SnomedSystem
                    && coding.TryGetProperty("code", out JsonElement codingCode)
                    && codingCode.ValueKind == JsonValueKind.String)
                {
                    return codingCode.GetString() ?? string.Empty;
                }
            }

            return string.Empty;
        }

        internal virtual string InternalGetMatchKey(JsonElement resource, Dictionary<string, JsonElement> resourceIndex)
        {
            if (!resource.TryGetProperty("title", out var title))
                return null;

            return title.GetString();
        }

        private readonly struct ListGrouping
        {
            public ListGrouping(string title, JsonElement merged)
            {
                Title = title;
                Merged = merged;
            }

            public string Title { get; }
            public JsonElement Merged { get; }
        }
    }
}
