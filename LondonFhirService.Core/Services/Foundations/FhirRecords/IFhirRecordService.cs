// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirRecords;

namespace LondonFhirService.Core.Services.Foundations.FhirRecords
{
    public interface IFhirRecordService
    {
        ValueTask<FhirRecord> AddFhirRecordAsync(
            FhirRecord fhirRecord,
            CancellationToken cancellationToken = default);
        ValueTask<IQueryable<FhirRecord>> RetrieveAllFhirRecordsAsync(CancellationToken cancellationToken = default);
        ValueTask<FhirRecord> RetrieveFhirRecordByIdAsync(
            Guid fhirRecordId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Claims a record only if it is still exactly as the caller saw it - same status, and no
        /// newer than <paramref name="notUpdatedAfter"/> when supplied - returning true when this
        /// caller won it. Lets the database arbitrate between competing workers.
        /// </summary>
        ValueTask<bool> TryClaimFhirRecordAsync(
            Guid fhirRecordId,
            StatusType expectedStatus,
            StatusType claimedStatus,
            DateTimeOffset? notUpdatedAfter = null,
            CancellationToken cancellationToken = default);

        ValueTask<FhirRecord> ModifyFhirRecordAsync(
            FhirRecord fhirRecord,
            CancellationToken cancellationToken = default);
        ValueTask<FhirRecord> RemoveFhirRecordByIdAsync(
            Guid fhirRecordId,
            CancellationToken cancellationToken = default);
    }
}