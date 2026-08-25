// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
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
    ///
    /// There is no Modify verb. A metric is a span of work that already happened, so the table is
    /// append-only and correcting history is not something this surface offers.
    /// </summary>
    public interface IMetricService
    {
        /// <summary>
        /// Awaited, unlike the logging verbs below. This is the API surface: a caller posting a
        /// span is asking for the stored entity back, so it has to wait for the write and see any
        /// failure.
        /// </summary>
        ValueTask<Metric> AddMetricAsync(Metric metric, CancellationToken cancellationToken = default);

        /// <summary>Dispatched to the background: validated and stamped now, written later.</summary>
        ValueTask LogMetricAsync(Metric metric, CancellationToken cancellationToken = default);

        /// <summary>Dispatched; see LogMetricAsync.</summary>
        ValueTask LogMetricsAsync(List<Metric> metrics, CancellationToken cancellationToken = default);

        ValueTask<IQueryable<Metric>> RetrieveAllMetricsAsync(CancellationToken cancellationToken = default);
        ValueTask<Metric> RetrieveMetricByIdAsync(Guid metricId, CancellationToken cancellationToken = default);
        ValueTask<Metric> RemoveMetricByIdAsync(Guid metricId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes metrics past the configured retention period and returns how many rows went.
        /// Whether anything is deleted at all is decided by the library's IsPurgingAllowed and
        /// RetentionPeriodInDays.
        /// </summary>
        ValueTask<int> PurgeMetricsOlderThanRetentionPeriodAsync(CancellationToken cancellationToken = default);
    }
}
