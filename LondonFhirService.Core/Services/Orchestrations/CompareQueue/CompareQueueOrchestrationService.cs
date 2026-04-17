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

namespace LondonFhirService.Core.Services.Orchestrations.Accesses
{
    public class CompareQueueOrchestrationService : ICompareQueueOrchestrationService
    {
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

        public async ValueTask<CompareQueueItem> GetUnprocessedRecordAsync()
        {
            DateTimeOffset currentDateTime =
                await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

            DateTimeOffset bufferedDateTime =
                currentDateTime.AddMinutes(-5); // add buffer to ensure all records are ingested before processing

            IQueryable<FhirRecord> secondaryFhirRecordQueryable =
                await this.fhirRecordService.RetrieveAllFhirRecordsAsync();

            // get the oldest unprocessed secondary record that is past the buffer time
            secondaryFhirRecordQueryable = secondaryFhirRecordQueryable
                .Where(record =>
                    record.Status == StatusType.Pending
                    && !record.IsPrimarySource
                    && record.CreatedDate <= bufferedDateTime)
                .OrderBy(record => record.CreatedDate);

            FhirRecord secondaryFhirRecord = secondaryFhirRecordQueryable.FirstOrDefault();

            if (secondaryFhirRecord == null)
            {
                // no unprocessed records found, return null to end the queue processing
                return null;
            }

            // prevent queue from picking up the same record by marking it as processed before comparison
            secondaryFhirRecord.Status = StatusType.Processing;
            secondaryFhirRecord = await this.fhirRecordService.ModifyFhirRecordAsync(secondaryFhirRecord);

            // get the corresponding primary record for comparison
            IQueryable<FhirRecord> primaryFhirRecordQueryable =
                await this.fhirRecordService.RetrieveAllFhirRecordsAsync();

            primaryFhirRecordQueryable = primaryFhirRecordQueryable
                .Where(record =>
                record.CorrelationId == secondaryFhirRecord.CorrelationId
                && record.IsPrimarySource);

            FhirRecord primaryFhirRecord = primaryFhirRecordQueryable.FirstOrDefault();

            CompareQueueItem compareQueueItems = new CompareQueueItem()
            {
                PrimaryFhirRecord = primaryFhirRecord,
                SecondaryFhirRecord = secondaryFhirRecord
            };

            return compareQueueItems;
        }

        public async ValueTask ChangeFhirRecordStatusAsync(Guid fhirRecordId, StatusType status)
        {
            // ValidateOnMarkFhirRecordsAsProcessed(fhirRecordId, status)

            FhirRecord maybeFhirRecord =
                await this.fhirRecordService.RetrieveFhirRecordByIdAsync(fhirRecordId);

            //ValidateStorageFhirRecord(maybeFhirRecord, maybeFhirRecord.Id);

            maybeFhirRecord.Status = status;

            if (status == StatusType.Completed || status == StatusType.Failed)
            {
                maybeFhirRecord.IsProcessed = true;
            }

            await this.fhirRecordService.ModifyFhirRecordAsync(maybeFhirRecord);
        }

        public async ValueTask PersistFhirRecordDifferencesAsync(CompareQueueItem compareQueueItems)
        {
            // ValidateOnPersistFhirRecordDifferences(compareQueueItems)

            await this.fhirRecordDifferenceService
                .AddFhirRecordDifferenceAsync(compareQueueItems.FhirRecordDifference);
        }
    }
}
