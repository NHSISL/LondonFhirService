// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Processings.JsonIgnoreRules.ArrayOrderIgnoreRules.Exceptions
{
    public class InvalidArgumentArrayOrderIgnoreProcessingException : Xeption
    {
        public InvalidArgumentArrayOrderIgnoreProcessingException(string message)
            : base(message)
        { }
    }
}
