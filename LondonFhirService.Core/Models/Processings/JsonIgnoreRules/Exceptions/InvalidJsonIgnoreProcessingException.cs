// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Processings.JsonIgnoreRules.Exceptions
{
    public class InvalidJsonIgnoreProcessingException : Xeption
    {
        public InvalidJsonIgnoreProcessingException(string message)
            : base(message)
        { }
    }
}
