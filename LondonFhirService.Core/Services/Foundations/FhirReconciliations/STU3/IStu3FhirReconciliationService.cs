// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Providers;

namespace LondonFhirService.Core.Services.Foundations.FhirReconciliations.STU3
{
    public interface IStu3FhirReconciliationService
    {
        ValueTask<string> ReconcileSerialisedAsync(
            List<(string Provider, string Json)> bundles,
            string nhsNumber,
            Provider primaryProvider);
    }
}
