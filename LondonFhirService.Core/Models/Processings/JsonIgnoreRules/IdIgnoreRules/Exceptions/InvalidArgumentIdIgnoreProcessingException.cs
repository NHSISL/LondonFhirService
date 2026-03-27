// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Processings.JsonIgnoreRules.IdIgnoreRules.Exceptions
{
    public class InvalidArgumentIdIgnoreProcessingException : Xeption
    {
        public InvalidArgumentIdIgnoreProcessingException(string message)
            : base(message)
        { }
    }
}
