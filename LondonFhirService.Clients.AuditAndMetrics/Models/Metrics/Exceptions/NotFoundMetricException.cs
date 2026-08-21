// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Models.Metrics.Exceptions
{
    public class NotFoundMetricException : Xeption
    {
        public NotFoundMetricException(string message)
            : base(message)
        { }
    }
}
