// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Patients.Exceptions
{
    internal class InvalidArgumentsPatientServiceException : Xeption
    {
        public InvalidArgumentsPatientServiceException(string message)
            : base(message)
        { }
    }
}
