// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Model;

namespace LondonFhirService.Core.Services.Foundations.FhirReconciliations.STU3
{
    public class Stu3FhirReconciliationService : IStu3FhirReconciliationService
    {
        public async ValueTask<Bundle> ReconcileAsync(List<Bundle> bundles, string primaryProviderName)
        {
            return bundles.FirstOrDefault();
        }

        public async ValueTask<string> ReconcileSerialisedAsync(List<string> bundles, string primaryProviderName)
        {
            return bundles.FirstOrDefault();
        }
    }
}
