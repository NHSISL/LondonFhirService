// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Providers;

namespace LondonFhirService.Core.Services.Foundations.Providers
{
    public interface IProviderService
    {
        ValueTask<Provider> AddProviderAsync(
            Provider provider,
            CancellationToken cancellationToken = default);
        ValueTask<IQueryable<Provider>> RetrieveAllProvidersAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the providers already materialised. Preferred on the request path, where
        /// enumerating a deferred queryable would run the round trip synchronously on the calling
        /// thread.
        /// </summary>
        ValueTask<List<Provider>> RetrieveAllProvidersAsListAsync(CancellationToken cancellationToken = default);
        ValueTask<Provider> RetrieveProviderByIdAsync(
            Guid providerId,
            CancellationToken cancellationToken = default);
        ValueTask<Provider> ModifyProviderAsync(
            Provider provider,
            CancellationToken cancellationToken = default);
        ValueTask<Provider> RemoveProviderByIdAsync(
            Guid providerId,
            CancellationToken cancellationToken = default);
    }
}
