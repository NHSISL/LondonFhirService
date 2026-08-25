// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;

namespace LondonFhirService.Manage.Tests.Acceptance.Models.Audits
{
    public class Audit
    {
        public Guid Id { get; set; }
        public string CorrelationId { get; set; }
        public string AuditType { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string FileName { get; set; }
        public string LogLevel { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
    }
}
