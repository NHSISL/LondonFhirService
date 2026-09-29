// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;

namespace LondonFhirService.Core.Abstractions.Models.Audits
{
    /// <summary>
    /// The contract this library works against. The concrete entity lives in the consuming
    /// application and derives from this.
    ///
    /// The stamping fields are declared here rather than inherited from IAuditable. IAuditable
    /// is the hosting application's convention for every entity it stamps, not something the
    /// library owns, so inheriting it tied the contract to that convention. A host whose Audit
    /// is also IAuditable still satisfies both with the same four properties.
    /// </summary>
    public interface IAudit : IKey
    {
        string CorrelationId { get; set; }
        string AuditType { get; set; }
        string Title { get; set; }
        string Message { get; set; }
        string FileName { get; set; }
        string LogLevel { get; set; }
        string CreatedBy { get; set; }
        DateTimeOffset CreatedDate { get; set; }
        string UpdatedBy { get; set; }
        DateTimeOffset UpdatedDate { get; set; }
    }
}
