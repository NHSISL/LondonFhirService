// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Api.Workers
{
    /// <summary>
    /// How often the retention sweeps run. What they delete is decided by
    /// AuditAndMetricsConfigurations - this only decides when to ask. One cadence covers both
    /// sweeps: they are the same kind of work against the same database, and separate timers
    /// would only make it possible for the two to land on it at once.
    /// </summary>
    public class AuditAndMetricPurgeWorkerSettings
    {
        /// <summary>
        /// Hours between sweeps. Metrics accumulate a row per span rather than per request, so
        /// that table grows fast, but both retention windows are measured in days - sweeping
        /// more often than daily spends database time to delete rows that are barely over the
        /// line.
        /// </summary>
        public int SweepIntervalHours { get; set; } = 24;

        /// <summary>
        /// How long to wait after startup before the first sweep, so a deployment does not put a
        /// bulk delete and a cold cache into the same minute. Applied once, before the audit
        /// sweep; the metric sweep follows it immediately rather than waiting again.
        /// </summary>
        public int InitialDelayMinutes { get; set; } = 5;
    }
}
