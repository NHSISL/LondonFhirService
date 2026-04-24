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
    public class ComparisonCoordinationService : IComparisonCoordinationService
    {
        private readonly ICompareQueueOrchestrationService compareQueueOrchestrationService;
        private readonly IComparisonOrchestrationService comparisonOrchestrationService;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ILoggingBroker loggingBroker;
        private readonly IIdentifierBroker identifierBroker;

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

        public async ValueTask ProcessFhirRecords()
        {
            CompareQueueItem compareQueueItem;

            // get next item to process
            while ((compareQueueItem =
                await this.compareQueueOrchestrationService.GetUnprocessedRecordAsync()) != null)
            {
                try
                {
                    // if item does not have secondary records to compare with
                    // mark as complete and continue next iteration
                    if (compareQueueItem.PrimaryFhirRecord == null)
                    {
                        // log warning that primary record is missing for this item
                        await this.loggingBroker.LogWarningAsync(
                            $"CompareQueueItem with CorrelationId: " +
                            $"{compareQueueItem.SecondaryFhirRecord.CorrelationId} does not have " +
                            $"a primary record. Marking as completed without comparison.");

                        // mark secondary record as failed since we cannot compare without primary record
                        await this.compareQueueOrchestrationService
                            .ChangeFhirRecordStatusAsync(compareQueueItem.SecondaryFhirRecord.Id, StatusType.Failed);

                        continue;
                    }

                    // compare primary record with current secondary record
                    ComparisonResult comparisonResult = await this.comparisonOrchestrationService.CompareAsync(
                        correlationId: compareQueueItem.PrimaryFhirRecord.CorrelationId,
                        source1Json: compareQueueItem.PrimaryFhirRecord.JsonPayload,
                        source2Json: compareQueueItem.SecondaryFhirRecord.JsonPayload);

                    // serialize comparison result to JSON and create FhirRecordDifference
                    string diffJson = JsonSerializer.Serialize(comparisonResult);  // Create serializationBroker
                    DateTimeOffset currentTime = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

                    Guid id = await this.identifierBroker.GetIdentifierAsync();

                    FhirRecordDifference fhirRecordDifference = new FhirRecordDifference
                    {
                        Id = id,
                        PrimaryId = compareQueueItem.PrimaryFhirRecord.Id,
                        SecondaryId = compareQueueItem.SecondaryFhirRecord.Id,
                        CorrelationId = comparisonResult.CorrelationId,
                        DiffJson = diffJson,
                        DiffCount = comparisonResult.DiffCount,
                        ComparedAt = currentTime
                    };

                    compareQueueItem.FhirRecordDifference = fhirRecordDifference;

                    // persist differences
                    await this.compareQueueOrchestrationService.PersistFhirRecordDifferencesAsync(compareQueueItem);

                    // mark primary and secondary records as completed
                    await this.compareQueueOrchestrationService
                        .ChangeFhirRecordStatusAsync(compareQueueItem.SecondaryFhirRecord.Id, StatusType.Completed);

                    if (compareQueueItem.PrimaryFhirRecord.Status != StatusType.Completed)
                    {
                        await this.compareQueueOrchestrationService
                            .ChangeFhirRecordStatusAsync(compareQueueItem.PrimaryFhirRecord.Id, StatusType.Completed);
                    }
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

                    await this.compareQueueOrchestrationService
                        .ChangeFhirRecordStatusAsync(compareQueueItem.SecondaryFhirRecord.Id, StatusType.Failed);

                    if (compareQueueItem.PrimaryFhirRecord is not null
                        && compareQueueItem.PrimaryFhirRecord.Status != StatusType.Completed)
                    {
                        await this.compareQueueOrchestrationService
                            .ChangeFhirRecordStatusAsync(
                                compareQueueItem.PrimaryFhirRecord.Id, StatusType.Completed);
                    }
                }
            }
        }
    }
}
