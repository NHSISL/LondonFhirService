// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Core.Models.Foundations.Metrics
{
    public class MetricServiceConfigurations
    {
        /// <summary>
        /// The kill switch. When false, recording is skipped without touching either broker, so
        /// metrics can be turned off in an incident without a deployment. Reads are unaffected -
        /// existing rows stay queryable.
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
    }
}
