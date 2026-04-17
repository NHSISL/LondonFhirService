// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Orchestrations.Comparisons.Exceptions
{
    public class ComparisonOrchestrationDependencyException : Xeption
    {
        public ComparisonOrchestrationDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
