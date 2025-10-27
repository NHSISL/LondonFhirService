// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Providers;

namespace LondonFhirService.Core.Services.Foundations.Providers
{
    public partial class ProviderService
    {
        private delegate ValueTask<Provider> ReturningProviderFunction();

        private async ValueTask<Provider> TryCatch(ReturningProviderFunction returningProviderFunction)
        {
            return await returningProviderFunction();
        }
    }
}
