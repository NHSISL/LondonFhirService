// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ResourceMatchers.Conditions.Exceptions
{
    public class ConditionMatcherServiceValidationException : Xeption
    {
        public ConditionMatcherServiceValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
