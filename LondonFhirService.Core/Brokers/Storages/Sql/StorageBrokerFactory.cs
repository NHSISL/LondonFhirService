// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public sealed class StorageBrokerFactory : IStorageBrokerFactory
    {
        private readonly IDbContextFactory<StorageBroker> dbContextFactory;

        public StorageBrokerFactory(IDbContextFactory<StorageBroker> dbContextFactory) =>
            this.dbContextFactory = dbContextFactory;

        public async ValueTask<IStorageBroker> CreateStorageBrokerAsync() =>
            await this.dbContextFactory.CreateDbContextAsync();
    }
}
