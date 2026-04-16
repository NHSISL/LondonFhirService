// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Processings.ResourceMatchings.Exceptions
{
    public class ResourceMatcherProcessingServiceException : Xeption
    {
        public ResourceMatcherProcessingServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
