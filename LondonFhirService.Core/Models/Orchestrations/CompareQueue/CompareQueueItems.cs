// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Models.Foundations.FhirRecords;

namespace LondonFhirService.Core.Models.Orchestrations.CompareQueue
{
    public class CompareQueueItem
    {
        public FhirRecord PrimaryFhirRecord { get; set; }
        public FhirRecord SecondaryFhirRecord { get; set; }
        public FhirRecordDifference FhirRecordDifference { get; set; }
    }
}
