// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Manage.Models.Foundations.Patients.Exceptions
{
    public class PatientServiceException : Xeption
    {
        public PatientServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
