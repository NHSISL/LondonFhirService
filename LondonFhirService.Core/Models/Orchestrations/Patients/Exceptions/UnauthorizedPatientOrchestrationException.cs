// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Orchestrations.Patients.Exceptions
{
    public class UnauthorizedPatientOrchestrationException : Xeption
    {
        public UnauthorizedPatientOrchestrationException(string message)
            : base(message)
        { }
    }
}
