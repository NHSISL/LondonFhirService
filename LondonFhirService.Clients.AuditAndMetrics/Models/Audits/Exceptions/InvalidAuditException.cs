// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Models.Audits.Exceptions
{
    internal class InvalidAuditException : Xeption
    {
        public InvalidAuditException(string message)
            : base(message)
        { }
    }
}
