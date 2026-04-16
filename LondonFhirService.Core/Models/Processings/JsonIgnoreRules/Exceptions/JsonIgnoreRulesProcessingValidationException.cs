// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Processings.JsonIgnoreRules.ArrayOrderIgnoreRules.Exceptions
{
    public class JsonIgnoreRulesProcessingValidationException : Xeption
    {
        public JsonIgnoreRulesProcessingValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
