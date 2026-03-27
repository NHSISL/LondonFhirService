// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers.MedicationStatements
{
    public partial class MedicationStatementMatcherService : ResourceMatcherServiceBase, IResourceMatcherService
    {
        public MedicationStatementMatcherService(LoggingBroker loggingBroker)
            : base(loggingBroker)
        { }

        public override string ResourceType => "MedicationStatement";
        private const string SnomedSystem = "http://snomed.info/sct";

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

        private static string? GetDateAsserted(JsonElement resource)
        {
            if (!resource.TryGetProperty("dateAsserted", out var dateAsserted))
                return null;

            return dateAsserted.GetString();
        }

        private static string? GetMedicationSnomedCode(JsonElement resource, Dictionary<string, JsonElement> resourceIndex)
        {
            if (resource.TryGetProperty("medicationReference", out var medicationReference)
                && medicationReference.TryGetProperty("reference", out var referenceProp))
            {
                var reference = referenceProp.GetString();
                if (!string.IsNullOrWhiteSpace(reference) && resourceIndex.TryGetValue(reference, out var medication))
                {
                    var code = ExtractSnomedCodeFromCode(medication);
                    if (!string.IsNullOrWhiteSpace(code))
                    {
                        return code;
                    }
                }
            }

            if (resource.TryGetProperty("medicationCodeableConcept", out var medicationCodeableConcept))
            {
                var code = ExtractSnomedCodeFromCodeableConcept(medicationCodeableConcept);
                if (!string.IsNullOrWhiteSpace(code))
                {
                    return code;
                }
            }

            return null;
        }

        private static string? ExtractSnomedCodeFromCode(JsonElement resource)
        {
            if (!resource.TryGetProperty("code", out var code))
                return null;

            return ExtractSnomedCodeFromCodeableConcept(code);
        }

        private static string? ExtractSnomedCodeFromCodeableConcept(JsonElement codeableConcept)
        {
            if (!codeableConcept.TryGetProperty("coding", out var coding))
                return null;

            foreach (var codingElement in coding.EnumerateArray())
            {
                if (!codingElement.TryGetProperty("system", out var system))
                    continue;

                var systemValue = system.GetString();
                if (systemValue == SnomedSystem)
                {
                    if (codingElement.TryGetProperty("code", out var codeValue))
                    {
                        return codeValue.GetString();
                    }
                }
            }

            return null;
        }

        internal virtual string InternalGetMatchKey(JsonElement resource, Dictionary<string, JsonElement> resourceIndex)
        {
            var snomedCode = GetMedicationSnomedCode(resource, resourceIndex);
            var dateAsserted = GetDateAsserted(resource);

            if (string.IsNullOrWhiteSpace(snomedCode) || string.IsNullOrWhiteSpace(dateAsserted))
            {
                return null;
            }

            return $"{snomedCode}|{dateAsserted}";
        }
    }
}
