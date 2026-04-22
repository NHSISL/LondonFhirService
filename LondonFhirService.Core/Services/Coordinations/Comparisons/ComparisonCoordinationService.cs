// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Text.Json;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue;
using LondonFhirService.Core.Models.Orchestrations.Comparisons;
using LondonFhirService.Core.Services.Orchestrations.CompareQueue;

namespace LondonFhirService.Core.Services.Coordinations.Patients.STU3
{
    public partial class ComparisonCoordinationService : IComparisonCoordinationService
    {
        private readonly ICompareQueueOrchestrationService compareQueueOrchestrationService;
        private readonly IComparisonOrchestrationService comparisonOrchestrationService;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ILoggingBroker loggingBroker;

        public ComparisonCoordinationService(
            ICompareQueueOrchestrationService compareQueueOrchestrationService,
            IComparisonOrchestrationService comparisonOrchestrationService,
            IDateTimeBroker dateTimeBroker,
            ILoggingBroker loggingBroker)
        {
            this.compareQueueOrchestrationService = compareQueueOrchestrationService;
            this.comparisonOrchestrationService = comparisonOrchestrationService;
            this.dateTimeBroker = dateTimeBroker;
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
                            await this.loggingBroker.LogWarningAsync(
                                $"CompareQueueItem with CorrelationId: " +
                                $"{compareQueueItem.SecondaryFhirRecord.CorrelationId} does not have " +
                                $"a primary record. Marking as completed without comparison.");

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

                        DateTimeOffset currentTime =
                            await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

                        var fhirRecordDifference = new FhirRecordDifference
                        {
                            PrimaryId = compareQueueItem.PrimaryFhirRecord.Id,
                            SecondaryId = compareQueueItem.SecondaryFhirRecord.Id,
                            CorrelationId = comparisonResult.CorrelationId,
                            DiffJson = diffJson,
                            DiffCount = comparisonResult.DiffCount,
                            ComparedAt = currentTime
                        };

                        compareQueueItem.FhirRecordDifference = fhirRecordDifference;

                        await this.compareQueueOrchestrationService
                            .PersistFhirRecordDifferencesAsync(compareQueueItem);

                        await this.compareQueueOrchestrationService
                            .ChangeFhirRecordStatusAsync(
                                compareQueueItem.SecondaryFhirRecord.Id, StatusType.Completed);

                        if (compareQueueItem.PrimaryFhirRecord.Status != StatusType.Completed)
                        {
                            await this.compareQueueOrchestrationService
                                .ChangeFhirRecordStatusAsync(
                                    compareQueueItem.PrimaryFhirRecord.Id, StatusType.Completed);
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
                            .ChangeFhirRecordStatusAsync(
                                compareQueueItem.SecondaryFhirRecord.Id, StatusType.Failed);

                        if (compareQueueItem.PrimaryFhirRecord is not null
                            && compareQueueItem.PrimaryFhirRecord.Status != StatusType.Completed)
                        {
                            await this.compareQueueOrchestrationService
                                .ChangeFhirRecordStatusAsync(
                                    compareQueueItem.PrimaryFhirRecord.Id, StatusType.Completed);
                        }
                    }
                }
            });
    }
}
