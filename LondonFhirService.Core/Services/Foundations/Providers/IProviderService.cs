// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Providers;

namespace LondonFhirService.Core.Services.Foundations.Providers
{
    public interface IProviderService
    {
        ValueTask<Provider> AddProviderAsync(Provider provider);
        ValueTask<IQueryable<Provider>> RetrieveAllProvidersAsync();

        /// <summary>
        /// Retrieves the providers already materialised. Preferred on the request path, where
        /// enumerating a deferred queryable would run the round trip synchronously on the calling
        /// thread.
        /// </summary>
        ValueTask<List<Provider>> RetrieveAllProvidersAsListAsync();
        ValueTask<Provider> RetrieveProviderByIdAsync(Guid providerId);
        ValueTask<Provider> ModifyProviderAsync(Provider provider);
        ValueTask<Provider> RemoveProviderByIdAsync(Guid providerId);
    }
}
