// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Core.Services.Foundations.AllergyIntolerances;

namespace LondonFhirService.Core.Services.Processings.ResourceMatchings
{
    public interface IAllergyIntoleranceProcessingService
    {
        IAllergyIntoleranceService? GetMatcher(string resourceType);
        bool HasMatcher(string resourceType);
    }
}
