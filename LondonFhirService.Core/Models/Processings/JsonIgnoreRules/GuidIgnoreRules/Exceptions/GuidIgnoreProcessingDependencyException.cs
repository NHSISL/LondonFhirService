// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Processings.JsonIgnoreRules.GuidIgnoreRules.Exceptions
{
    public class GuidIgnoreProcessingDependencyException : Xeption
    {
        public GuidIgnoreProcessingDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
