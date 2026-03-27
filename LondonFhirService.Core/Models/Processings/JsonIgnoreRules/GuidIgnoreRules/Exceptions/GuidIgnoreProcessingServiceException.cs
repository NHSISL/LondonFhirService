// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Processings.JsonIgnoreRules.GuidIgnoreRules.Exceptions
{
    public class GuidIgnoreProcessingServiceException : Xeption
    {
        public GuidIgnoreProcessingServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
