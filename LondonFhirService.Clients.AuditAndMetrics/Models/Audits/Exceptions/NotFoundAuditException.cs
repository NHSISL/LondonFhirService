// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Models.Audits.Exceptions
{
    internal class NotFoundAuditException : Xeption
    {
        public NotFoundAuditException(string message)
            : base(message)
        { }
    }
}
