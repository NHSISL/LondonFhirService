// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Orchestrations.Patients.Exceptions
{
    public class PatientOrchestrationValidationException : Xeption
    {
        public PatientOrchestrationValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
