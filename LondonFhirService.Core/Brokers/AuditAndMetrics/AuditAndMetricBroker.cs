// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Clients.AuditAndMetrics.Clients;
using LondonFhirService.Core.Models.Foundations.Audits;
using LondonFhirService.Core.Models.Foundations.Metrics;
using IAudit = LondonFhirService.Core.Abstractions.Models.Audits.IAudit;
using IMetric = LondonFhirService.Core.Abstractions.Models.Metrics.IMetric;

namespace LondonFhirService.Core.Brokers.AuditAndMetrics
{
    /// <summary>
    /// A pass through to the audit and metrics client. It casts at the boundary so callers work
    /// in Audit and Metric, and does nothing else - no stamping, no construction, no validation,
    /// and no control flow.
    ///
    /// Deferring the write used to happen here, which made this more than a wrapper. It now
    /// happens in the library's services, where orchestration belongs, and the pair of verbs on
    /// this interface simply says which of the two the caller wants.
    /// </summary>
    public class AuditAndMetricBroker : IAuditAndMetricBroker
    {
        private readonly IAuditAndMetricsClient auditAndMetricsClient;

        public AuditAndMetricBroker(IAuditAndMetricsClient auditAndMetricsClient) =>
            this.auditAndMetricsClient = auditAndMetricsClient;

        public async ValueTask LogInformationAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId,
            CancellationToken cancellationToken = default) =>
            await this.auditAndMetricsClient.AuditClient.LogAuditAsync(
                auditType,
                title,
                message,
                fileName,
                correlationId,
                logLevel: "Information",
                cancellationToken);

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
            await this.auditAndMetricsClient.AuditClient.LogAuditAsync(audit, cancellationToken);

        public async ValueTask BulkLogAuditsAsync(
            List<Audit> audits,
            int batchSize = 10000,
            CancellationToken cancellationToken = default) =>
            await this.auditAndMetricsClient.AuditClient.BulkLogAuditsAsync(
                audits.Cast<IAudit>().ToList(), batchSize, cancellationToken);

        public async ValueTask<Audit> AddAuditAsync(
            Audit audit,
            CancellationToken cancellationToken = default) =>
            (Audit)await this.auditAndMetricsClient.AuditClient.AddAuditAsync(audit, cancellationToken);

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
            await this.auditAndMetricsClient.MetricClient.LogMetricAsync(metric, cancellationToken);

        public async ValueTask LogMetricsAsync(
            List<Metric> metrics,
            CancellationToken cancellationToken = default) =>
            await this.auditAndMetricsClient.MetricClient.LogMetricsAsync(
                metrics.Cast<IMetric>().ToList(), cancellationToken);

        public async ValueTask<int> PurgeMetricsOlderThanRetentionPeriodAsync(
            CancellationToken cancellationToken = default) =>
            await this.auditAndMetricsClient.MetricClient
                .PurgeMetricsOlderThanRetentionPeriodAsync(cancellationToken);
    }
}
