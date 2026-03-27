// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ResourceMatchers.Patients.Exceptions
{
    public class PatientMatcherServiceValidationException : Xeption
    {
        public PatientMatcherServiceValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
