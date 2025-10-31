// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;

namespace LondonFhirService.Core.Services.Foundations.FhirReconciliations
{
    public class FhirReconciliationService : IFhirReconciliationService
    {
        public ValueTask<Bundle> Reconcile(List<Bundle> bundles, string primaryProviderName)
        {
            throw new System.NotImplementedException();
        }
    }
}
