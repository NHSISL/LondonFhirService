// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Api.Dispatchers
{
    public class AuditAndMetricsDispatcherSettings
    {
        /// <summary>
        /// How many deferred writes may be waiting before new ones are refused. Sized to absorb a
        /// burst, not to hold a backlog: if the queue is persistently near this, the drain is not
        /// keeping up and the answer is more workers or fewer spans, not a bigger buffer.
        /// </summary>
        public int Capacity { get; set; } = 10_000;

        /// <summary>
        /// How many writes drain at once. Above one purely so a single slow write does not stall
        /// the queue behind it.
        /// </summary>
        public int DrainConcurrency { get; set; } = 4;
    }
}
