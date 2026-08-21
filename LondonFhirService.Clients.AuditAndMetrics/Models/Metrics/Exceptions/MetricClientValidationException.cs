// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Models.Metrics.Exceptions
{
    public class MetricClientValidationException : Xeption
    {
        public MetricClientValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
