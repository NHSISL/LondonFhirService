// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;

namespace LondonFhirService.Core.Services.Foundations.FhirRecordDifferences
{
    public interface IFhirRecordDifferenceService
    {
        ValueTask<FhirRecordDifference> AddFhirRecordDifferenceAsync(
            FhirRecordDifference fhirRecordDifference,
            CancellationToken cancellationToken = default);
        ValueTask<IQueryable<FhirRecordDifference>> RetrieveAllFhirRecordDifferencesAsync(CancellationToken cancellationToken = default);
        ValueTask<FhirRecordDifference> RetrieveFhirRecordDifferenceByIdAsync(
            Guid fhirRecordDifferenceId,
            CancellationToken cancellationToken = default);
        ValueTask<FhirRecordDifference> ModifyFhirRecordDifferenceAsync(
            FhirRecordDifference fhirRecordDifference,
            CancellationToken cancellationToken = default);
        ValueTask<FhirRecordDifference> RemoveFhirRecordDifferenceByIdAsync(
            Guid fhirRecordDifferenceId,
            CancellationToken cancellationToken = default);
    }
}