// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Models.Audits.Exceptions
{
    internal class FailedAuditServiceException : Xeption
    {
        public FailedAuditServiceException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}
