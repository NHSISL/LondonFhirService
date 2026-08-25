// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace LondonFhirService.Core.Models.Processings.JsonIgnoreRules.ArrayOrderIgnoreRules.Exceptions
{
    internal class FailedJsonIgnoreRulesProcessingException : Xeption
    {
        public FailedJsonIgnoreRulesProcessingException(
            string message, 
            Exception innerException, 
            IDictionary data)
                : base(message, innerException, data)
        { }
    }
}
