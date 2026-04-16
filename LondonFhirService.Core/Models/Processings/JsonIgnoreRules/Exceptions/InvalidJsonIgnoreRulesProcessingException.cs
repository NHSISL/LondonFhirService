// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Processings.JsonIgnoreRules.Exceptions
{
    public class InvalidJsonIgnoreRulesProcessingException : Xeption
    {
        public InvalidJsonIgnoreRulesProcessingException(string message)
            : base(message)
        { }
    }
}
