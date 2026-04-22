// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;

namespace LondonFhirService.Core.Services.Coordinations.Patients.STU3
{
    public interface IComparisonCoordinationService
    {
        ValueTask ProcessFhirRecordsAsync();
    }
}
