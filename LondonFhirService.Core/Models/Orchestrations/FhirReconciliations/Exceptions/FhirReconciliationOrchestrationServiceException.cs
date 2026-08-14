// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Orchestrations.FhirReconciliations.Exceptions
{
    public class FhirReconciliationOrchestrationServiceException : Xeption
    {
        public FhirReconciliationOrchestrationServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}