// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses;
using Microsoft.EntityFrameworkCore;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial class StorageBroker
    {
        public DbSet<ConsumerAccess> ConsumerAccesses { get; set; }

        public async ValueTask<ConsumerAccess> InsertConsumerAccessAsync(ConsumerAccess consumerAccess) =>
            await InsertAsync(consumerAccess);

        public async ValueTask<IQueryable<ConsumerAccess>> SelectAllConsumerAccessesAsync() =>
            await SelectAllAsync<ConsumerAccess>();

        public async ValueTask<ConsumerAccess> SelectConsumerAccessByIdAsync(Guid consumerAccessId) =>
            await SelectAsync<ConsumerAccess>(consumerAccessId);

        public async ValueTask<ConsumerAccess> UpdateConsumerAccessAsync(ConsumerAccess consumerAccess) =>
            await UpdateAsync(consumerAccess);

        public async ValueTask<ConsumerAccess> DeleteConsumerAccessAsync(ConsumerAccess consumerAccess) =>
            await DeleteAsync(consumerAccess);
    }
}
