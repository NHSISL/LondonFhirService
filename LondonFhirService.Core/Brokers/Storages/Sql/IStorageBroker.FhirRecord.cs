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
        /// The queue has five operations - claim, reclaim an expired lease, renew, settle and
        /// abandon - and four of them are this one statement: a compare-and-swap on identity,
        /// status and the lease token. Settling was the exception, written as a read-then-write
        /// behind a separate ownership check, which is how a worker could pass the check, lose
        /// the lease, and then write a terminal status over the row's new holder. It goes through
        /// here now, which is what <paramref name="isProcessed"/> is for.
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
            string claimedBy,
            bool isProcessed,
            DateTimeOffset? notUpdatedAfter,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Moves a record to a status unless it is already in it, in one statement. Used for the
        /// primary record, which is shared by every secondary of the same correlation and so is
        /// completed by whichever worker finishes first - a read-then-write there is a check by
        /// one worker and a write by another.
        /// </summary>
        ValueTask<int> UpdateFhirRecordStatusAsync(
            Guid fhirRecordId,
            StatusType excludedStatus,
            StatusType newStatus,
            bool isProcessed,
            DateTimeOffset updatedDate,
            string updatedBy,
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
