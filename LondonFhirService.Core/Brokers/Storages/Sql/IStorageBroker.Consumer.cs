// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Consumers;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial interface IStorageBroker
    {
        ValueTask<Consumer> InsertConsumerAsync(Consumer consumer);
        ValueTask<IQueryable<Consumer>> SelectAllConsumersAsync();
        ValueTask<Consumer> SelectConsumerByIdAsync(Guid consumerId);
        ValueTask<Consumer> UpdateConsumerAsync(Consumer consumer);
        ValueTask<Consumer> DeleteConsumerAsync(Consumer consumer);
    }
}
