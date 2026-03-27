// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Processings.JsonIgnoreRules.ArrayOrderIgnoreRules.Exceptions
{
    public class ArrayOrderIgnoreProcessingValidationException : Xeption
    {
        public ArrayOrderIgnoreProcessingValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
