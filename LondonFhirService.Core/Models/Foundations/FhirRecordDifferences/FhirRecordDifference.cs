// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Core.Models.Bases;

namespace LondonFhirService.Core.Models.Foundations.FhirRecordDifferences
{
    public class FhirRecordDifference : IKey, IAudit
    {
        public Guid Id { get; set; }
        public Guid PrimaryId { get; set; }
        public Guid SecondaryId { get; set; }
        public Guid CorrelationId { get; set; }
        public string DiffJson { get; set; }
        public int DiffCount { get; set; }
        public int AcceptablceDiffCount { get; set; }
        public DateTime ComparedAt { get; set; }
        public string Comment { get; set; }
        public bool IsResolved { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
    }
}
