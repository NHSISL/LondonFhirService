// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Coordinations.Patients.Exceptions
{
    public class PatientCoordinationDependencyException : Xeption
    {
        public PatientCoordinationDependencyException(string message, Xeption? innerException)
            : base(message, innerException)
        { }
    }
}
