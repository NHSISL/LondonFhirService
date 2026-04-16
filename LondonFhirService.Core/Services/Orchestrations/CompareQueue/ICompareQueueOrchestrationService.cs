// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue;

namespace LondonFhirService.Core.Services.Orchestrations.Accesses
{
    public interface ICompareQueueOrchestrationService
    {
        ValueTask<CompareQueueItems> GetUnprocessedRecordAsync();
        ValueTask MarkFhirRecordsAsync(Guid fhirRecordId);
        ValueTask PersistFhirRecordDifferencesAsync(CompareQueueItems compareQueueItems);
    }
}
