// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace LondonFhirService.Core.Models.Orchestrations.FhirReconciliations.Exceptions
{
    internal class FailedFhirReconciliationOrchestrationException : Xeption
    {
        public FailedFhirReconciliationOrchestrationException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}