// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Patients.Exceptions
{
    public class InvalidArgumentsPatientServiceException : Xeption
    {
        public InvalidArgumentsPatientServiceException(string message)
            : base(message)
        { }
    }
}
