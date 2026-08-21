// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Models.Metrics.Exceptions
{
    public class InvalidMetricException : Xeption
    {
        public InvalidMetricException(string message)
            : base(message)
        { }
    }
}
