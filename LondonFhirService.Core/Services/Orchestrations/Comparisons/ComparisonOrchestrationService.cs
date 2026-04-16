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
using LondonFhirService.Core.Models.Orchestrations.Comparisons;
using LondonFhirService.Core.Models.Processings.ListEntryComparisons;
using LondonFhirService.Core.Services.Foundations.JsonElements;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers;
using LondonFhirService.Core.Services.Processings.JsonIgnoreRules;
using LondonFhirService.Core.Services.Processings.ListEntryComparisons;
using LondonFhirService.Core.Services.Processings.ResourceMatchings;

namespace LondonFhirService.Core.Services.Orchestrations.Comparisons
{
    public partial class ComparisonOrchestrationService : IComparisonOrchestrationService
    {
        private readonly List<IJsonIgnoreProcessingRule> ignoreRules;
        private readonly IResourceMatcherProcessingService resourceMatcherProcessingService;
        private readonly IListEntryComparisonProcessingService listEntryComparisonProcessingService;
        private readonly IJsonElementService jsonElementService;
        private readonly ILoggingBroker loggingBroker;

        public ComparisonOrchestrationService(
            List<IJsonIgnoreProcessingRule> ignoreRules,
            IResourceMatcherProcessingService resourceMatcherProcessingService,
            IListEntryComparisonProcessingService listEntryComparisonProcessingService,
            IJsonElementService jsonElementService,
            ILoggingBroker loggingBroker)
        {
            this.ignoreRules = ignoreRules;
            this.resourceMatcherProcessingService = resourceMatcherProcessingService;
            this.listEntryComparisonProcessingService = listEntryComparisonProcessingService;
            this.jsonElementService = jsonElementService;
            this.loggingBroker = loggingBroker;
        }

        public async ValueTask<ComparisonResult> CompareAsync(
            string correlationId,
            string source1Json,
            string source2Json)
        {
            var diffs = new List<DiffItem>();
            using JsonDocument source1Doc = JsonDocument.Parse(source1Json);
            using JsonDocument source2Doc = JsonDocument.Parse(source2Json);
            JsonElement source1Bundle = source1Doc.RootElement;
            JsonElement source2Bundle = source2Doc.RootElement;

            Dictionary<string, List<JsonElement>> source1Resources =
                ExtractResourcesByType(source1Bundle);

            Dictionary<string, List<JsonElement>> source2Resources =
                ExtractResourcesByType(source2Bundle);

            Dictionary<string, JsonElement> source1ResourceIndex =
                BuildResourceIndex(source1Bundle);

            Dictionary<string, JsonElement> source2ResourceIndex =
                BuildResourceIndex(source2Bundle);

            List<string> allResourceTypes = source1Resources.Keys
                .Union(source2Resources.Keys)
                .ToList();

            foreach (string resourceType in allResourceTypes)
            {
                List<JsonElement> s1Resources =
                    source1Resources.GetValueOrDefault(resourceType, new List<JsonElement>());

                List<JsonElement> s2Resources =
                    source2Resources.GetValueOrDefault(resourceType, new List<JsonElement>());

                IResourceMatcherService? matcher =
                    await resourceMatcherProcessingService.GetMatcherAsync(resourceType);

                if (matcher is null)
                {
                    foreach (JsonElement _ in s1Resources)
                    {
                        diffs.Add(new DiffItem
                        {
                            Type = "manual-review-required",
                            Reason = $"No matching strategy for {resourceType} resources",
                            ResourceType = resourceType
                        });
                    }

                    foreach (JsonElement _ in s2Resources)
                    {
                        diffs.Add(new DiffItem
                        {
                            Type = "manual-review-required",
                            Reason = $"No matching strategy for {resourceType} resources",
                            ResourceType = resourceType
                        });
                    }

                    continue;
                }

                ResourceMatch resourceMatch = await matcher.MatchAsync(
                    s1Resources,
                    s2Resources,
                    source1ResourceIndex,
                    source2ResourceIndex);

                foreach (UnmatchedResource unmatchedResource in resourceMatch.Unmatched)
                {
                    if (unmatchedResource.IsFromSource1)
                    {
                        diffs.Add(new DiffItem
                        {
                            Type = "manual-review-required",
                            Reason = string.IsNullOrEmpty(unmatchedResource.Identifier)
                                ? $"No match key found for {resourceType}"
                                : $"Match key '{unmatchedResource.Identifier}' not found in Source2",
                            ResourceType = resourceType,
                            Identifier = unmatchedResource.Identifier
                        });
                    }
                    else
                    {
                        diffs.Add(new DiffItem
                        {
                            Type = "manual-review-required",
                            Reason = string.IsNullOrEmpty(unmatchedResource.Identifier)
                                ? $"No match key found for {resourceType}"
                                : $"Match key '{unmatchedResource.Identifier}' not found in Source1",
                            ResourceType = resourceType,
                            Identifier = unmatchedResource.Identifier
                        });
                    }
                }

                foreach (MatchedResource match in resourceMatch.Matched)
                {
                    string resourceId =
                        await matcher.GetMatchKeyAsync(match.Source1, source1ResourceIndex)
                        ?? await matcher.GetMatchKeyAsync(match.Source2, source2ResourceIndex);

                    IEnumerable<DiffItem> resourceDiffs = await CompareJsonElements(
                        match.Source1,
                        match.Source2,
                        $"$.{resourceType}[{resourceId}]",
                        resourceType,
                        resourceId);

                    diffs.AddRange(resourceDiffs);

                    if (string.Equals(resourceType, "List", StringComparison.OrdinalIgnoreCase) &&
                        resourceId is not null)
                    {
                        IEnumerable<DiffItem> listEntryDiffs =
                            await listEntryComparisonProcessingService.CompareListEntryCountsAsync(
                                match.Source1,
                                match.Source2,
                                resourceId);

                        diffs.AddRange(listEntryDiffs);
                    }
                }
            }

            return new ComparisonResult
            {
                CorrelationId = correlationId,
                DiffCount = diffs.Count,
                Diffs = diffs
            };
        }

        private Dictionary<string, List<JsonElement>> ExtractResourcesByType(JsonElement bundle)
        {
            var result = new Dictionary<string, List<JsonElement>>(StringComparer.OrdinalIgnoreCase);

            if (!bundle.TryGetProperty("entry", out var entries))
                return result;

            foreach (var entry in entries.EnumerateArray())
            {
                if (!entry.TryGetProperty("resource", out var resource))
                    continue;

                if (!resource.TryGetProperty("resourceType", out var resourceTypeProp))
                    continue;

                var resourceType = resourceTypeProp.GetString();
                if (string.IsNullOrEmpty(resourceType))
                    continue;

                if (!result.ContainsKey(resourceType))
                    result[resourceType] = new List<JsonElement>();

                result[resourceType].Add(resource);
            }

            return result;
        }

        private Dictionary<string, JsonElement> BuildResourceIndex(JsonElement bundle)
        {
            var index = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);

            if (!bundle.TryGetProperty("entry", out var entries))
                return index;

            foreach (var entry in entries.EnumerateArray())
            {
                if (!entry.TryGetProperty("resource", out var resource))
                    continue;

                if (!resource.TryGetProperty("resourceType", out var resourceTypeProp))
                    continue;

                if (!resource.TryGetProperty("id", out var idProp))
                    continue;

                var resourceType = resourceTypeProp.GetString();
                var id = idProp.GetString();
                if (string.IsNullOrEmpty(resourceType) || string.IsNullOrEmpty(id))
                    continue;

                index[$"{resourceType}/{id}"] = resource;
            }

            return index;
        }

        private async ValueTask<List<DiffItem>> CompareJsonElements(
            JsonElement source1, JsonElement source2, string path, string resourceType, string? resourceId)
        {
            var diffs = new List<DiffItem>();
            var normalized1 = await ApplyIgnoreRules(source1, path);
            var normalized2 = await ApplyIgnoreRules(source2, path);
            CompareElements(normalized1, normalized2, path, diffs, resourceType, resourceId);

            return diffs;
        }

        private async ValueTask<JsonElement> ApplyIgnoreRules(JsonElement element, string path)
        {
            foreach (var rule in ignoreRules)
            {
                if (await rule.ShouldIgnoreAsync(element, path))
                {
                    return await rule.GetReplacementAsync(element);
                }
            }

            if (element.ValueKind == JsonValueKind.Object)
            {
                var properties = new Dictionary<string, JsonElement>();

                foreach (JsonProperty prop in element.EnumerateObject())
                {
                    properties[prop.Name] =
                        await ApplyIgnoreRules(prop.Value, $"{path}.{prop.Name}");
                }

                return await jsonElementService.CreateObjectElement(properties);
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                var elements = new List<JsonElement>();
                int index = 0;

                foreach (JsonElement item in element.EnumerateArray())
                {
                    JsonElement processedElement =
                        await ApplyIgnoreRules(item, $"{path}[{index}]");

                    elements.Add(processedElement);
                    index++;
                }

                return await jsonElementService.CreateArrayElement(elements);
            }

            return element;
        }

        private void CompareElements(
            JsonElement source1,
            JsonElement source2,
            string path,
            List<DiffItem> diffs,
            string resourceType,
            string? resourceId)
        {
            if (source1.ValueKind != source2.ValueKind)
            {
                diffs.Add(new DiffItem
                {
                    Type = "modified",
                    Path = path,
                    OldValue = source1.GetRawText(),
                    NewValue = source2.GetRawText(),
                    ResourceType = resourceType,
                    Identifier = resourceId
                });

                return;
            }

            if (source1.ValueKind == JsonValueKind.Object)
            {
                var allKeys = source1.EnumerateObject().Select(p => p.Name)
                    .Union(source2.EnumerateObject().Select(p => p.Name))
                    .Distinct();

                foreach (var key in allKeys)
                {
                    var hasS1 = source1.TryGetProperty(key, out var s1Prop);
                    var hasS2 = source2.TryGetProperty(key, out var s2Prop);

                    if (hasS1 && hasS2)
                    {
                        CompareElements(s1Prop, s2Prop, $"{path}.{key}", diffs, resourceType, resourceId);
                    }
                    else if (hasS1)
                    {
                        diffs.Add(new DiffItem
                        {
                            Type = "removed",
                            Path = $"{path}.{key}",
                            OldValue = s1Prop.GetRawText(),
                            ResourceType = resourceType,
                            Identifier = resourceId
                        });
                    }
                    else if (hasS2)
                    {
                        diffs.Add(new DiffItem
                        {
                            Type = "added",
                            Path = $"{path}.{key}",
                            NewValue = s2Prop.GetRawText(),
                            ResourceType = resourceType,
                            Identifier = resourceId
                        });
                    }
                }
                return;
            }

            if (source1.ValueKind == JsonValueKind.Array)
            {
                var s1Array = source1.EnumerateArray().ToList();
                var s2Array = source2.EnumerateArray().ToList();

                var maxLength = Math.Max(s1Array.Count, s2Array.Count);
                for (int i = 0; i < maxLength; i++)
                {
                    if (i < s1Array.Count && i < s2Array.Count)
                    {
                        CompareElements(s1Array[i], s2Array[i], $"{path}[{i}]", diffs, resourceType, resourceId);
                    }
                    else if (i < s1Array.Count)
                    {
                        diffs.Add(new DiffItem
                        {
                            Type = "removed",
                            Path = $"{path}[{i}]",
                            OldValue = s1Array[i].GetRawText(),
                            ResourceType = resourceType,
                            Identifier = resourceId
                        });
                    }
                    else
                    {
                        diffs.Add(new DiffItem
                        {
                            Type = "added",
                            Path = $"{path}[{i}]",
                            NewValue = s2Array[i].GetRawText(),
                            ResourceType = resourceType,
                            Identifier = resourceId
                        });
                    }
                }
                return;
            }

            if (source1.ValueKind == JsonValueKind.String)
            {
                var s1Value = source1.GetString();
                var s2Value = source2.GetString();

                if (s1Value != s2Value)
                {
                    diffs.Add(new DiffItem
                    {
                        Type = "modified",
                        Path = path,
                        OldValue = s1Value,
                        NewValue = s2Value,
                        ResourceType = resourceType,
                        Identifier = resourceId
                    });
                }
                return;
            }

            if (source1.GetRawText() != source2.GetRawText())
            {
                diffs.Add(new DiffItem
                {
                    Type = "modified",
                    Path = path,
                    OldValue = source1.GetRawText(),
                    NewValue = source2.GetRawText(),
                    ResourceType = resourceType,
                    Identifier = resourceId
                });
            }
        }
    }
}
