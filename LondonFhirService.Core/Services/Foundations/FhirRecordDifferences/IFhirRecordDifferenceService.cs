// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;

namespace LondonFhirService.Core.Services.Foundations.FhirRecordDifferences
{
    public interface IFhirRecordDifferenceService
    {
        ValueTask<FhirRecordDifference> AddFhirRecordDifferenceAsync(FhirRecordDifference fhirRecordDifference);
        ValueTask<IQueryable<FhirRecordDifference>> RetrieveAllFhirRecordDifferencesAsync();
        ValueTask<FhirRecordDifference> RetrieveFhirRecordDifferenceByIdAsync(Guid fhirRecordDifferenceId);
        ValueTask<FhirRecordDifference> ModifyFhirRecordDifferenceAsync(FhirRecordDifference fhirRecordDifference);
        ValueTask<FhirRecordDifference> RemoveFhirRecordDifferenceByIdAsync(Guid fhirRecordDifferenceId);
    }
}