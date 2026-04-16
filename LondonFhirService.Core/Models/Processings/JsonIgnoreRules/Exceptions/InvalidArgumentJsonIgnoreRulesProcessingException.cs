// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Processings.JsonIgnoreRules.ArrayOrderIgnoreRules.Exceptions
{
    public class InvalidArgumentJsonIgnoreRulesProcessingException : Xeption
    {
        public InvalidArgumentJsonIgnoreRulesProcessingException(string message)
            : base(message)
        { }
    }
}
