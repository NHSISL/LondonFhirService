// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial interface IStorageBroker
    {
        ValueTask<ConsumerAccess> InsertConsumerAccessAsync(ConsumerAccess consumerAccess);
        ValueTask<IQueryable<ConsumerAccess>> SelectAllConsumerAccessesAsync();
        ValueTask<ConsumerAccess> SelectConsumerAccessByIdAsync(Guid consumerAccessId);
        ValueTask<ConsumerAccess> UpdateConsumerAccessAsync(ConsumerAccess consumerAccess);
        ValueTask<ConsumerAccess> DeleteConsumerAccessAsync(ConsumerAccess consumerAccess);
    }
}
