// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Providers;

namespace LondonFhirService.Core.Services.Foundations.Providers
{
    public interface IProviderService
    {
        ValueTask<Provider> AddProviderAsync(Provider provider);
        ValueTask<IQueryable<Provider>> RetrieveAllProvidersAsync();
        ValueTask<Provider> RetrieveProviderByIdAsync(Guid providerId);
        ValueTask<Provider> ModifyProviderAsync(Provider provider);
    }
}
