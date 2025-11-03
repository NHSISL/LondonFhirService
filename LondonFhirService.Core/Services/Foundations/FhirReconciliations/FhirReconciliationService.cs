// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Model;

namespace LondonFhirService.Core.Services.Foundations.FhirReconciliations
{
    public class FhirReconciliationService : IFhirReconciliationService
    {
        public async ValueTask<Bundle> Reconcile(List<Bundle> bundles, string primaryProviderName)
        {
            return bundles.FirstOrDefault();
        }
    }
}
