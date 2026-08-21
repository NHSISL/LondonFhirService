// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Models.Audits.Exceptions
{
    public class AuditClientServiceException : Xeption
    {
        public AuditClientServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
