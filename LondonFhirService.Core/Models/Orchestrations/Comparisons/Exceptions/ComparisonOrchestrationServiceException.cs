// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Orchestrations.Comparisons.Exceptions
{
    public class ComparisonOrchestrationServiceException : Xeption
    {
        public ComparisonOrchestrationServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
