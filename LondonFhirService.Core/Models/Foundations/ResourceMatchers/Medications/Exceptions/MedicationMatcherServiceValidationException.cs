// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ResourceMatchers.Medications.Exceptions
{
    public class MedicationMatcherServiceValidationException : Xeption
    {
        public MedicationMatcherServiceValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
