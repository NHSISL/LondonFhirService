// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using Microsoft.EntityFrameworkCore;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial class StorageBroker
    {
        public DbSet<FhirRecordDifference> FhirRecordDifferences { get; set; }

        public async ValueTask<FhirRecordDifference> InsertFhirRecordDifferenceAsync(
            FhirRecordDifference fhirRecordDifference) =>
                await InsertAsync(fhirRecordDifference);

        public async ValueTask<IQueryable<FhirRecordDifference>> SelectAllFhirRecordDifferencesAsync() =>
            await SelectAllAsync<FhirRecordDifference>();

        public async ValueTask<FhirRecordDifference> SelectFhirRecordDifferenceByIdAsync(
            Guid fhirRecordDifferenceId) =>
                await SelectAsync<FhirRecordDifference>(fhirRecordDifferenceId);

        public async ValueTask<FhirRecordDifference> UpdateFhirRecordDifferenceAsync(
            FhirRecordDifference fhirRecordDifference) =>
                await UpdateAsync(fhirRecordDifference);

        public async ValueTask<FhirRecordDifference> DeleteFhirRecordDifferenceAsync(
            FhirRecordDifference fhirRecordDifference) =>
                await DeleteAsync(fhirRecordDifference);
    }
}
