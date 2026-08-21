// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Models.Metrics.Exceptions
{
    internal class InvalidMetricException : Xeption
    {
        public InvalidMetricException(string message)
            : base(message)
        { }
    }
}
