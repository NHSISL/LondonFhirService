// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Providers;
using Microsoft.EntityFrameworkCore;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial class StorageBroker
    {
        public DbSet<Provider> Providers { get; set; }

        public async ValueTask<Provider> InsertProviderAsync(Provider provider) =>
            await InsertAsync(provider);

        public async ValueTask<IQueryable<Provider>> SelectAllProvidersAsync() =>
            await SelectAllAsync<Provider>();

        public async ValueTask<Provider> SelectProviderByIdAsync(Guid providerId) =>
            await SelectAsync<Provider>(providerId);

        public async ValueTask<Provider> UpdateProviderAsync(Provider provider) =>
            await UpdateAsync(provider);

        public async ValueTask<Provider> DeleteProviderAsync(Provider provider) =>
            await DeleteAsync(provider);
    }
}
