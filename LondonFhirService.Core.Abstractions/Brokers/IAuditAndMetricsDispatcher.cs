// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

namespace LondonFhirService.Core.Abstractions.Brokers
{
    /// <summary>
    /// Where a deferred audit or metric write goes.
    ///
    /// The library defers these writes so that recording something does not add to the elapsed
    /// time of the work being recorded. How that deferral is done is the hosting application's
    /// business: a host with a lifecycle can queue the work and drain it under control, which a
    /// library with no lifecycle of its own cannot.
    /// </summary>
    public interface IAuditAndMetricsDispatcher
    {
        /// <summary>
        /// Hands over a unit of deferred work. Returns false when it was not accepted - a full
        /// queue, or a host shutting down.
        ///
        /// Rejection is a return value rather than an exception because the caller is recording
        /// telemetry, not doing the work the request came for. Throwing here would let a full
        /// queue take down the very requests it is trying to measure.
        /// </summary>
        bool TryDispatch(Func<CancellationToken, ValueTask> work);
    }
}
