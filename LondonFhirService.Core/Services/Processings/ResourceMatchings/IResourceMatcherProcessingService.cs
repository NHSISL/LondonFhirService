// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Core.Services.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Services.Processings.ResourceMatchings
{
    public interface IResourceMatcherProcessingService
    {
        IResourceMatcherService? GetMatcher(string resourceType);
        bool HasMatcher(string resourceType);
    }
}
