// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Processings.JsonIgnoreRules.IdIgnoreRules.Exceptions
{
    public class IdIgnoreProcessingServiceException : Xeption
    {
        public IdIgnoreProcessingServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
