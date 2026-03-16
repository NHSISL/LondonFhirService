// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.FhirRecords;

namespace LondonFhirService.Core.Services.Foundations.FhirRecords
{
    public partial class FhirRecordService : IFhirRecordService
    {
        private readonly IStorageBroker storageBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ISecurityBroker securityBroker;
        private readonly ISecurityAuditBroker securityAuditBroker;
        private readonly ILoggingBroker loggingBroker;

        public FhirRecordService(
            IStorageBroker storageBroker,
            IDateTimeBroker dateTimeBroker,
            ISecurityBroker securityBroker,
            ISecurityAuditBroker securityAuditBroker,
            ILoggingBroker loggingBroker)
        {
            this.storageBroker = storageBroker;
            this.dateTimeBroker = dateTimeBroker;
            this.securityBroker = securityBroker;
            this.securityAuditBroker = securityAuditBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<FhirRecord> AddFhirRecordAsync(FhirRecord fhirRecord) =>
            TryCatch(async () =>
            {
                fhirRecord = await this.securityAuditBroker.ApplyAddAuditValuesAsync(fhirRecord);
                await ValidateFhirRecordOnAdd(fhirRecord);

                return await this.storageBroker.InsertFhirRecordAsync(fhirRecord);
            });

        public ValueTask<IQueryable<FhirRecord>> RetrieveAllFhirRecordsAsync() =>
            TryCatch(async () => await this.storageBroker.SelectAllFhirRecordsAsync());

        public ValueTask<FhirRecord> RetrieveFhirRecordByIdAsync(Guid fhirRecordId) =>
            TryCatch(async () =>
            {
                ValidateFhirRecordId(fhirRecordId);

                FhirRecord maybeFhirRecord = await this.storageBroker
                    .SelectFhirRecordByIdAsync(fhirRecordId);

                ValidateStorageFhirRecord(maybeFhirRecord, fhirRecordId);

                return maybeFhirRecord;
            });

        public ValueTask<FhirRecord> ModifyFhirRecordAsync(FhirRecord fhirRecord) =>
            TryCatch(async () =>
            {
                fhirRecord = await this.securityAuditBroker.ApplyModifyAuditValuesAsync(fhirRecord);

                await ValidateFhirRecordOnModify(fhirRecord);

                FhirRecord maybeFhirRecord =
                    await this.storageBroker.SelectFhirRecordByIdAsync(fhirRecord.Id);

                ValidateStorageFhirRecord(maybeFhirRecord, fhirRecord.Id);

                fhirRecord = await this.securityAuditBroker
                    .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(fhirRecord, maybeFhirRecord);

                ValidateAgainstStorageFhirRecordOnModify(
                    inputFhirRecord: fhirRecord,
                    storageFhirRecord: maybeFhirRecord);

                return await this.storageBroker.UpdateFhirRecordAsync(fhirRecord);
            });

        public ValueTask<FhirRecord> RemoveFhirRecordByIdAsync(Guid fhirRecordId) =>
            TryCatch(async () =>
            {
                ValidateFhirRecordId(fhirRecordId);

                FhirRecord maybeFhirRecord = await this.storageBroker
                    .SelectFhirRecordByIdAsync(fhirRecordId);

                ValidateStorageFhirRecord(maybeFhirRecord, fhirRecordId);

                return await this.storageBroker.DeleteFhirRecordAsync(maybeFhirRecord);
            });
    }
}