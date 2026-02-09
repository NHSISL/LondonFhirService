// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Foundations.Providers;

namespace LondonFhirService.Core.Services.Foundations.FhirReconciliations.STU3
{
    public class Stu3FhirReconciliationService : IStu3FhirReconciliationService
    {
        public async ValueTask<Bundle> ReconcileAsync(List<Bundle> bundles, Provider primaryProvider)
        {
            return bundles.FirstOrDefault();
        }

        public async ValueTask<string> ReconcileSerialisedAsync(List<string> bundles, Provider primaryProvider)
        {
            return bundles.FirstOrDefault();
        }
    }
}
