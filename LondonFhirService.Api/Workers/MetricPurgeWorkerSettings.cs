// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Api.Workers
{
    /// <summary>
    /// How often the retention sweep runs. What it deletes is decided by
    /// AuditAndMetricsConfigurations - this only decides when to ask.
    /// </summary>
    public class MetricPurgeWorkerSettings
    {
        /// <summary>
        /// Hours between sweeps. Metrics accumulate a row per span rather than per request, so
        /// the table grows fast, but the retention window is measured in days - sweeping more
        /// often than daily spends database time to delete rows that are barely over the line.
        /// </summary>
        public int SweepIntervalHours { get; set; } = 24;

        /// <summary>
        /// How long to wait after startup before the first sweep, so a deployment does not put a
        /// bulk delete and a cold cache into the same minute.
        /// </summary>
        public int InitialDelayMinutes { get; set; } = 5;
    }
}
