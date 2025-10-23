// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Consumers;
using Microsoft.EntityFrameworkCore;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial class StorageBroker
    {
        public DbSet<Consumer> Consumers { get; set; }

        public async ValueTask<Consumer> InsertConsumerAsync(Consumer consumer) =>
            await InsertAsync(consumer);

        public async ValueTask<IQueryable<Consumer>> SelectAllConsumersAsync() =>
            await SelectAllAsync<Consumer>();

        public async ValueTask<Consumer> SelectConsumerByIdAsync(Guid consumerId) =>
            await SelectAsync<Consumer>(consumerId);

        public async ValueTask<Consumer> UpdateConsumerAsync(Consumer consumer) =>
            await UpdateAsync(consumer);

        public async ValueTask<Consumer> DeleteConsumerAsync(Consumer consumer) =>
            await DeleteAsync(consumer);
    }
}
