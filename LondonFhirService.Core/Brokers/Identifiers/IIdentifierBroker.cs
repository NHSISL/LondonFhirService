// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;

namespace LondonFhirService.Core.Brokers.Identifiers
{
    public interface IIdentifierBroker
    {
        ValueTask<Guid> GetIdentifierAsync();
    }
}
