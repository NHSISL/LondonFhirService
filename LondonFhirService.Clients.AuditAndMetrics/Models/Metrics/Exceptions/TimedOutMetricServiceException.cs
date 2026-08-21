// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Models.Metrics.Exceptions
{
    internal class TimedOutMetricServiceException : Xeption
    {
        public TimedOutMetricServiceException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}
