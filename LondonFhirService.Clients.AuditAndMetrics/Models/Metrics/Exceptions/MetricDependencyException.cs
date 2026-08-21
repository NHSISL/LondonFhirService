// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Models.Metrics.Exceptions
{
    public class MetricDependencyException : Xeption
    {
        public MetricDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
