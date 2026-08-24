// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Core.Models.Orchestrations.FhirReconciliations.Exceptions;

namespace LondonFhirService.Core.Services.Orchestrations.FhirReconciliations.STU3
{
    internal partial class Stu3FhirReconciliationService : IStu3FhirReconciliationService
    {
        private static void ValidateBundleIsFound(
            (string Provider, string Json) bundle,
            string nhsNumber,
            Guid correlationId)
        {
            if (bundle == default)
            {
                throw new NotFoundFhirReconciliationOrchestrationException(
                    $"NotFound:Patient resource with id = '{nhsNumber}' not found.  " +
                    $"CorrelationId: {correlationId.ToString()}");
            }
        }
    }
}
