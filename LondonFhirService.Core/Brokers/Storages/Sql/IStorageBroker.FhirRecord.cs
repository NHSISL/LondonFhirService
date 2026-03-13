// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirRecords;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial interface IStorageBroker
    {
        ValueTask<FhirRecord> InsertFhirRecordAsync(FhirRecord fhirRecord);
        ValueTask<IQueryable<FhirRecord>> SelectAllFhirRecordsAsync();
        ValueTask<FhirRecord> SelectFhirRecordByIdAsync(Guid fhirRecordId);
        ValueTask<FhirRecord> UpdateFhirRecordAsync(FhirRecord fhirRecord);
        ValueTask<FhirRecord> DeleteFhirRecordAsync(FhirRecord fhirRecord);
    }
}
