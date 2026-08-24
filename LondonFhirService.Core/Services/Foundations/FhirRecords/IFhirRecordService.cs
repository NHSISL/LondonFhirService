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

        /// <summary>
        /// Claims a record only if it is still exactly as the caller saw it - same status, and no
        /// newer than <paramref name="notUpdatedAfter"/> when supplied - returning true when this
        /// caller won it. Lets the database arbitrate between competing workers.
        /// </summary>
        ValueTask<bool> TryClaimFhirRecordAsync(
            Guid fhirRecordId,
            StatusType expectedStatus,
            StatusType claimedStatus,
            DateTimeOffset? notUpdatedAfter = null);

        ValueTask<FhirRecord> ModifyFhirRecordAsync(FhirRecord fhirRecord);
        ValueTask<FhirRecord> RemoveFhirRecordByIdAsync(Guid fhirRecordId);
    }
}