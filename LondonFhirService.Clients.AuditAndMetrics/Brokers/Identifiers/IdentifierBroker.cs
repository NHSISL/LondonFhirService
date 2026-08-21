// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;

namespace LondonFhirService.Clients.AuditAndMetrics.Brokers.Identifiers
{
    internal class IdentifierBroker : IIdentifierBroker
    {
        public async ValueTask<Guid> GetIdentifierAsync() =>
            Guid.NewGuid();
    }
}
