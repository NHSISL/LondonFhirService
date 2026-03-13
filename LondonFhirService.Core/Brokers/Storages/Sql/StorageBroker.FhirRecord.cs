// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using Microsoft.EntityFrameworkCore;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial class StorageBroker
    {
        public DbSet<FhirRecord> FhirRecords { get; set; }

        public async ValueTask<FhirRecord> InsertFhirRecordAsync(FhirRecord fhirRecord) =>
            await InsertAsync(fhirRecord);

        public async ValueTask<IQueryable<FhirRecord>> SelectAllFhirRecordsAsync() =>
            await SelectAllAsync<FhirRecord>();

        public async ValueTask<FhirRecord> SelectFhirRecordByIdAsync(Guid fhirRecordId) =>
            await SelectAsync<FhirRecord>(fhirRecordId);

        public async ValueTask<FhirRecord> UpdateFhirRecordAsync(FhirRecord fhirRecord) =>
            await UpdateAsync(fhirRecord);

        public async ValueTask<FhirRecord> DeleteFhirRecordAsync(FhirRecord fhirRecord) =>
            await DeleteAsync(fhirRecord);
    }
}
