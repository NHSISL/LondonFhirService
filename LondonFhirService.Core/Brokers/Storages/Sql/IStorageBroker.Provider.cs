// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Providers;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial interface IStorageBroker
    {
        ValueTask<Provider> InsertProviderAsync(Provider provider);
        ValueTask<IQueryable<Provider>> SelectAllProvidersAsync();

        /// <summary>
        /// Materialises the provider list asynchronously. SelectAllProvidersAsync returns a
        /// deferred queryable, so enumerating it synchronously runs the round trip on the calling
        /// thread; on the patient request path that parks a thread-pool worker inside synchronous
        /// reader I/O instead of yielding it.
        /// </summary>
        ValueTask<List<Provider>> SelectAllProvidersAsListAsync();
        ValueTask<Provider> SelectProviderByIdAsync(Guid providerId);
        ValueTask<Provider> UpdateProviderAsync(Provider provider);
        ValueTask<Provider> DeleteProviderAsync(Provider provider);
    }
}
