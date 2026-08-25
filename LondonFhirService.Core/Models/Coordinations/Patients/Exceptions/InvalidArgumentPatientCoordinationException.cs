// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Coordinations.Patients.Exceptions
{
    internal class InvalidArgumentPatientCoordinationException : Xeption
    {
        public InvalidArgumentPatientCoordinationException(string message)
            : base(message)
        { }
    }
}