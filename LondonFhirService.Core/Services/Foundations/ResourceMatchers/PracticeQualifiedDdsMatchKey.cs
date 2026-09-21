// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Text.Json;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers
{
    /// <summary>
    /// The DDS identifier qualified by the practice that issued it.
    ///
    /// A DDS identifier is unique within one practice's feed, not within a bundle. A patient
    /// registered at more than one practice gets a record built from both, and both number their
    /// resources from the same sequence - so a record spanning two practices carries two
    /// unrelated resources both claiming DDS id '54'.
    ///
    /// Keyed on the bare identifier those collide, and the matcher does the safe thing with a
    /// collision: keeps the first and sets the rest aside as manual-review-required. Safe, but
    /// wrong - and the reason it writes reads "not found in Source2" when both sides hold the
    /// resource perfectly well. Worth being clear about what the alternative would have been: had
    /// it matched them, it would have compared one practice's sublingual temperature against the
    /// other's tympanic one and reported confident, meaningless differences.
    ///
    /// Qualifying with the ODS code off meta.tag separates them, so each practice's '54' pairs
    /// with its own counterpart.
    ///
    /// A resource with no ODS tag keys on an empty prefix rather than being dropped. That leaves
    /// untagged resources grouped exactly as they were before this existed - no better, no worse -
    /// rather than inventing a distinction the data does not carry.
    /// </summary>
    internal static class PracticeQualifiedDdsMatchKey
    {
        private const string DdsIdentifierSystem = "https://fhir.hl7.org.uk/Id/dds";
        private const string OdsCodeSystem = "https://fhir.nhs.uk/Id/ODS-Code";

        public static string Build(JsonElement resource)
        {
            string ddsIdentifier = GetDdsIdentifier(resource);

            // No DDS identifier means no key at all, which is what every one of these matchers
            // did before and what the portal mirrors when it decides a resource has no row of
            // its own to hang a difference on.
            if (string.IsNullOrWhiteSpace(ddsIdentifier))
            {
                return null;
            }

            return $"{GetOdsCode(resource)}|{ddsIdentifier}";
        }

        private static string GetDdsIdentifier(JsonElement resource)
        {
            if (!resource.TryGetProperty("identifier", out JsonElement identifiers)
                || identifiers.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            foreach (JsonElement identifier in identifiers.EnumerateArray())
            {
                if (identifier.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                if (identifier.TryGetProperty("system", out JsonElement system)
                    && system.ValueKind == JsonValueKind.String
                    && system.GetString() == DdsIdentifierSystem
                    && identifier.TryGetProperty("value", out JsonElement value)
                    && value.ValueKind == JsonValueKind.String)
                {
                    return value.GetString();
                }
            }

            return null;
        }

        private static string GetOdsCode(JsonElement resource)
        {
            if (!resource.TryGetProperty("meta", out JsonElement meta)
                || meta.ValueKind != JsonValueKind.Object)
            {
                return string.Empty;
            }

            if (!meta.TryGetProperty("tag", out JsonElement tags)
                || tags.ValueKind != JsonValueKind.Array)
            {
                return string.Empty;
            }

            foreach (JsonElement tag in tags.EnumerateArray())
            {
                if (tag.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                if (tag.TryGetProperty("system", out JsonElement system)
                    && system.ValueKind == JsonValueKind.String
                    && system.GetString() == OdsCodeSystem
                    && tag.TryGetProperty("code", out JsonElement code)
                    && code.ValueKind == JsonValueKind.String)
                {
                    return code.GetString() ?? string.Empty;
                }
            }

            return string.Empty;
        }
    }
}
