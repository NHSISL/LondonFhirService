// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue;

namespace LondonFhirService.Core.Services.Orchestrations.Accesses
{
    public interface ICompareQueueOrchestrationService
    {
        ValueTask<CompareQueueItem> GetUnprocessedRecordAsync();
        ValueTask ChangeFhirRecordStatusAsync(Guid fhirRecordId, StatusType status);
        ValueTask PersistFhirRecordDifferencesAsync(CompareQueueItem compareQueueItems);
    }
}
