// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;

namespace LondonFhirService.Core.Brokers.Correlations
{
    public interface ICorrelationBroker
    {
        ValueTask<Guid> GetCorrelationIdAsync();

        /// <summary>
        /// The span id of the request itself, as 16 hex characters, or null when there is no
        /// request span to name. Captured from the same activity as the correlation id, so the
        /// two always describe one trace.
        /// </summary>
        ValueTask<string> GetRequestSpanIdAsync();
    }
}
