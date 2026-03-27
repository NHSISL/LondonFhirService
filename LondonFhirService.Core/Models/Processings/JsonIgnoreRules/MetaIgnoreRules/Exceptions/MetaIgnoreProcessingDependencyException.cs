// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Processings.JsonIgnoreRules.MetaIgnoreRules.Exceptions
{
    public class MetaIgnoreProcessingDependencyException : Xeption
    {
        public MetaIgnoreProcessingDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
