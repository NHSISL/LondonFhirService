// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Providers;
using Microsoft.EntityFrameworkCore;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial class StorageBroker
    {
        public DbSet<Provider> Providers { get; set; }

        public async ValueTask<Provider> InsertProviderAsync(
            Provider provider,
            CancellationToken cancellationToken = default) =>
            await InsertAsync(provider, cancellationToken);

        public async ValueTask<IQueryable<Provider>> SelectAllProvidersAsync(
            CancellationToken cancellationToken = default) =>
            await SelectAllAsync<Provider>(cancellationToken);

        public async ValueTask<List<Provider>> SelectAllProvidersAsListAsync(
            CancellationToken cancellationToken = default) =>
            await this.Providers.ToListAsync(cancellationToken);

        public async ValueTask<Provider> SelectProviderByIdAsync(
            Guid providerId,
            CancellationToken cancellationToken = default) =>
            await SelectAsync<Provider>(new object[] { providerId }, cancellationToken);

        public async ValueTask<Provider> UpdateProviderAsync(
            Provider provider,
            CancellationToken cancellationToken = default) =>
            await UpdateAsync(provider, cancellationToken);

        public async ValueTask<Provider> DeleteProviderAsync(
            Provider provider,
            CancellationToken cancellationToken = default) =>
            await DeleteAsync(provider, cancellationToken);
    }
}
