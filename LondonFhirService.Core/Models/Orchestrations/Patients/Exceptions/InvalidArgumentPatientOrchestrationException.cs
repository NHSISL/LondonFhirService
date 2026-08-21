// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Orchestrations.Patients.Exceptions
{
    internal class InvalidArgumentPatientOrchestrationException : Xeption
    {
        public InvalidArgumentPatientOrchestrationException(string message)
            : base(message)
        { }
    }
}