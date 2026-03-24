// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;

namespace LondonFhirService.Manage.Tests.Acceptance.Models.FhirRecordDifferences
{
    public class FhirRecordDifference
    {
        public Guid Id { get; set; }

        // TODO:  Add your properties here

        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
    }
}
