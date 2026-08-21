// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Core.Abstractions.Models.Metrics
{
    /// <summary>
    /// The outcome of a measured span. Durations are only comparable within a single status:
    /// a span that failed fast or timed out at its configured ceiling will otherwise distort
    /// any average or percentile it is included in.
    /// </summary>
    public enum MetricStatus
    {
        Succeeded,
        Failed,
        TimedOut,
        Cancelled,
        Skipped
    }
}
