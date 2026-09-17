// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using Microsoft.EntityFrameworkCore;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial class StorageBroker
    {
        public DbSet<FhirRecord> FhirRecords { get; set; }

        public async ValueTask<FhirRecord> InsertFhirRecordAsync(
            FhirRecord fhirRecord,
            CancellationToken cancellationToken = default) =>
            await InsertAsync(fhirRecord, cancellationToken);

        public async ValueTask<IQueryable<FhirRecord>> SelectAllFhirRecordsAsync(
            CancellationToken cancellationToken = default) =>
            await SelectAllAsync<FhirRecord>(cancellationToken);

        public async ValueTask<int> ClaimFhirRecordAsync(
            Guid fhirRecordId,
            StatusType expectedStatus,
            StatusType claimedStatus,
            DateTimeOffset claimedDate,
            string claimedBy,
            DateTimeOffset? notUpdatedAfter,
            CancellationToken cancellationToken = default) =>
            await this.FhirRecords
                .Where(fhirRecord =>
                    fhirRecord.Id == fhirRecordId
                        && fhirRecord.Status == expectedStatus

                        // Carries the lease into the statement. Reclaiming a stranded row moves
                        // it from Processing to Processing, so the status check alone is vacuous
                        // there and two workers would both match and both "win". Requiring the
                        // row to still be as stale as the reader saw it means the first claim
                        // bumps UpdatedDate and the second matches nothing.
                        && (notUpdatedAfter == null || fhirRecord.UpdatedDate <= notUpdatedAfter))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(fhirRecord => fhirRecord.Status, claimedStatus)
                    .SetProperty(fhirRecord => fhirRecord.UpdatedDate, claimedDate)

                    // FhirRecord is IAuditable, and ExecuteUpdateAsync goes round the change
                    // tracker - so nothing stamps the actor unless the statement does. Moving
                    // UpdatedDate while leaving UpdatedBy behind produces a row that says it was
                    // touched, at a time, by whoever last touched it through a different path.
                    .SetProperty(fhirRecord => fhirRecord.UpdatedBy, claimedBy),
                    cancellationToken);

        public async ValueTask<int> UpdateFhirRecordStatusAsync(
            Guid fhirRecordId,
            StatusType excludedStatus,
            StatusType newStatus,
            bool isProcessed,
            DateTimeOffset updatedDate,
            string updatedBy,
            CancellationToken cancellationToken = default) =>
            await this.FhirRecords
                .Where(fhirRecord =>
                    fhirRecord.Id == fhirRecordId
                        && fhirRecord.Status != excludedStatus)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(fhirRecord => fhirRecord.Status, newStatus)

                    // Set here too, because the read-then-write path this replaced set it for
                    // terminal statuses. Dropping it left every completed primary marked
                    // unprocessed, which anything reading IsProcessed would read as still pending.
                    // Passed in rather than derived from newStatus: deciding which statuses are
                    // terminal is the caller's business, not the broker's.
                    .SetProperty(fhirRecord => fhirRecord.IsProcessed, isProcessed)
                    .SetProperty(fhirRecord => fhirRecord.UpdatedDate, updatedDate)

                    // Same reason as the claim above, and here it is a restoration rather than a
                    // precaution: the read-then-write path this replaced ran through
                    // ApplyModifyAuditValuesAsync, so completing a primary used to stamp the
                    // actor. Dropping it would have left the previous actor on a row whose
                    // UpdatedDate had moved.
                    .SetProperty(fhirRecord => fhirRecord.UpdatedBy, updatedBy),
                    cancellationToken);

        public async ValueTask<FhirRecord> SelectFhirRecordByIdAsync(
            Guid fhirRecordId,
            CancellationToken cancellationToken = default) =>
            await SelectAsync<FhirRecord>(new object[] { fhirRecordId }, cancellationToken);

        public async ValueTask<FhirRecord> UpdateFhirRecordAsync(
            FhirRecord fhirRecord,
            CancellationToken cancellationToken = default) =>
            await UpdateAsync(fhirRecord, cancellationToken);

        public async ValueTask<FhirRecord> DeleteFhirRecordAsync(
            FhirRecord fhirRecord,
            CancellationToken cancellationToken = default) =>
            await DeleteAsync(fhirRecord, cancellationToken);
    }
}
