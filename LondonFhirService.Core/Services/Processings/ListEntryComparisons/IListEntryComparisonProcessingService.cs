// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Processings.ListEntryComparisons;

namespace LondonFhirService.Core.Services.Processings.ListEntryComparisons;

public interface IListEntryComparisonProcessingService
{
    ValueTask<List<DiffItem>> CompareListEntryCountsAsync(
        JsonElement source1List,
        JsonElement source2List,
        string listTitle);
}
