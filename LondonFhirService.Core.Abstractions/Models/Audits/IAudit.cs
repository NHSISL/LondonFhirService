// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Core.Abstractions.Models.Audits
{
    /// <summary>
    /// The contract this library works against. The concrete entity lives in the consuming
    /// application and derives from this.
    ///
    /// Note this is a different thing from LondonFhirService.Core.Abstractions.Models.IAuditable,
    /// which carries only the stamping fields. A concrete Audit implements both.
    /// </summary>
    public interface IAudit : IKey, IAuditable
    {
        string CorrelationId { get; set; }
        string AuditType { get; set; }
        string Title { get; set; }
        string Message { get; set; }
        string FileName { get; set; }
        string LogLevel { get; set; }
    }
}
