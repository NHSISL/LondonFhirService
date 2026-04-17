// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Orchestrations.Comparisons.Exceptions
{
    public class InvalidComparisonOrchestrationException : Xeption
    {
        public InvalidComparisonOrchestrationException(string message)
            : base(message)
        { }
    }
}
