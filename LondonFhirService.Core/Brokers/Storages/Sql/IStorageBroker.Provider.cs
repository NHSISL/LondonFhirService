// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Providers;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial interface IStorageBroker
    {
        ValueTask<Provider> InsertProviderAsync(Provider provider);
        ValueTask<IQueryable<Provider>> SelectAllProvidersAsync();
        ValueTask<Provider> SelectProviderByIdAsync(Guid providerId);
        ValueTask<Provider> UpdateProviderAsync(Provider provider);
        ValueTask<Provider> DeleteProviderAsync(Provider provider);
    }
}
