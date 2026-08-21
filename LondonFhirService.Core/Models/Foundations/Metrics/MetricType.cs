// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Core.Models.Foundations.Metrics
{
    /// <summary>
    /// The kind of work a metric span represents. Request is the root span; every other value is
    /// a child, directly or indirectly, of a Request span within the same correlation. The nesting
    /// is expressed by Metric.ParentId rather than by this enum, which only names the kind.
    ///
    /// For GetStructuredRecord the tree is:
    ///
    ///   Request
    ///   +- AccessCheck
    ///   +- ProviderRequests
    ///   |  +- ProviderDiscovery
    ///   |  +- ProviderFanOut
    ///   |     +- Provider          (one per provider, in parallel)
    ///   |     |  +- ProviderCall
    ///   |     |  +- Persist
    ///   |     +- Provider
    ///   |        +- ProviderCall
    ///   |        +- Persist
    ///   +- Consolidation
    ///
    /// Which yields, without any span kind being special cased:
    ///
    ///   sibling wait  = ProviderFanOut - Provider          per provider, time spent idle
    ///                                                      waiting for the slowest sibling
    ///   API overhead  = Request - (AccessCheck + ProviderRequests + Consolidation)
    /// </summary>
    public enum MetricType
    {
        /// <summary>The full API request, measured at the controller boundary.</summary>
        Request,

        /// <summary>The consumer access permission check.</summary>
        AccessCheck,

        /// <summary>
        /// All work involved in getting data from providers, covering discovery and the fan out.
        /// The single figure to set against AccessCheck and Consolidation.
        /// </summary>
        ProviderRequests,

        /// <summary>Resolving the set of active providers to fan out to.</summary>
        ProviderDiscovery,

        /// <summary>
        /// The parallel execution barrier that waits for every provider task. Recorded rather
        /// than inferred as the longest Provider span, which is wrong whenever task scheduling
        /// delays a start.
        /// </summary>
        ProviderFanOut,

        /// <summary>
        /// One provider task end to end, including the persistence it performs. Subtracting this
        /// from its ProviderFanOut parent gives the time the result sat idle waiting for the
        /// slowest sibling.
        /// </summary>
        Provider,

        /// <summary>
        /// The outbound call to a provider, excluding the local work that follows it. Separate
        /// from Provider so that a slow provider can be told apart from slow handling of what it
        /// returned.
        /// </summary>
        ProviderCall,

        /// <summary>
        /// Writing a retrieved payload to storage. A child of the Provider span whose payload it
        /// writes, since this happens once per provider inside that provider's own task.
        /// </summary>
        Persist,

        /// <summary>Reconciling the retrieved bundles into a single response.</summary>
        Consolidation
    }
}
