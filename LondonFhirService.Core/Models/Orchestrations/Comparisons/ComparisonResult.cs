// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json.Serialization;
using LondonFhirService.Core.Models.Processings.ListEntryComparisons;

namespace LondonFhirService.Core.Models.Orchestrations.Comparisons
{
    public class ComparisonResult
    {
        [JsonPropertyName("correlationId")]
        public string CorrelationId { get; set; } = string.Empty;

        [JsonPropertyName("diffCount")]
        public int DiffCount { get; set; }

        [JsonPropertyName("diffs")]
        public List<DiffItem> Diffs { get; set; } = new();
    }
}
