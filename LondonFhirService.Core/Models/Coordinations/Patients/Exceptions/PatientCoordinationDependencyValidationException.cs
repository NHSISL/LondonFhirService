// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Coordinations.Patients.Exceptions
{
    public class PatientCoordinationDependencyValidationException : Xeption
    {
        public PatientCoordinationDependencyValidationException(string message, Xeption? innerException)
            : base(message, innerException)
        { }
    }
}
