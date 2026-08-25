// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.AuditAndMetrics;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.Metrics;

namespace LondonFhirService.Core.Services.Foundations.Metrics
{
    /// <summary>
    /// Delegates to the audit and metrics broker rather than reaching for storage, exactly as
    /// AuditService does. Validation and stamping live in the library behind that broker, so what
    /// remains here is the surface this application exposes and the localisation of the client's
    /// exceptions into this service's own.
    ///
    /// There is deliberately no Validations partial: duplicating the library's rules here would
    /// let the two drift, and the library rejects the same input either way.
    ///
    /// Nothing is stamped here either. A metric span carries its own Started and Completed, taken
    /// when the work happened; stamping at this point would record when the span was submitted.
    /// </summary>
    internal partial class MetricService : IMetricService
    {
        private readonly IAuditAndMetricBroker auditAndMetricBroker;
        private readonly ILoggingBroker loggingBroker;

        public MetricService(
            IAuditAndMetricBroker auditAndMetricBroker,
            ILoggingBroker loggingBroker)
        {
            this.auditAndMetricBroker = auditAndMetricBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<Metric> AddMetricAsync(Metric metric, CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
                await this.auditAndMetricBroker.AddMetricAsync(metric, cancellationToken));

        public ValueTask LogMetricAsync(Metric metric, CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
                await this.auditAndMetricBroker.LogMetricAsync(metric, cancellationToken));

        public ValueTask LogMetricsAsync(
            List<Metric> metrics,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
                await this.auditAndMetricBroker.LogMetricsAsync(metrics, cancellationToken));

        public ValueTask<IQueryable<Metric>> RetrieveAllMetricsAsync(
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
                await this.auditAndMetricBroker.RetrieveAllMetricsAsync(cancellationToken));

        public ValueTask<Metric> RetrieveMetricByIdAsync(
            Guid metricId,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
                await this.auditAndMetricBroker.RetrieveMetricByIdAsync(metricId, cancellationToken));

        public ValueTask<Metric> RemoveMetricByIdAsync(
            Guid metricId,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
                await this.auditAndMetricBroker.RemoveMetricByIdAsync(metricId, cancellationToken));

        public ValueTask<int> PurgeMetricsOlderThanRetentionPeriodAsync(
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
                await this.auditAndMetricBroker
                    .PurgeMetricsOlderThanRetentionPeriodAsync(cancellationToken));
    }
}
