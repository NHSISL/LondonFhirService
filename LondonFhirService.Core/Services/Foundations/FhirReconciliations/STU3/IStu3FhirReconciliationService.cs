// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Foundations.Providers;

namespace LondonFhirService.Core.Services.Foundations.FhirReconciliations.STU3
{
    public interface IStu3FhirReconciliationService
    {
        ValueTask<Bundle> ReconcileAsync(List<Bundle> bundles, Provider primaryProvider);
        ValueTask<string> ReconcileSerialisedAsync(List<string> bundles, Provider primaryProvider);
    }
}
