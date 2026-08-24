// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Orchestrations.FhirReconciliations.Exceptions
{
    public class FhirReconciliationOrchestrationDependencyException : Xeption
    {
        public FhirReconciliationOrchestrationDependencyException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}