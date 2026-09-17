// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Models.Foundations.FhirRecords;

using System;

namespace LondonFhirService.Core.Models.Orchestrations.CompareQueue
{
    public class CompareQueueItem
    {
        /// <summary>
        /// Exactly the value written to the secondary record's UpdatedDate when this worker
        /// claimed it. Re-asserting the claim with it proves the row has not been taken back by
        /// another worker since - which it can be, because a claim whose lease expires while the
        /// work is still running is indistinguishable from one left by a worker that died.
        /// </summary>
        public DateTimeOffset ClaimedAt { get; set; }

        public FhirRecord PrimaryFhirRecord { get; set; }
        public FhirRecord SecondaryFhirRecord { get; set; }
        public FhirRecordDifference FhirRecordDifference { get; set; }
    }
}
