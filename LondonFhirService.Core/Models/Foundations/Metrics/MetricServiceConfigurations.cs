// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Core.Models.Foundations.Metrics
{
    public class MetricServiceConfigurations
    {
        /// <summary>
        /// The kill switch. When false, recording is skipped without touching either broker, so
        /// metrics can be turned off without a code change. Reads are unaffected - existing rows
        /// stay queryable.
        ///
        /// This is bound once at startup and registered as a singleton, in line with every other
        /// configuration object here, so a change takes effect when the process restarts rather
        /// than immediately. If it ever needs to be flipped mid-incident without a restart, this
        /// has to move to IOptionsMonitor and be read per call.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Whether the retention purge is permitted to delete. Separate from IsEnabled so that
        /// recording and deleting can be controlled independently, and so an environment can
        /// record metrics while never purging them.
        /// </summary>
        public bool IsPurgingAllowed { get; set; }

        /// <summary>
        /// How many days of metrics to keep. Anything older is eligible for the purge. Must be
        /// greater than zero - a zero or negative retention period would purge everything, so it
        /// is rejected rather than obeyed.
        /// </summary>
        public int RetentionPeriodInDays { get; set; }

        /// <summary>
        /// How many rows a single purge statement may delete. The purge loops until a batch comes
        /// back short, so this bounds the transaction and the lock footprint of each statement
        /// rather than the total amount deleted. Must be greater than zero - a batch size of zero
        /// would loop forever without deleting anything.
        /// </summary>
        public int PurgeBatchSize { get; set; }
    }
}
