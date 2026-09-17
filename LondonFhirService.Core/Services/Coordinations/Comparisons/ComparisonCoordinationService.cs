// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Text.Json;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Identifiers;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue;
using LondonFhirService.Core.Models.Orchestrations.Comparisons;
using LondonFhirService.Core.Services.Orchestrations.CompareQueue;

namespace LondonFhirService.Core.Services.Coordinations.Patients.STU3
{
    internal partial class ComparisonCoordinationService : IComparisonCoordinationService
    {
        private readonly ICompareQueueOrchestrationService compareQueueOrchestrationService;
        private readonly IComparisonOrchestrationService comparisonOrchestrationService;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly IIdentifierBroker identifierBroker;
        private readonly ILoggingBroker loggingBroker;

        public ComparisonCoordinationService(
            ICompareQueueOrchestrationService compareQueueOrchestrationService,
            IComparisonOrchestrationService comparisonOrchestrationService,
            IDateTimeBroker dateTimeBroker,
            IIdentifierBroker identifierBroker,
            ILoggingBroker loggingBroker)
        {
            this.compareQueueOrchestrationService = compareQueueOrchestrationService;
            this.comparisonOrchestrationService = comparisonOrchestrationService;
            this.dateTimeBroker = dateTimeBroker;
            this.identifierBroker = identifierBroker;
            this.loggingBroker = loggingBroker;
        }

        /// <summary>
        /// Reports what the statement actually did, not why. Zero rows means the record is no
        /// longer Processing under this worker's token - usually because the lease expired and
        /// another worker took it over, but the same result appears if this worker already
        /// settled the row and came back through the catch arm. The statement cannot tell those
        /// apart, so neither does the message.
        /// </summary>
        private static string LeaseLostMessage(
            CompareQueueItem compareQueueItem,
            StatusType terminalStatus) =>
            $"Settling CorrelationId: {compareQueueItem.SecondaryFhirRecord.CorrelationId} as " +
            $"{terminalStatus} matched no rows - the record is no longer Processing under this " +
            "worker's lease, so nothing was written. Whichever worker holds it now reports its " +
            "own outcome.";

        /// <summary>
        /// Completing the shared primary is the last thing either path does, and its failure is
        /// held here rather than allowed to escape.
        ///
        /// On the success path everything that matters is already durable by this point - the
        /// difference row and the secondary's terminal status - so the comparison HAS succeeded,
        /// and letting this throw would report it as a failure. It would also be reported as the
        /// wrong failure: the catch fences on the secondary still being Processing, and it is
        /// Completed by now, so the fence would match nothing and a real error would be logged as
        /// a lost lease. On the failure path the catch has already run, so an exception here
        /// would escape the loop entirely and abandon the rest of the queue.
        ///
        /// The statement behind it is conditional on the row not already being Completed, so
        /// repeating it is safe and the next secondary of the same correlation finishes the job.
        /// A correlation with only one secondary has no second chance, which is why this is an
        /// error rather than a swallowed warning: nothing revisits primaries, because the queue
        /// only ever offers secondaries.
        /// </summary>
        private async ValueTask CompletePrimaryBestEffortAsync(CompareQueueItem compareQueueItem)
        {
            try
            {
                await this.compareQueueOrchestrationService
                    .CompletePrimaryFhirRecordAsync(compareQueueItem.PrimaryFhirRecord.Id);
            }
            catch (Exception primaryCompletionException)
            {
                await this.loggingBroker.LogErrorAsync(
                    new Exception(
                        $"Could not complete PrimaryFhirRecordId: " +
                        $"{compareQueueItem.PrimaryFhirRecord.Id} for CorrelationId: " +
                        $"{compareQueueItem.SecondaryFhirRecord?.CorrelationId}. The secondary " +
                        "has been settled and any difference row is written, so the comparison " +
                        "itself stands; the primary is left unfinished and only another " +
                        "secondary of the same correlation would pick it up.",
                        primaryCompletionException));
            }
        }

        public ValueTask ProcessFhirRecordsAsync() =>
            TryCatch(async () =>
            {
                CompareQueueItem compareQueueItem;

                while ((compareQueueItem =
                    await this.compareQueueOrchestrationService.GetUnprocessedRecordAsync()) != null)
                {
                    try
                    {
                        if (compareQueueItem.PrimaryFhirRecord == null)
                        {
                            await this.loggingBroker.LogWarningAsync(
                                $"CompareQueueItem with CorrelationId: " +
                                $"{compareQueueItem.SecondaryFhirRecord.CorrelationId} does not have " +
                                $"a primary record. Marking as failed without comparison.");

                            // The primary is looked up after the claim, so a slow lookup is
                            // exactly where the lease has room to expire. The write carries the
                            // ownership test rather than following one, so it cannot land on a
                            // row another worker has since taken over.
                            bool settledWithoutPrimary = await this.compareQueueOrchestrationService
                                .TryFinalizeClaimedFhirRecordAsync(
                                    compareQueueItem, StatusType.Failed);

                            if (settledWithoutPrimary is false)
                            {
                                await this.loggingBroker.LogWarningAsync(
                                    LeaseLostMessage(compareQueueItem, StatusType.Failed));
                            }

                            continue;
                        }

                        ComparisonResult comparisonResult =
                            await this.comparisonOrchestrationService.CompareAsync(
                                correlationId: compareQueueItem.PrimaryFhirRecord.CorrelationId,
                                source1Json: compareQueueItem.PrimaryFhirRecord.JsonPayload,
                                source2Json: compareQueueItem.SecondaryFhirRecord.JsonPayload);

                        string diffJson = JsonSerializer.Serialize(comparisonResult);

                        Guid fhirRecordDifferenceId =
                            await this.identifierBroker.GetIdentifierAsync();

                        DateTimeOffset currentTime =
                            await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

                        var fhirRecordDifference = new FhirRecordDifference
                        {
                            Id = fhirRecordDifferenceId,
                            PrimaryId = compareQueueItem.PrimaryFhirRecord.Id,
                            SecondaryId = compareQueueItem.SecondaryFhirRecord.Id,
                            CorrelationId = comparisonResult.CorrelationId,
                            DiffJson = diffJson,
                            DiffCount = comparisonResult.DiffCount,
                            ComparedAt = currentTime
                        };

                        compareQueueItem.FhirRecordDifference = fhirRecordDifference;

                        // Still a check rather than a fence, and it has to be: the insert below
                        // is not a status write, so no status guard can arbitrate it. This narrows
                        // the window instead of closing it - if the lease expires between here and
                        // the insert, two workers can write a difference row for one pair. The
                        // settle that follows IS fenced, so the record itself cannot be wrongly
                        // finished; only the difference row can duplicate.
                        //
                        // The order is load-bearing. TryRetainClaimAsync advances
                        // compareQueueItem.ClaimedAt on success, so the settle below carries the
                        // fresh token. Move the settle above this call and it would present a
                        // token the row has already moved past, match nothing, and silently stop
                        // completing records.
                        bool stillClaimed = await this.compareQueueOrchestrationService
                            .TryRetainClaimAsync(compareQueueItem);

                        if (stillClaimed is false)
                        {
                            await this.loggingBroker.LogWarningAsync(
                                $"Abandoning comparison for CorrelationId: " +
                                $"{compareQueueItem.SecondaryFhirRecord.CorrelationId}; its lease " +
                                "expired mid-flight and another worker has taken the record over. " +
                                "The comparison is being performed there, so this result is " +
                                "discarded rather than written twice.");

                            continue;
                        }

                        await this.compareQueueOrchestrationService
                            .PersistFhirRecordDifferencesAsync(compareQueueItem);

                        bool settled = await this.compareQueueOrchestrationService
                            .TryFinalizeClaimedFhirRecordAsync(
                                compareQueueItem, StatusType.Completed);

                        if (settled is false)
                        {
                            await this.loggingBroker.LogWarningAsync(
                                LeaseLostMessage(compareQueueItem, StatusType.Completed));

                            continue;
                        }

                        // Reached only once the secondary has actually been settled under this
                        // worker's lease, which is the same gate the failure path applies. A
                        // worker that has lost the row should not go on writing on its behalf,
                        // and nothing is lost by stopping - the worker that took the row over
                        // completes the primary when it finishes.
                        //
                        // Unconditional beyond that gate: the "is it already completed" test
                        // lives in the database statement. The primary is shared by every
                        // secondary of this correlation, so with more than one provider several
                        // workers reach here for the same row, and a read here with a write
                        // there is a race.
                        await CompletePrimaryBestEffortAsync(compareQueueItem);
                    }
                    catch (Exception ex)
                    {
                        await this.loggingBroker.LogErrorAsync(
                            new Exception(
                                $"Failed processing CompareQueueItem for CorrelationId: " +
                                $"{compareQueueItem.SecondaryFhirRecord?.CorrelationId}. " +
                                $"PrimaryFhirRecordId: {compareQueueItem.PrimaryFhirRecord?.Id}." +
                                $"SecondaryFhirRecordId: {compareQueueItem.SecondaryFhirRecord?.Id}.",
                                ex));

                        // Fenced for a worse reason than the success path. Failed is terminal
                        // and nothing reclaims it - GetUnprocessedRecordAsync only takes Pending
                        // or stale Processing rows - so a worker whose lease expired mid-flight
                        // could bury a record another worker was comparing perfectly well, and
                        // that record would never be compared again. Writing nothing is safe: the
                        // worker that holds the row reports its own outcome.
                        bool settledOnFailure = await this.compareQueueOrchestrationService
                            .TryFinalizeClaimedFhirRecordAsync(
                                compareQueueItem, StatusType.Failed);

                        if (settledOnFailure is false)
                        {
                            await this.loggingBroker.LogWarningAsync(
                                LeaseLostMessage(compareQueueItem, StatusType.Failed));

                            continue;
                        }

                        if (compareQueueItem.PrimaryFhirRecord is not null)
                        {
                            await CompletePrimaryBestEffortAsync(compareQueueItem);
                        }
                    }
                }
            });
    }
}
