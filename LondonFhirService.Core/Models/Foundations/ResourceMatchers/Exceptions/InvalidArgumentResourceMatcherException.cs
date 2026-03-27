// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions
{
    public class InvalidArgumentResourceMatcherException : Xeption
    {
        public InvalidArgumentResourceMatcherException(string message)
            : base(message)
        { }
    }
}
