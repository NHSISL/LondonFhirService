// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using LondonFhirService.Core.Models.Processings.ListEntryComparisons;

namespace LondonFhirService.Core.Services.Processings.ListEntryComparisons;

public class ListEntryComparisonProcessingService : IListEntryComparisonProcessingService
{
    public List<DiffItem> CompareListEntryCounts(
      JsonElement source1List,
      JsonElement source2List,
      string listTitle
    )
    {
        var diffs = new List<DiffItem>();

        var source1Count = GetEntryCount(source1List);
        var source2Count = GetEntryCount(source2List);

        if (source1Count != source2Count)
        {
            diffs.Add(
              new DiffItem
              {
                  Type = "entry-count-mismatch",
                  Path = $"$.List[{listTitle}].entry",
                  ResourceType = "List",
                  Identifier = listTitle,
                  OldValue = source1Count.ToString(),
                  NewValue = source2Count.ToString(),
                  Reason = $"List '{listTitle}' has {source1Count} entries in Source1 but {source2Count} entries in Source2"
              }
            );
        }

        return diffs;
    }

    private int GetEntryCount(JsonElement listResource)
    {
        if (!listResource.TryGetProperty("entry", out var entryElement))
        {
            return 0;
        }

        if (entryElement.ValueKind != JsonValueKind.Array)
        {
            return 0;
        }

        return entryElement.GetArrayLength();
    }
}
