// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Processings.JsonIgnoreRules.MetaIgnoreRules.Exceptions
{
    public class MetaIgnoreProcessingValidationException : Xeption
    {
        public MetaIgnoreProcessingValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
