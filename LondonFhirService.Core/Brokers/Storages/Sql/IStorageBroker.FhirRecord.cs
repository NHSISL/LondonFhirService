// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirRecords;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial interface IStorageBroker
    {
        ValueTask<FhirRecord> InsertFhirRecordAsync(
            FhirRecord fhirRecord,
            CancellationToken cancellationToken = default);

        ValueTask<IQueryable<FhirRecord>> SelectAllFhirRecordsAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Moves one record from an expected status to a new one in a single statement, returning
        /// the number of rows it actually changed. The compare queue used to claim a row by
        /// reading it and writing it back, which two workers could both win; this lets the
        /// database decide, so a caller that gets back zero knows somebody else took it.
        ///
        /// <paramref name="notUpdatedAfter"/> additionally requires the row to be no newer than
        /// the caller saw it. Reclaiming a stranded row keeps the same status on both sides of the
        /// move, which would make the status check alone match for every competing worker.
        /// </summary>
        ValueTask<int> ClaimFhirRecordAsync(
            Guid fhirRecordId,
            StatusType expectedStatus,
            StatusType claimedStatus,
            DateTimeOffset claimedDate,
            DateTimeOffset? notUpdatedAfter,
            CancellationToken cancellationToken = default);

        ValueTask<FhirRecord> SelectFhirRecordByIdAsync(
            Guid fhirRecordId,
            CancellationToken cancellationToken = default);

        ValueTask<FhirRecord> UpdateFhirRecordAsync(
            FhirRecord fhirRecord,
            CancellationToken cancellationToken = default);

        ValueTask<FhirRecord> DeleteFhirRecordAsync(
            FhirRecord fhirRecord,
            CancellationToken cancellationToken = default);
    }
}
