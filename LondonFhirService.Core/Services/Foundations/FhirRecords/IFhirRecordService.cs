// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirRecords;

namespace LondonFhirService.Core.Services.Foundations.FhirRecords
{
    public interface IFhirRecordService
    {
        ValueTask<FhirRecord> AddFhirRecordAsync(FhirRecord fhirRecord);
        ValueTask<IQueryable<FhirRecord>> RetrieveAllFhirRecordsAsync();
        ValueTask<FhirRecord> RetrieveFhirRecordByIdAsync(Guid fhirRecordId);
        ValueTask<FhirRecord> ModifyFhirRecordAsync(FhirRecord fhirRecord);
        ValueTask<FhirRecord> RemoveFhirRecordByIdAsync(Guid fhirRecordId);
    }
}