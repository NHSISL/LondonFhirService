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
    public partial class CompareQueueOrchestrationService : ICompareQueueOrchestrationService
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

        public ValueTask<CompareQueueItem> GetUnprocessedRecordAsync() =>
            TryCatch(async () =>
            {
                DateTimeOffset currentDateTime =
                    await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

                DateTimeOffset bufferedDateTime = currentDateTime.AddMinutes(-5);

                IQueryable<FhirRecord> secondaryFhirRecordQueryable =
                    await this.fhirRecordService.RetrieveAllFhirRecordsAsync();

                secondaryFhirRecordQueryable = secondaryFhirRecordQueryable
                    .Where(fhirRecord =>
                        fhirRecord.Status == StatusType.Pending
                        && !fhirRecord.IsPrimarySource
                        && fhirRecord.CreatedDate <= bufferedDateTime)
                    .OrderBy(fhirRecord => fhirRecord.CreatedDate);

                FhirRecord secondaryFhirRecord = secondaryFhirRecordQueryable.FirstOrDefault();

                if (secondaryFhirRecord == null)
                {
                    return null;
                }

                secondaryFhirRecord.Status = StatusType.Processing;

                secondaryFhirRecord =
                    await this.fhirRecordService.ModifyFhirRecordAsync(secondaryFhirRecord);

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
