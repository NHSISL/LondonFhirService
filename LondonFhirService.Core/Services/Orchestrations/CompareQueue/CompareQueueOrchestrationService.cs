// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Identifiers;
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

        /// <summary>
        /// How many of the oldest eligible rows a worker chooses from, rather than always taking
        /// the single oldest.
        ///
        /// Every worker runs the same ordered query, so taking the head meant N workers converged
        /// on the identical row every cycle and N-1 of them lost the claim by construction -
        /// contention was guaranteed rather than incidental, and with more workers than
        /// MaxClaimAttempts a worker could lose every attempt and idle a whole tick with a backlog
        /// waiting. Picking within a small window of the oldest rows spreads them out while
        /// keeping the queue approximately first-in-first-out.
        /// </summary>
        private const int ClaimCandidateWindow = 10;

        private readonly IFhirRecordService fhirRecordService;
        private readonly IFhirRecordDifferenceService fhirRecordDifferenceService;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly IIdentifierBroker identifierBroker;
        private readonly ILoggingBroker loggingBroker;

        public CompareQueueOrchestrationService(
            IFhirRecordService fhirRecordService,
            IFhirRecordDifferenceService fhirRecordDifferenceService,
            IDateTimeBroker dateTimeBroker,
            IIdentifierBroker identifierBroker,
            ILoggingBroker loggingBroker)
        {
            this.fhirRecordService = fhirRecordService;
            this.fhirRecordDifferenceService = fhirRecordDifferenceService;
            this.dateTimeBroker = dateTimeBroker;
            this.identifierBroker = identifierBroker;
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

                Guid? claimedFhirRecordId = null;

                // Losing a claim means another worker took that row, not that the queue is empty,
                // so the next candidate is tried rather than returning null - the caller uses null
                // as its drain-loop terminator, and stopping there would end the cycle early and
                // leave a ready backlog sitting until the next tick. Bounded so a pathological
                // run of contention cannot spin.
                for (int attempt = 0; attempt < MaxClaimAttempts; attempt++)
                {
                    IQueryable<FhirRecord> secondaryFhirRecordQueryable =
                        await this.fhirRecordService.RetrieveAllFhirRecordsAsync();

                    // Projected, not materialised whole. Every row in this window carries a
                    // JsonPayload holding an entire FHIR bundle, and the claim needs two columns
                    // out of it - so pulling ClaimCandidateWindow full rows every tick, on every
                    // worker, moved megabytes to decide one identifier. The winner is read in
                    // full below, once.
                    List<ClaimCandidate> claimCandidates = secondaryFhirRecordQueryable
                        .Where(fhirRecord =>
                            !fhirRecord.IsPrimarySource
                            && ((fhirRecord.Status == StatusType.Pending
                                    && fhirRecord.InsertedDate <= bufferedDateTime)
                                || (fhirRecord.Status == StatusType.Processing
                                    && fhirRecord.UpdatedDate <= leaseExpiryDateTime)))
                        .OrderBy(fhirRecord => fhirRecord.CreatedDate)
                        .Take(ClaimCandidateWindow)
                        .Select(fhirRecord => new ClaimCandidate
                        {
                            Id = fhirRecord.Id,
                            Status = fhirRecord.Status
                        })
                        .ToList();

                    if (claimCandidates.Count == 0)
                    {
                        return null;
                    }

                    ClaimCandidate claimCandidate =
                        await SelectCandidateAsync(claimCandidates);

                    // Claimed by the database, not by a read-then-write. Two workers - a
                    // scale-out, or the overlap of a rolling deployment - could both read the same
                    // row and both succeed at writing it back, comparing the same pair twice and
                    // persisting two difference rows for it.
                    //
                    // The lease bound goes into the statement for the reclaim arm, where the row
                    // moves Processing to Processing and a status-only guard would match for
                    // every competing worker.
                    bool claimed = await this.fhirRecordService.TryClaimFhirRecordAsync(
                        claimCandidate.Id,
                        expectedStatus: claimCandidate.Status,
                        claimedStatus: StatusType.Processing,
                        claimedDate: currentDateTime,
                        isProcessed: false,

                        notUpdatedAfter: claimCandidate.Status == StatusType.Processing
                            ? leaseExpiryDateTime
                            : null);

                    if (claimed)
                    {
                        claimedFhirRecordId = claimCandidate.Id;

                        break;
                    }
                }

                if (claimedFhirRecordId is null)
                {
                    await this.loggingBroker.LogWarningAsync(
                        $"Gave up claiming a compare-queue record after {MaxClaimAttempts} " +
                            "attempt(s); another worker won each one. The backlog is unchanged " +
                            "and the next cycle will retry.");

                    return null;
                }

                // Read after the claim rather than before it, so the payload is fetched once and
                // only for the row this worker actually holds - and it reads back as Processing
                // because the claim wrote that, rather than needing the status patched in memory.
                FhirRecord secondaryFhirRecord =
                    await this.fhirRecordService.RetrieveFhirRecordByIdAsync(
                        claimedFhirRecordId.Value);

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
                compareQueueItem.ClaimedAt = currentDateTime;

                return compareQueueItem;
            });

        /// <summary>
        /// One of the window at random, so concurrent workers do not all reach for the same row.
        /// The spread comes from the identifier broker rather than System.Random: the source of
        /// non-determinism stays behind a broker, which is what makes the choice substitutable in
        /// a test, and a fresh identifier per attempt means a worker that loses a race does not
        /// deterministically collide with the same rival on the retry.
        /// </summary>
        private async ValueTask<ClaimCandidate> SelectCandidateAsync(List<ClaimCandidate> claimCandidates)
        {
            Guid selectionIdentifier = await this.identifierBroker.GetIdentifierAsync();
            int selectionSeed = selectionIdentifier.ToByteArray()[0];

            return claimCandidates[selectionSeed % claimCandidates.Count];
        }

        /// <summary>
        /// The two columns the claim statement needs, and nothing else. Status comes along because
        /// it decides which arm of the claim runs - a Pending row is taken, a Processing one is
        /// reclaimed past its lease - so reading it keeps the guard in the statement matching the
        /// row this worker actually chose.
        /// </summary>
        private sealed class ClaimCandidate
        {
            public Guid Id { get; set; }

            public StatusType Status { get; set; }
        }

        /// <summary>
        /// Re-asserts this worker's claim and, in the same statement, extends the lease. False
        /// means another worker has taken the row back - the lease expired while this worker was
        /// still going, which reclaim cannot tell apart from a worker that died.
        ///
        /// A worker that has lost the row must not go on to persist its result: the worker that
        /// took it over is producing the same comparison, and two difference rows for one pair is
        /// worse than one produced slightly later.
        /// </summary>
        public ValueTask<bool> TryRetainClaimAsync(CompareQueueItem compareQueueItem) =>
            TryCatch(async () =>
            {
                ValidateCompareQueueItemOnRetainClaim(compareQueueItem);

                DateTimeOffset currentDateTime =
                    await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

                bool retained = await this.fhirRecordService.TryClaimFhirRecordAsync(
                    compareQueueItem.SecondaryFhirRecord.Id,
                    expectedStatus: StatusType.Processing,
                    claimedStatus: StatusType.Processing,
                    claimedDate: currentDateTime,
                    isProcessed: false,
                    notUpdatedAfter: compareQueueItem.ClaimedAt);

                // The token has to move with the lease. This call rewrote UpdatedDate, so the
                // value the item is carrying is now older than the row and a SECOND re-assertion
                // would match nothing and report the claim lost when it is still held. Only
                // advanced on success: a failed re-assertion did not write, so the row still
                // carries whatever the worker that took it over put there.
                if (retained)
                {
                    compareQueueItem.ClaimedAt = currentDateTime;
                }

                return retained;
            });

        public ValueTask CompletePrimaryFhirRecordAsync(Guid fhirRecordId) =>
            TryCatch(async () =>
            {
                ValidateChangeFhirRecordStatus(fhirRecordId);

                bool transitioned = await this.fhirRecordService.TryTransitionFhirRecordStatusAsync(
                    fhirRecordId,
                    excludedStatus: StatusType.Completed,
                    newStatus: StatusType.Completed,

                    // Completed is terminal, and the read-then-write path this replaced marked
                    // terminal rows processed. Keeping it in step matters because the queue's own
                    // reporting reads IsProcessed rather than Status.
                    isProcessed: true);

                // Debug rather than warning, and not an exception, because the ordinary reason for
                // no rows is the one this method exists to tolerate: a primary is shared by every
                // secondary of its correlation, so the second worker to finish finds it already
                // Completed. The count cannot tell that apart from a row that is not there at all,
                // which would be a real fault - so the line is recorded, quietly, for the case
                // where a primary turns out to be missing.
                if (transitioned is false)
                {
                    await this.loggingBroker.LogDebugAsync(
                        $"Completing primary FhirRecordId: {fhirRecordId} changed no rows. " +
                            "Either a sibling secondary completed it first, which is expected, " +
                            "or the row is gone.");
                }
            });

        /// <summary>
        /// One statement, which is the whole point. This replaced a read-then-write that ran
        /// behind a separate TryRetainClaimAsync check: the check could pass, the lease expire,
        /// another worker reclaim the row, and this write then put a terminal status on it -
        /// burying a comparison that worker was still performing, because nothing reclaims
        /// Completed or Failed. Carrying the lease token into the statement removes the window
        /// rather than narrowing it.
        ///
        /// It also stops dragging the whole row through memory. The read-then-write loaded and
        /// rewrote every column, including a JsonPayload holding an entire FHIR bundle, to change
        /// two fields.
        ///
        /// ClaimedAt is deliberately not advanced on success, unlike TryRetainClaimAsync: the row
        /// has left Processing, so no later re-assertion against it can or should match.
        /// </summary>
        public ValueTask<bool> TryFinalizeClaimedFhirRecordAsync(
            CompareQueueItem compareQueueItem,
            StatusType terminalStatus) =>
            TryCatch(async () =>
            {
                ValidateFinalizeClaimedFhirRecord(compareQueueItem, terminalStatus);

                DateTimeOffset currentDateTime =
                    await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

                return await this.fhirRecordService.TryClaimFhirRecordAsync(
                    compareQueueItem.SecondaryFhirRecord.Id,
                    expectedStatus: StatusType.Processing,
                    claimedStatus: terminalStatus,
                    claimedDate: currentDateTime,

                    // Terminal rows are processed rows. The read-then-write this replaced derived
                    // the flag from the status; the validation above keeps that honest by
                    // refusing a non-terminal status here.
                    isProcessed: true,
                    notUpdatedAfter: compareQueueItem.ClaimedAt);
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
