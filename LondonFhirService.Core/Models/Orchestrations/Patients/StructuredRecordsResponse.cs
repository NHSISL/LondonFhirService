// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using LondonFhirService.Core.Models.Foundations.Providers;

namespace LondonFhirService.Core.Models.Orchestrations.Patients
{
    public class StructuredRecordsResponse
    {
        public Provider PrimaryProvider { get; set; }
        public List<(string Provider, string Json)> Bundles { get; set; } = new();
    }
}
