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
                    .SetProperty(fhirRecord => fhirRecord.UpdatedDate, claimedDate),
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
