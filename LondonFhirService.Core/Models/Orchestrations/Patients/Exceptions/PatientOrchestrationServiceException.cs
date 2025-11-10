// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Orchestrations.Patients.Exceptions
{
    public class PatientOrchestrationServiceException : Xeption
    {
        public PatientOrchestrationServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}