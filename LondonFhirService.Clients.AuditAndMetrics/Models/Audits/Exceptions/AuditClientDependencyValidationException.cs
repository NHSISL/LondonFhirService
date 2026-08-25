// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Models.Audits.Exceptions
{
    /// <summary>
    /// A dependency rejected the operation for a reason the caller can act on - the row already
    /// exists, or it is locked by a concurrent change. The inner exception carries which, using
    /// the storage contract types from Core.Abstractions, so a caller can map a status code.
    /// </summary>
    public class AuditClientDependencyValidationException : Xeption
    {
        public AuditClientDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
