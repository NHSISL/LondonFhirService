// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Processings.JsonIgnoreRules.GuidIgnoreRules.Exceptions
{
    public class InvalidArgumentGuidIgnoreProcessingException : Xeption
    {
        public InvalidArgumentGuidIgnoreProcessingException(string message)
            : base(message)
        { }
    }
}
