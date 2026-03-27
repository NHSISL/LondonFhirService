// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using LondonFhirService.Core.Models.Processings.ListEntryComparisons;

namespace LondonFhirService.Core.Services.Processings.ListEntryComparisons;

public interface IListEntryComparisonProcessingService
{
    List<DiffItem> CompareListEntryCounts(
      JsonElement source1List,
      JsonElement source2List,
      string listTitle
    );
}
