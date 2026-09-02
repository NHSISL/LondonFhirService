// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

#nullable enable annotations

using System.Threading.Tasks;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Services.Processings.ResourceMatchings
{
    public interface IResourceMatcherProcessingService
    {
        ValueTask<IResourceMatcherService?> GetMatcherAsync(string resourceType);
        ValueTask<bool> HasMatcherAsync(string resourceType);
    }
}
