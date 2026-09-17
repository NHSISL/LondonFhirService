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
                            // Fenced like the other two terminal writes. The primary lookup happens
                            // after the claim, so a slow one leaves room for the lease to expire and
                            // the row to be taken over - and this branch would then mark the new
                            // holder's row Failed, which nothing reclaims, hiding a comparison that
                            // worker may have been about to complete with a primary this one simply
                            // read too early.
                            bool stillClaimedWithoutPrimary = await this.compareQueueOrchestrationService
                                .TryRetainClaimAsync(compareQueueItem);

                            if (stillClaimedWithoutPrimary is false)
                            {
                                await this.loggingBroker.LogWarningAsync(
                                    $"Not marking CorrelationId: " +
                                    $"{compareQueueItem.SecondaryFhirRecord.CorrelationId} as failed " +
                                    "for a missing primary; its lease expired mid-flight and another " +
                                    "worker has taken the record over.");

                                continue;
                            }

                            await this.loggingBroker.LogWarningAsync(
                                $"CompareQueueItem with CorrelationId: " +
                                $"{compareQueueItem.SecondaryFhirRecord.CorrelationId} does not have " +
                                $"a primary record. Marking as failed without comparison.");

                            await this.compareQueueOrchestrationService
                                .ChangeFhirRecordStatusAsync(
                                    compareQueueItem.SecondaryFhirRecord.Id, StatusType.Failed);

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

                        // Checked here, immediately before anything is written. A comparison that
                        // outran its lease has already been handed to another worker, which is
                        // producing the same result - persisting ours too would put two difference
                        // rows against one pair. Nothing is lost by stopping: the worker that took
                        // the row over finishes it.
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

                        await this.compareQueueOrchestrationService
                            .ChangeFhirRecordStatusAsync(
                                compareQueueItem.SecondaryFhirRecord.Id, StatusType.Completed);

                        // Unconditional: the "is it already completed" test lives in the database
                        // statement now. The primary is shared by every secondary of this
                        // correlation, so with more than one provider several workers reach here
                        // for the same row and a read here with a write there is a race.
                        await this.compareQueueOrchestrationService
                            .CompletePrimaryFhirRecordAsync(compareQueueItem.PrimaryFhirRecord.Id);
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

                        // Fenced exactly like the success path, and for a worse reason. Failed is
                        // terminal and nothing reclaims it - GetUnprocessedRecordAsync only takes
                        // Pending or stale Processing rows - so a worker whose lease expired
                        // mid-flight could bury a record another worker was comparing perfectly
                        // well, and that record would never be compared again. Writing nothing is
                        // safe: the worker that holds the row reports its own outcome.
                        bool stillClaimedOnFailure = await this.compareQueueOrchestrationService
                            .TryRetainClaimAsync(compareQueueItem);

                        if (stillClaimedOnFailure is false)
                        {
                            await this.loggingBroker.LogWarningAsync(
                                $"Not marking CorrelationId: " +
                                $"{compareQueueItem.SecondaryFhirRecord.CorrelationId} as failed; " +
                                "its lease expired mid-flight and another worker has taken the " +
                                "record over. Marking it failed here would be terminal and would " +
                                "bury a comparison that worker may yet complete.");

                            continue;
                        }

                        await this.compareQueueOrchestrationService
                            .ChangeFhirRecordStatusAsync(
                                compareQueueItem.SecondaryFhirRecord.Id, StatusType.Failed);

                        if (compareQueueItem.PrimaryFhirRecord is not null)
                        {
                            await this.compareQueueOrchestrationService
                                .CompletePrimaryFhirRecordAsync(
                                    compareQueueItem.PrimaryFhirRecord.Id);
                        }
                    }
                }
            });
    }
}
