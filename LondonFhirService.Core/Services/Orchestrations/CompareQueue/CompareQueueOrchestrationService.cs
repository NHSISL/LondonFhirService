// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue;
using LondonFhirService.Core.Services.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Services.Foundations.FhirRecords;

namespace LondonFhirService.Core.Services.Orchestrations.CompareQueue
{
    internal partial class CompareQueueOrchestrationService : ICompareQueueOrchestrationService
    {
        /// <summary>How long a secondary waits for its sibling primary to land before it is compared.</summary>
        private const int CompareBufferMinutes = 5;

        /// <summary>
        /// How long a claim is honoured before another worker may take the row back. Comfortably
        /// longer than a comparison takes, so a live worker is never overtaken.
        /// </summary>
        private const int ProcessingLeaseMinutes = 30;

        /// <summary>
        /// How many candidates to try before giving up this cycle when other workers keep winning
        /// the claim. Bounds the contention loop; the next tick retries either way.
        /// </summary>
        private const int MaxClaimAttempts = 3;

        private readonly IFhirRecordService fhirRecordService;
        private readonly IFhirRecordDifferenceService fhirRecordDifferenceService;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ILoggingBroker loggingBroker;

        public CompareQueueOrchestrationService(
            IFhirRecordService fhirRecordService,
            IFhirRecordDifferenceService fhirRecordDifferenceService,
            IDateTimeBroker dateTimeBroker,
            ILoggingBroker loggingBroker)
        {
            this.fhirRecordService = fhirRecordService;
            this.fhirRecordDifferenceService = fhirRecordDifferenceService;
            this.dateTimeBroker = dateTimeBroker;
            this.loggingBroker = loggingBroker;
        }

        /// <summary>
        /// The buffer gives a secondary's sibling primary time to land before the pair is
        /// compared, and it counts from InsertedDate - stamped by the database when the row became
        /// visible - rather than from UpdatedDate, which the request thread stamps before the
        /// insert is even queued.
        ///
        /// Processing rows older than the lease are picked up again. Processing used to be a
        /// write-only state with no reader, so a process recycle or a failed status write between
        /// the claim and the terminal status left the row invisible forever and its comparison
        /// silently never performed.
        /// </summary>
        public ValueTask<CompareQueueItem> GetUnprocessedRecordAsync() =>
            TryCatch(async () =>
            {
                DateTimeOffset currentDateTime =
                    await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

                DateTimeOffset bufferedDateTime =
                    currentDateTime.AddMinutes(-CompareBufferMinutes);

                DateTimeOffset leaseExpiryDateTime =
                    currentDateTime.AddMinutes(-ProcessingLeaseMinutes);

                FhirRecord secondaryFhirRecord = null;

                // Losing a claim means another worker took that row, not that the queue is empty,
                // so the next candidate is tried rather than returning null - the caller uses null
                // as its drain-loop terminator, and stopping there would end the cycle early and
                // leave a ready backlog sitting until the next tick. Bounded so a pathological
                // run of contention cannot spin.
                for (int attempt = 0; attempt < MaxClaimAttempts; attempt++)
                {
                    IQueryable<FhirRecord> secondaryFhirRecordQueryable =
                        await this.fhirRecordService.RetrieveAllFhirRecordsAsync();

                    FhirRecord candidateFhirRecord = secondaryFhirRecordQueryable
                        .Where(fhirRecord =>
                            !fhirRecord.IsPrimarySource
                            && ((fhirRecord.Status == StatusType.Pending
                                    && fhirRecord.InsertedDate <= bufferedDateTime)
                                || (fhirRecord.Status == StatusType.Processing
                                    && fhirRecord.UpdatedDate <= leaseExpiryDateTime)))
                        .OrderBy(fhirRecord => fhirRecord.CreatedDate)
                        .FirstOrDefault();

                    if (candidateFhirRecord == null)
                    {
                        return null;
                    }

                    // Claimed by the database, not by a read-then-write. Two workers - a
                    // scale-out, or the overlap of a rolling deployment - could both read the same
                    // row and both succeed at writing it back, comparing the same pair twice and
                    // persisting two difference rows for it.
                    //
                    // The lease bound goes into the statement for the reclaim arm, where the row
                    // moves Processing to Processing and a status-only guard would match for
                    // every competing worker.
                    bool claimed = await this.fhirRecordService.TryClaimFhirRecordAsync(
                        candidateFhirRecord.Id,
                        expectedStatus: candidateFhirRecord.Status,
                        claimedStatus: StatusType.Processing,

                        notUpdatedAfter: candidateFhirRecord.Status == StatusType.Processing
                            ? leaseExpiryDateTime
                            : null);

                    if (claimed)
                    {
                        secondaryFhirRecord = candidateFhirRecord;

                        break;
                    }
                }

                if (secondaryFhirRecord == null)
                {
                    await this.loggingBroker.LogWarningAsync(
                        $"Gave up claiming a compare-queue record after {MaxClaimAttempts} " +
                            "attempt(s); another worker won each one. The backlog is unchanged " +
                            "and the next cycle will retry.");

                    return null;
                }

                secondaryFhirRecord.Status = StatusType.Processing;

                IQueryable<FhirRecord> primaryFhirRecordQueryable =
                    await this.fhirRecordService.RetrieveAllFhirRecordsAsync();

                primaryFhirRecordQueryable = primaryFhirRecordQueryable
                    .Where(fhirRecord =>
                        fhirRecord.CorrelationId == secondaryFhirRecord.CorrelationId
                        && fhirRecord.IsPrimarySource);

                FhirRecord primaryFhirRecord = primaryFhirRecordQueryable.FirstOrDefault();

                var compareQueueItem = new CompareQueueItem();
                compareQueueItem.PrimaryFhirRecord = primaryFhirRecord;
                compareQueueItem.SecondaryFhirRecord = secondaryFhirRecord;

                return compareQueueItem;
            });

        public ValueTask ChangeFhirRecordStatusAsync(Guid fhirRecordId, StatusType status) =>
            TryCatch(async () =>
            {
                ValidateChangeFhirRecordStatus(fhirRecordId);

                FhirRecord maybeFhirRecord =
                    await this.fhirRecordService.RetrieveFhirRecordByIdAsync(fhirRecordId);

                maybeFhirRecord.Status = status;

                if (status == StatusType.Completed || status == StatusType.Failed)
                {
                    maybeFhirRecord.IsProcessed = true;
                }

                await this.fhirRecordService.ModifyFhirRecordAsync(maybeFhirRecord);
            });

        public ValueTask PersistFhirRecordDifferencesAsync(CompareQueueItem compareQueueItem) =>
            TryCatch(async () =>
            {
                ValidatePersistFhirRecordDifferences(compareQueueItem);

                await this.fhirRecordDifferenceService
                    .AddFhirRecordDifferenceAsync(compareQueueItem.FhirRecordDifference);
            });
    }
}
