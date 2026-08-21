// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Core.Abstractions.Models.Audits;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Models.Audits
{
    /// <summary>
    /// Stands in for the concrete entity the hosting application supplies. The library holds no
    /// implementation of IAudit by design - that is what keeps the reference one way - so the
    /// tests bring their own.
    /// </summary>
    internal class TestAudit : IAudit
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
