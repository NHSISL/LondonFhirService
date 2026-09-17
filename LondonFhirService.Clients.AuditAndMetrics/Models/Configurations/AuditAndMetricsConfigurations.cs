// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Clients.AuditAndMetrics.Models.Configurations
{
    /// <summary>
    /// Everything the library needs to run, handed in by the consuming application rather than
    /// read from its configuration system, so the library stays standalone.
    ///
    /// Audit and metrics are switched independently. They are the same library but not the same
    /// obligation - audit entries are a record of who did what and are usually required, while
    /// metrics are volume telemetry and are the first thing an environment wants to turn off. A
    /// single switch over both meant silencing the noisy one also silenced the required one.
    /// </summary>
    public class AuditAndMetricsConfigurations
    {
        /// <summary>
        /// The audit recording kill switch. When false, audit writes are skipped without
        /// touching storage. Reads are unaffected, and metrics are unaffected.
        /// </summary>
        public bool IsAuditEnabled { get; set; } = true;

        /// <summary>
        /// Whether the audit retention purge is permitted to delete. Separate from
        /// IsAuditEnabled so an environment can record without ever purging.
        /// </summary>
        public bool IsAuditPurgingAllowed { get; set; } = true;

        /// <summary>
        /// How many days of audits to keep. Must be greater than zero - a zero or negative
        /// retention period would put the cut off at or after the present and purge everything.
        /// </summary>
        public int AuditRetentionPeriodInDays { get; set; } = 30;

        /// <summary>
        /// The metric recording kill switch. When false, metric recording is skipped without
        /// touching storage or telemetry. Reads are unaffected, and audits are unaffected.
        /// </summary>
        public bool IsMetricsEnabled { get; set; } = true;

        /// <summary>
        /// Whether the metric retention purge is permitted to delete. Separate from
        /// IsMetricsEnabled so an environment can record without ever purging.
        /// </summary>
        public bool IsMetricsPurgingAllowed { get; set; } = true;

        /// <summary>
        /// How many days of metrics to keep. Must be greater than zero - a zero or negative
        /// retention period would put the cut off at or after the present and purge everything.
        /// </summary>
        public int MetricsRetentionPeriodInDays { get; set; } = 30;

        /// <summary>
        /// How many rows a single purge statement may delete. Bounds the transaction and lock
        /// footprint of each statement rather than the total deleted. Shared by both purges -
        /// it is a property of the database, not of what is being purged.
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
