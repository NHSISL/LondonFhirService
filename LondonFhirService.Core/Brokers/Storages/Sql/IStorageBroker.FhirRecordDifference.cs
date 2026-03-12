// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial interface IStorageBroker
    {
        ValueTask<FhirRecordDifference> InsertFhirRecordDifferenceAsync(FhirRecordDifference fhirRecordDifference);
        ValueTask<IQueryable<FhirRecordDifference>> SelectAllFhirRecordDifferencesAsync();
        ValueTask<FhirRecordDifference> SelectFhirRecordDifferenceByIdAsync(Guid fhirRecordDifferenceId);
        ValueTask<FhirRecordDifference> UpdateFhirRecordDifferenceAsync(FhirRecordDifference fhirRecordDifference);
        ValueTask<FhirRecordDifference> DeleteFhirRecordDifferenceAsync(FhirRecordDifference fhirRecordDifference);
    }
}
