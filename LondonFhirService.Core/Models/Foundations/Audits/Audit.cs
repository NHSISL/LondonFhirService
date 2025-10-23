// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Core.Models.Bases;

namespace LondonFhirService.Core.Models.Foundations.Audits
{
    public class Audit : IKey, IAudit
    {
        public Guid Id { get; set; }
        public string CorrelationId { get; set; }
        public string AuditType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string FileName { get; set; }
        public string LogLevel { get; set; } = "Information";
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTimeOffset UpdatedDate { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
