// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;

namespace LondonFhirService.Api.Tests.Acceptance.Models.FhirRecordDifferences
{
    public class FhirRecordDifference
    {
        public Guid Id { get; set; }
        public Guid PrimaryId { get; set; }
        public Guid SecondaryId { get; set; }
        public string CorrelationId { get; set; }
        public string DiffJson { get; set; }
        public int DiffCount { get; set; }
        public int AcceptableDiffCount { get; set; }
        public DateTimeOffset ComparedAt { get; set; }
        public string Comment { get; set; }
        public bool IsResolved { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
    }
}
