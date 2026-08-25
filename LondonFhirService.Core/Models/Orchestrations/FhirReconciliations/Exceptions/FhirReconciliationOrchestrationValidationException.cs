// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Orchestrations.FhirReconciliations.Exceptions
{
    public class FhirReconciliationOrchestrationValidationException : Xeption
    {
        public FhirReconciliationOrchestrationValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}