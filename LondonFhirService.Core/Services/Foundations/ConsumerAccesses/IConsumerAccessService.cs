// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses;

namespace LondonFhirService.Core.Services.Foundations.ConsumerAccesses
{
    public interface IConsumerAccessService
    {
        ValueTask<ConsumerAccess> AddConsumerAccessAsync(ConsumerAccess consumerAccess);
        ValueTask<IQueryable<ConsumerAccess>> RetrieveAllConsumerAccessesAsync();
        ValueTask<ConsumerAccess> RetrieveConsumerAccessByIdAsync(Guid consumerAccessId);
        ValueTask<ConsumerAccess> ModifyConsumerAccessAsync(ConsumerAccess consumerAccess);
        ValueTask<ConsumerAccess> RemoveConsumerAccessByIdAsync(Guid consumerAccessId);
        ValueTask<List<string>> RetrieveAllActiveOrganisationsUserHasAccessToAsync(Guid consumerId);
    }
}
