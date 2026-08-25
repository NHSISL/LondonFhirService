// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Core.Abstractions.Models.Metrics
{
    /// <summary>
    /// The kind of work a metric span represents. Request is the root span; every other value is
    /// a child, directly or indirectly, of a Request span within the same correlation. The nesting
    /// is expressed by Metric.ParentId rather than by this enum, which only names the kind.
    ///
    /// For GetStructuredRecord the tree is:
    ///
    ///   Request
    ///   +- Orchestration
    ///   |  +- AccessCheck
    ///   |  +- ProviderRequests
    ///   |     +- ProviderDiscovery
    ///   |     +- Foundation
    ///   |        +- ProviderFanOut
    ///   |           +- Provider    (one per provider, in parallel)
    ///   |           |  +- ProviderCall
    ///   |           |  +- Persist  (deferred; runs after its parent completes)
    ///   |           +- Provider
    ///   |              +- ProviderCall
    ///   |              +- Persist  (deferred)
    ///   +- Consolidation
    ///
    /// Which yields, without any span kind being special cased:
    ///
    ///   sibling wait  = ProviderFanOut - Provider          per provider, time spent idle
    ///                                                      waiting for the slowest sibling
    ///   API overhead  = Request - (Orchestration + Consolidation)
    ///
    /// Persist is the one span whose duration is not part of its ancestors' durations: the write
    /// is dispatched to a background queue, so it starts around the time its Provider parent
    /// finishes and costs the request nothing.
    /// </summary>
    public enum MetricType
    {
        /// <summary>
        /// The root span: the coordination service end to end, which is the outermost layer that
        /// records spans. Controller and middleware time sits above it and is not measured, so
        /// this is the figure the old "Coordination Service Request Completed in Nms" audit line
        /// used to carry, not the wire-to-wire time.
        /// </summary>
        Request,

        /// <summary>
        /// The orchestration layer end to end: access check, provider requests, and the
        /// orchestration overhead between them. Subtracting AccessCheck and ProviderRequests
        /// from it isolates that overhead.
        /// </summary>
        Orchestration,

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
        /// The foundation service end to end: the fan out plus the assembly of its outcomes.
        /// Sits between ProviderRequests and ProviderFanOut, so foundation overhead is
        /// Foundation - ProviderFanOut.
        /// </summary>
        Foundation,

        /// <summary>
        /// The parallel execution barrier that waits for every provider task. Recorded rather
        /// than inferred as the longest Provider span, which is wrong whenever task scheduling
        /// delays a start.
        /// </summary>
        ProviderFanOut,

        /// <summary>
        /// One provider task end to end, excluding the persistence it queues - that write is
        /// deferred and recorded as a Persist child. Subtracting this from its ProviderFanOut
        /// parent gives the time the result sat idle waiting for the slowest sibling.
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
        /// writes, but deferred to a background queue - its duration is deliberately not part of
        /// its parent's, or of the request's.
        /// </summary>
        Persist,

        /// <summary>Reconciling the retrieved bundles into a single response.</summary>
        Consolidation
    }
}
