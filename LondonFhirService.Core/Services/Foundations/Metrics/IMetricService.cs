// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Metrics;

namespace LondonFhirService.Core.Services.Foundations.Metrics
{
    /// <summary>
    /// The metric half of what this application exposes over the audit and metrics library, and
    /// the counterpart to IAuditService. Both reach the library through the one
    /// IAuditAndMetricBroker.
    ///
    /// Callers used to reach the broker directly for metrics, which left metric failures
    /// arriving at controllers and workers in the library's exception types while audit failures
    /// arrived in this application's. This service closes that gap.
    /// </summary>
    public interface IMetricService
    {
        /// <summary>Dispatched to the background: validated and stamped now, written later.</summary>
        ValueTask AddMetricAsync(Metric metric, CancellationToken cancellationToken = default);

        /// <summary>Dispatched; see AddMetricAsync.</summary>
        ValueTask AddMetricsAsync(List<Metric> metrics, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes metrics past the configured retention period and returns how many rows went.
        /// Whether anything is deleted at all is decided by the library's IsPurgingAllowed and
        /// RetentionPeriodInDays.
        /// </summary>
        ValueTask<int> PurgeMetricsOlderThanRetentionPeriodAsync(CancellationToken cancellationToken = default);
    }
}
