// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Core.Models.Foundations.Metrics
{
    /// <summary>
    /// The kind of work a metric span represents. Request is the root span; every other value
    /// is a child, directly or indirectly, of a Request span within the same correlation.
    /// </summary>
    public enum MetricType
    {
        /// <summary>The full API request, measured at the controller boundary.</summary>
        Request,

        /// <summary>The consumer access permission check.</summary>
        AccessCheck,

        /// <summary>Resolving the set of active providers to fan out to.</summary>
        ProviderDiscovery,

        /// <summary>The parallel execution barrier that waits for every provider task.</summary>
        FanOut,

        /// <summary>A single provider task, end to end, including any persistence it performs.</summary>
        Provider,

        /// <summary>The outbound call to a provider, excluding local work.</summary>
        ProviderCall,

        /// <summary>Writing a retrieved payload to storage.</summary>
        Persist,

        /// <summary>Reconciling the retrieved bundles into a single response.</summary>
        Consolidation
    }
}
