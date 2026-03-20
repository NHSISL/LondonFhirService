// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public interface IStorageBrokerFactory
    {
        ValueTask<IStorageBroker> CreateStorageBrokerAsync();
    }
}
