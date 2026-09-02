// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Text.Json.Serialization;

namespace LondonFhirService.Core.Models.Processings.ListEntryComparisons
{
    public class DiffItem
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        [JsonPropertyName("oldValue")]
        public string OldValue { get; set; }

        [JsonPropertyName("newValue")]
        public string NewValue { get; set; }

        [JsonPropertyName("resourceType")]
        public string ResourceType { get; set; }

        [JsonPropertyName("identifier")]
        public string Identifier { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        /// <summary>
        /// Whether an administrator has reviewed this difference and judged it acceptable - the
        /// two providers disagree, but not in a way that matters. Always false when the comparison
        /// engine first writes it: acceptance is a human decision made afterwards, in the
        /// management portal, which sets the flag here and republishes the whole
        /// FhirRecordDifference.DiffJson.
        ///
        /// It lives on the difference rather than in a table of its own because a difference has
        /// no identity outside the comparison that produced it: re-running a comparison rewrites
        /// the whole result, and an acceptance that outlived the difference it was about would be
        /// worse than none. FhirRecordDifference.AcceptableDiffCount is the count of the
        /// differences flagged here, kept in step whenever this is written so a list can be
        /// triaged without reading every stored result.
        /// </summary>
        [JsonPropertyName("acceptableDiff")]
        public bool AcceptableDiff { get; set; }
    }
}
