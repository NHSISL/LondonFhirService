// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using LondonFhirService.Core.Abstractions.Brokers;

namespace LondonFhirService.Clients.AuditAndMetrics.Brokers.Metrics
{
    /// <summary>
    /// What the library uses when a host supplies no request-span port: it reports that there is
    /// no request span, and MetricBroker falls back to a parent derived from the correlation id.
    ///
    /// A null object rather than a null field, so the metric service never has to guard the call.
    /// Stateless, so one shared instance serves every client.
    /// </summary>
    internal class UnknownRequestTraceBroker : IRequestTraceBroker
    {
        internal static readonly UnknownRequestTraceBroker Instance = new UnknownRequestTraceBroker();

        public ValueTask<string> GetRequestSpanIdAsync() =>
            ValueTask.FromResult<string>(null);
    }
}
