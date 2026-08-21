// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Clients.AuditAndMetrics.Clients;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.Audits;
using LondonFhirService.Core.Models.Foundations.Metrics;
using IAudit = LondonFhirService.Core.Abstractions.Models.Audits.IAudit;
using IMetric = LondonFhirService.Core.Abstractions.Models.Metrics.IMetric;

namespace LondonFhirService.Core.Brokers.AuditAndMetrics
{
    /// <summary>
    /// A pass through to the audit and metrics client. It casts at the boundary so callers work
    /// in Audit and Metric, and dispatches the logging verbs so they cost the caller nothing.
    /// Nothing else: no stamping, no construction, no validation. Those belong to the services on
    /// either side of it.
    /// </summary>
    public class AuditAndMetricBroker : IAuditAndMetricBroker
    {
        private readonly IAuditAndMetricsClient auditAndMetricsClient;
        private readonly ILoggingBroker loggingBroker;

        public AuditAndMetricBroker(
            IAuditAndMetricsClient auditAndMetricsClient,
            ILoggingBroker loggingBroker)
        {
            this.auditAndMetricsClient = auditAndMetricsClient;
            this.loggingBroker = loggingBroker;
        }

        public async ValueTask LogInformationAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId,
            CancellationToken cancellationToken = default) =>
            await this.auditAndMetricsClient.AuditClient.LogAuditAsync(
                auditType, title, message, fileName, correlationId, "Information", cancellationToken);

        public async ValueTask<Audit> RecordAuditAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId,
            string logLevel = "Information",
            CancellationToken cancellationToken = default) =>
            (Audit)await this.auditAndMetricsClient.AuditClient.RecordAuditAsync(
                auditType, title, message, fileName, correlationId, logLevel, cancellationToken);

        public async ValueTask LogAuditAsync(Audit audit, CancellationToken cancellationToken = default) =>
            FireAndForget(async () =>
                await this.auditAndMetricsClient.AuditClient.LogAuditAsync(audit, cancellationToken));

        public async ValueTask BulkLogAuditsAsync(
            List<Audit> audits,
            int batchSize = 10000,
            CancellationToken cancellationToken = default) =>
            FireAndForget(async () =>
                await this.auditAndMetricsClient.AuditClient.BulkLogAuditsAsync(
                    audits.Cast<IAudit>().ToList(), batchSize, cancellationToken));

        public async ValueTask<Audit> AddAuditAsync(
            Audit audit,
            CancellationToken cancellationToken = default) =>
            (Audit)await this.auditAndMetricsClient.AuditClient.LogAuditAsync(audit, cancellationToken);

        public async ValueTask<IQueryable<Audit>> RetrieveAllAuditsAsync(
            CancellationToken cancellationToken = default) =>
            (await this.auditAndMetricsClient.AuditClient.RetrieveAllAuditsAsync(cancellationToken))
                .Cast<Audit>();

        public async ValueTask<Audit> RetrieveAuditByIdAsync(
            Guid auditId,
            CancellationToken cancellationToken = default) =>
            (Audit)await this.auditAndMetricsClient.AuditClient
                .RetrieveAuditByIdAsync(auditId, cancellationToken);

        public async ValueTask<Audit> ModifyAuditAsync(
            Audit audit,
            CancellationToken cancellationToken = default) =>
            (Audit)await this.auditAndMetricsClient.AuditClient
                .ModifyAuditAsync(audit, cancellationToken);

        public async ValueTask<Audit> RemoveAuditByIdAsync(
            Guid auditId,
            CancellationToken cancellationToken = default) =>
            (Audit)await this.auditAndMetricsClient.AuditClient
                .RemoveAuditByIdAsync(auditId, cancellationToken);

        public async ValueTask LogMetricAsync(
            Metric metric,
            CancellationToken cancellationToken = default) =>
            FireAndForget(async () =>
                await this.auditAndMetricsClient.MetricClient.AddMetricAsync(metric, cancellationToken));

        public async ValueTask LogMetricsAsync(
            List<Metric> metrics,
            CancellationToken cancellationToken = default) =>
            FireAndForget(async () =>
                await this.auditAndMetricsClient.MetricClient.AddMetricsAsync(
                    metrics.Cast<IMetric>().ToList(), cancellationToken));

        public async ValueTask<int> PurgeMetricsOlderThanRetentionPeriodAsync(
            CancellationToken cancellationToken = default) =>
            await this.auditAndMetricsClient.MetricClient
                .PurgeMetricsOlderThanRetentionPeriodAsync(cancellationToken);

        /// <summary>
        /// Dispatches the write and returns without waiting. Failures are logged rather than
        /// thrown: a caller recording an audit entry has no meaningful way to react to the
        /// recording failing, and throwing would turn an observability problem into an outage.
        /// </summary>
        private void FireAndForget(Func<Task> work) =>
            _ = Task.Run(async () =>
            {
                try
                {
                    await work();
                }
                catch (Exception exception)
                {
                    await this.loggingBroker.LogErrorAsync(exception);
                }
            });
    }
}
