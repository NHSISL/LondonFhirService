// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Services.Processings.ResourceMatchings;

public partial class ResourceMatcherProcessingService : IResourceMatcherProcessingService
{
    private readonly Dictionary<string, IResourceMatcherService> matchers;
    private readonly ILoggingBroker loggingBroker;

    public ResourceMatcherProcessingService(
        IEnumerable<IResourceMatcherService> matchers,
        ILoggingBroker loggingBroker)
    {
        this.matchers = matchers.ToDictionary(
            matcher => matcher.ResourceType,
            matcher => matcher,
            StringComparer.OrdinalIgnoreCase);

        this.loggingBroker = loggingBroker;
    }

    public ValueTask<IResourceMatcherService?> GetMatcherAsync(string resourceType) =>
        TryCatch(async () =>
        {
            ValidateResourceType(resourceType);

            return this.matchers.TryGetValue(resourceType, out IResourceMatcherService? matcher)
                ? matcher
                : null;
        });

    public ValueTask<bool> HasMatcherAsync(string resourceType) =>
        TryCatch(async () =>
        {
            ValidateResourceType(resourceType);

            return this.matchers.ContainsKey(resourceType);
        });
}
