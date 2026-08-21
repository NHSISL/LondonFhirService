// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Models.Audits.Exceptions
{
    public class AuditServiceException : Xeption
    {
        public AuditServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
