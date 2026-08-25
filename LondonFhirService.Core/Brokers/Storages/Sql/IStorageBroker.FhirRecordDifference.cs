// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial interface IStorageBroker
    {
        ValueTask<FhirRecordDifference> InsertFhirRecordDifferenceAsync(
            FhirRecordDifference fhirRecordDifference,
            CancellationToken cancellationToken = default);

        ValueTask<IQueryable<FhirRecordDifference>> SelectAllFhirRecordDifferencesAsync(
            CancellationToken cancellationToken = default);

        ValueTask<FhirRecordDifference> SelectFhirRecordDifferenceByIdAsync(
            Guid fhirRecordDifferenceId,
            CancellationToken cancellationToken = default);

        ValueTask<FhirRecordDifference> UpdateFhirRecordDifferenceAsync(
            FhirRecordDifference fhirRecordDifference,
            CancellationToken cancellationToken = default);

        ValueTask<FhirRecordDifference> DeleteFhirRecordDifferenceAsync(
            FhirRecordDifference fhirRecordDifference,
            CancellationToken cancellationToken = default);
    }
}
