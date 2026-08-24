// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Models.Audits.Exceptions
{
    /// <summary>
    /// The requested audit does not exist. Distinct from a validation failure so a caller can
    /// answer 404 rather than 400 without naming the library's internal categorization types.
    /// </summary>
    public class AuditClientNotFoundException : Xeption
    {
        public AuditClientNotFoundException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
