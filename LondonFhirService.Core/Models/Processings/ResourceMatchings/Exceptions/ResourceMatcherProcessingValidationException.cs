// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Processings.ResourceMatchings.Exceptions
{
    public class ResourceMatcherProcessingValidationException : Xeption
    {
        public ResourceMatcherProcessingValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
