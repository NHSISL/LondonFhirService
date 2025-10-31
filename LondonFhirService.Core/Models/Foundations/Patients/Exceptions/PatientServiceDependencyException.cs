// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Patients.Exceptions
{
    public class PatientServiceDependencyException : Xeption
    {
        public PatientServiceDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
