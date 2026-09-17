// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;

namespace LondonFhirService.Core.Abstractions.Brokers
{
    /// <summary>
    /// The span id of the HTTP request the work being recorded belongs to.
    ///
    /// A port rather than something the library reads for itself, for the same reason
    /// IAuditUserBroker is one: only a host knows what a request is. It is read while the request
    /// is still alive - the metric service stamps before it defers the write - and carried into
    /// the replay, which happens later on a background worker with no request left to ask.
    ///
    /// Null when there is no request behind the work, such as a background worker or a host that
    /// establishes no request span. The library then groups replayed spans by trace alone, which
    /// is what it did before this existed.
    /// </summary>
    public interface IRequestTraceBroker
    {
        ValueTask<string> GetRequestSpanIdAsync();
    }
}
