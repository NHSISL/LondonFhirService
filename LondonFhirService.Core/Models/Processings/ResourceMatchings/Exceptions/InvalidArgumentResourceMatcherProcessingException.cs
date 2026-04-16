// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Processings.ResourceMatchings.Exceptions
{
    public class InvalidArgumentResourceMatcherProcessingException : Xeption
    {
        public InvalidArgumentResourceMatcherProcessingException(string message)
            : base(message)
        { }
    }
}
