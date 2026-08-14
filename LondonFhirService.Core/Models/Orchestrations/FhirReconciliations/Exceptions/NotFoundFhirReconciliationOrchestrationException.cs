// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Orchestrations.FhirReconciliations.Exceptions
{
    public class NotFoundFhirReconciliationOrchestrationException : Xeption
    {
        public NotFoundFhirReconciliationOrchestrationException(string message)
            : base(message)
        { }
    }
}