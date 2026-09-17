// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue;

namespace LondonFhirService.Core.Services.Orchestrations.CompareQueue
{
    public interface ICompareQueueOrchestrationService
    {
        ValueTask<CompareQueueItem> GetUnprocessedRecordAsync();
        ValueTask ChangeFhirRecordStatusAsync(Guid fhirRecordId, StatusType status);

        /// <summary>
        /// Completes the primary record of a pair, unless it already is. The primary is shared by
        /// every secondary of the same correlation, so with more than one provider several workers
        /// reach this for the same row; the transition is conditional in the database rather than
        /// guarded by a read, which is a check by one worker and a write by another.
        /// </summary>
        ValueTask CompletePrimaryFhirRecordAsync(Guid fhirRecordId);

        /// <summary>
        /// Re-asserts the caller's claim on the secondary record and extends the lease. False
        /// means another worker reclaimed the row because the lease expired mid-flight, and the
        /// caller must abandon its result rather than persist a second one for the same pair.
        /// </summary>
        ValueTask<bool> TryRetainClaimAsync(CompareQueueItem compareQueueItem);
        ValueTask PersistFhirRecordDifferencesAsync(CompareQueueItem compareQueueItem);
    }
}
