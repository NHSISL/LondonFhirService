// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Consumers;

namespace LondonFhirService.Core.Services.Foundations.Consumers
{
    public interface IConsumerService
    {
        ValueTask<Consumer> AddConsumerAsync(Consumer consumer);
        ValueTask<IQueryable<Consumer>> RetrieveAllConsumersAsync();
        ValueTask<Consumer> RetrieveConsumerByIdAsync(Guid consumerId);
        ValueTask<Consumer> ModifyConsumerAsync(Consumer consumer);
        ValueTask<Consumer> RemoveConsumerByIdAsync(Guid consumerId);
    }
}
