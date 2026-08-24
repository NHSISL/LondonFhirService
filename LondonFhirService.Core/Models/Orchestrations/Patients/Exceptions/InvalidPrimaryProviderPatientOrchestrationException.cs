// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Orchestrations.Patients.Exceptions
{
    internal class InvalidPrimaryProviderPatientOrchestrationException : Xeption
    {
        public InvalidPrimaryProviderPatientOrchestrationException(string message)
            : base(message)
        { }
    }
}