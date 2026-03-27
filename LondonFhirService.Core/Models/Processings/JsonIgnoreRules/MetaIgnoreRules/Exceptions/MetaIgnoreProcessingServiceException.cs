// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Processings.JsonIgnoreRules.MetaIgnoreRules.Exceptions
{
    public class MetaIgnoreProcessingServiceException : Xeption
    {
        public MetaIgnoreProcessingServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
