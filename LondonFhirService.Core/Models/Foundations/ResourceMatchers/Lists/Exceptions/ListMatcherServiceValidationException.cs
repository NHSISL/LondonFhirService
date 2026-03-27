// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ResourceMatchers.Lists.Exceptions
{
    public class ListMatcherServiceValidationException : Xeption
    {
        public ListMatcherServiceValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
