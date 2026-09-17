// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Manage.Models.Foundations.Patients.Exceptions
{
    internal class InvalidPatientServiceException : Xeption
    {
        public InvalidPatientServiceException(string message)
            : base(message)
        { }
    }
}
