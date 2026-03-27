// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ResourceMatchers.AllergyIntolerances.Exceptions
{
    public class AllergyIntoleranceMatcherServiceValidationException : Xeption
    {
        public AllergyIntoleranceMatcherServiceValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
