// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Models.Audits.Exceptions
{
    public class AuditClientDependencyException : Xeption
    {
        public AuditClientDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
