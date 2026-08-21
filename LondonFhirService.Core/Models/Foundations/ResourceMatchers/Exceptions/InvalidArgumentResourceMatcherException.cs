// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions
{
    internal class InvalidArgumentResourceMatcherException : Xeption
    {
        public InvalidArgumentResourceMatcherException(string message)
            : base(message)
        { }
    }
}
