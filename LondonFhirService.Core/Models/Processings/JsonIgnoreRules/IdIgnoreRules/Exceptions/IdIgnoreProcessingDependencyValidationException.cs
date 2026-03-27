// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Processings.JsonIgnoreRules.IdIgnoreRules.Exceptions
{
    public class IdIgnoreProcessingDependencyValidationException : Xeption
    {
        public IdIgnoreProcessingDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
