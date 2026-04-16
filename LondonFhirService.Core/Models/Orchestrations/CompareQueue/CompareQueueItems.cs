// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;

namespace LondonFhirService.Core.Models.Orchestrations.CompareQueue
{
    public class CompareQueueItems
    {
        public class FhirRecord
        {
            public FhirRecord PrimaryFhirRecord { get; set; }
            public List<FhirRecord> SecondaryFhirRecords { get; set; }
            public List<FhirRecordDifference> FhirRecordDifference { get; set; }
        }
    }
}
