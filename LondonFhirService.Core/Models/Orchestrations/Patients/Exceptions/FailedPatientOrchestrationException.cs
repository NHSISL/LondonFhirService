// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace LondonFhirService.Core.Models.Orchestrations.Patients.Exceptions
{
    public class FailedPatientOrchestrationException : Xeption
    {
        public FailedPatientOrchestrationException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}