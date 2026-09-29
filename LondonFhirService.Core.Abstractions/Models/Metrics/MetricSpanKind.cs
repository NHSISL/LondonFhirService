// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Core.Abstractions.Models.Metrics
{
    /// <summary>
    /// How a replayed span is presented to telemetry, which is all the library needs to know
    /// about the kind of work a metric measured. What that work was is the host's vocabulary and
    /// travels as IMetric.Type; this is the part of it the library acts on.
    ///
    /// Internal is first so that it is the default: a host that never classifies its spans gets
    /// in-process spans, which is the safe reading of work the library knows nothing about.
    /// </summary>
    public enum MetricSpanKind
    {
        /// <summary>Work done within the host that is neither the incoming request nor a call out.</summary>
        Internal,

        /// <summary>The incoming request the host is serving - the root span of a correlation.</summary>
        Server,

        /// <summary>A call out of the host to something it depends on, such as a provider or a database.</summary>
        Client
    }
}
