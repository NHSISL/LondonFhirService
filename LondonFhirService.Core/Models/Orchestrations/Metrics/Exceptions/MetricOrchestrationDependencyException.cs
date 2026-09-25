// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Orchestrations.Metrics.Exceptions
{
    public class MetricOrchestrationDependencyException : Xeption
    {
        public MetricOrchestrationDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
