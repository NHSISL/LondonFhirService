// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions
{
    public class ResourceMatcherServiceValidationException : Xeption
    {
        public ResourceMatcherServiceValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
