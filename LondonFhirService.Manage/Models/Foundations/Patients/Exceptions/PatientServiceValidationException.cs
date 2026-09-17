// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Manage.Models.Foundations.Patients.Exceptions
{
    public class PatientServiceValidationException : Xeption
    {
        public PatientServiceValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
