// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using LondonFhirService.Core.Abstractions.Brokers;
using LondonFhirService.Core.Brokers.Correlations;

namespace LondonFhirService.Core.Brokers.AuditAndMetrics
{
    /// <summary>
    /// Satisfies the metric library's request-span port from this application's correlation
    /// broker, the same way AuditUserBroker satisfies its identity port from the security broker.
    ///
    /// Must be resolved per request: the value it forwards is captured on the HttpContext of the
    /// request in flight, so a singleton would hand every later request the first one's span - or
    /// nothing at all, having been built before any request existed.
    /// </summary>
    public class RequestTraceBroker : IRequestTraceBroker
    {
        private readonly ICorrelationBroker correlationBroker;

        public RequestTraceBroker(ICorrelationBroker correlationBroker) =>
            this.correlationBroker = correlationBroker;

        public async ValueTask<string> GetRequestSpanIdAsync() =>
            await this.correlationBroker.GetRequestSpanIdAsync();
    }
}
