// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Core.Models.Bases;

namespace LondonFhirService.Core.Models.Foundations.FhirRecords
{
    public class FhirRecord : IKey, IAudit
    {
        public Guid Id { get; set; }
        public string CorrelationId { get; set; }
        public string JsonPayload { get; set; }
        public string SourceName { get; set; }
        public bool IsPrimarySource { get; set; }
        public bool IsProcessed { get; set; }
        public StatusType Status { get; set; } = StatusType.Pending;
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
    }
}
