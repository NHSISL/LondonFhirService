// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.JsonElements;

namespace LondonFhirService.Core.Services.Processings.JsonIgnoreRules
{
    /// <summary>
    /// Masks the GUID out of a FHIR relative reference - "Organization/92f740a2-88ad-4e76-b948-
    /// 9583b85ecb15" becomes "Organization/&lt;GUID&gt;" - so two providers describing the same
    /// record do not differ on every reference they carry.
    ///
    /// The GUIDs in these bundles are assigned per request rather than stored, so the same
    /// resource comes back under a different one on every call and on each side of a comparison.
    /// Left alone they drown the result: on the comparison that prompted this rule, 83 of 111
    /// differences were a reference whose only disagreement was the GUID.
    ///
    /// GuidIgnoreProcessingRule does not cover these because it anchors on a bare GUID, and a
    /// reference is a type prefix plus a GUID.
    ///
    /// The resource type prefix is deliberately kept rather than blanking the whole value: a
    /// reference that points at an Organization on one side and a Practitioner on the other is a
    /// real difference and still reports as one. What this does give up is same-type,
    /// different-instance - an Encounter pointing at Practitioner A against one pointing at
    /// Practitioner B no longer differs here. That is a real loss, accepted on the grounds that
    /// every referenced resource is also compared in its own right under its own matcher and
    /// business key, so a genuinely different referent still surfaces against that resource. The
    /// wiring between them is what stops being checked. Resolving the reference through the
    /// source resource index and comparing the target's match key would keep it, and is the
    /// upgrade path if the wiring ever needs to be covered.
    ///
    /// Only the relative "Type/id" form is handled, which is what the STU3 bundles use. An
    /// absolute URL or a "urn:uuid:" reference would fall through to a raw difference.
    /// </summary>
    internal partial class ReferenceIgnoreProcessingRule : JsonIgnoreProcessingRuleBase, IJsonIgnoreProcessingRule
    {
        public ReferenceIgnoreProcessingRule(IJsonElementService jsonElementService, ILoggingBroker loggingBroker)
            : base(jsonElementService, loggingBroker)
        { }

        public override ValueTask<bool> ShouldIgnoreAsync(JsonElement element, string path) =>
        TryCatch(async () =>
        {
            ValidateOnShouldIgnore(element, path);

            if (element.ValueKind != JsonValueKind.String)
                return false;

            // Both the path and the value have to agree before anything is masked. The value
            // shape alone would catch any string that happens to read like "Word/<guid>"
            // wherever it appeared; requiring the FHIR element that holds a reference keeps this
            // to the field it was written for.
            if (path.EndsWith(".reference") == false)
                return false;

            var value = element.GetString();

            return !string.IsNullOrEmpty(value) && ReferencePattern.IsMatch(value);
        });

        public override ValueTask<JsonElement> GetReplacementAsync(JsonElement element) =>
        TryCatch(async () =>
        {
            ValidateOnGetReplacement(element);

            string value = element.ValueKind == JsonValueKind.String
                ? element.GetString()
                : null;

            Match referenceMatch = value is null
                ? Match.Empty
                : ReferencePattern.Match(value);

            // ShouldIgnoreAsync has already established the shape for every call the comparison
            // makes, but this is public: an element that does not match is handed back untouched
            // rather than replaced with a placeholder that would erase a real value.
            if (referenceMatch.Success == false)
            {
                return element;
            }

            string resourceType = referenceMatch.Groups["resourceType"].Value;

            return await jsonElementService.CreateStringElement($"{resourceType}/<GUID>");
        });

        private static readonly Regex ReferencePattern = new(
            @"^(?<resourceType>[A-Za-z]+)/" +
            @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$",
            RegexOptions.Compiled);
    }
}
