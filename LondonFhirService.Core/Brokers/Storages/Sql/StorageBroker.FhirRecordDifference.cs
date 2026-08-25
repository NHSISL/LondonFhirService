// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using Microsoft.EntityFrameworkCore;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial class StorageBroker
    {
        public DbSet<FhirRecordDifference> FhirRecordDifferences { get; set; }

        public async ValueTask<FhirRecordDifference> InsertFhirRecordDifferenceAsync(
            FhirRecordDifference fhirRecordDifference,
            CancellationToken cancellationToken = default) =>
                await InsertAsync(fhirRecordDifference, cancellationToken);

        public async ValueTask<IQueryable<FhirRecordDifference>> SelectAllFhirRecordDifferencesAsync(
            CancellationToken cancellationToken = default) =>
            await SelectAllAsync<FhirRecordDifference>(cancellationToken);

        public async ValueTask<FhirRecordDifference> SelectFhirRecordDifferenceByIdAsync(
            Guid fhirRecordDifferenceId,
            CancellationToken cancellationToken = default) =>
                await SelectAsync<FhirRecordDifference>(
                    new object[] { fhirRecordDifferenceId }, cancellationToken);

        public async ValueTask<FhirRecordDifference> UpdateFhirRecordDifferenceAsync(
            FhirRecordDifference fhirRecordDifference,
            CancellationToken cancellationToken = default) =>
                await UpdateAsync(fhirRecordDifference, cancellationToken);

        public async ValueTask<FhirRecordDifference> DeleteFhirRecordDifferenceAsync(
            FhirRecordDifference fhirRecordDifference,
            CancellationToken cancellationToken = default) =>
                await DeleteAsync(fhirRecordDifference, cancellationToken);
    }
}
