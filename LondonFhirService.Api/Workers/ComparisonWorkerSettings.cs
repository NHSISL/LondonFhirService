// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Api.Workers
{
    public class ComparisonWorkerSettings
    {
        /// <summary>
        /// How long the worker sleeps between drains, and so the worst case an operator waits
        /// between a request answering and its comparison appearing.
        ///
        /// A tick that finds nothing costs one indexed query against a filter that matches no
        /// rows, so the interval is set by how long a person is willing to look at an empty
        /// screen rather than by what the database can afford. Sixty seconds meant an operator
        /// following the correlation id straight off the structured record page reliably arrived
        /// before the comparison did.
        /// </summary>
        public int SleepIntervalSeconds { get; set; } = 10;
    }
}
