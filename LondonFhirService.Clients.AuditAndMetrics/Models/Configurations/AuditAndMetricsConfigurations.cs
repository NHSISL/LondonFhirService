// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Clients.AuditAndMetrics.Models.Configurations
{
    /// <summary>
    /// Everything the library needs to run, handed in by the consuming application rather than
    /// read from its configuration system, so the library stays standalone.
    /// </summary>
    public class AuditAndMetricsConfigurations
    {
        /// <summary>
        /// The recording kill switch. When false, metric recording is skipped without touching
        /// storage or telemetry. Reads are unaffected.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Whether the retention purge is permitted to delete. Separate from IsEnabled so an
        /// environment can record without ever purging.
        /// </summary>
        public bool IsPurgingAllowed { get; set; }

        /// <summary>
        /// How many days of metrics to keep. Must be greater than zero - a zero or negative
        /// retention period would put the cut off at or after the present and purge everything.
        /// </summary>
        public int RetentionPeriodInDays { get; set; } = 90;

        /// <summary>
        /// How many rows a single purge statement may delete. Bounds the transaction and lock
        /// footprint of each statement rather than the total deleted.
        /// </summary>
        public int PurgeBatchSize { get; set; } = 5000;

        /// <summary>
        /// The ActivitySource name the metric telemetry is published under. Application Insights
        /// in the hosting application collects these; the library does not talk to it directly,
        /// so it carries no Application Insights dependency of its own.
        /// </summary>
        public string ActivitySourceName { get; set; } = "LondonFhirService.Metrics";
    }
}
