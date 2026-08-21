// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.AuditAndMetrics;
using LondonFhirService.Core.Models.Foundations.Audits;
using LondonFhirService.Core.Models.Foundations.Metrics;

namespace LondonFhirService.Api.Tests.Acceptance.Brokers
{
    /// <summary>
    /// Silences the incidental audit and metric writes an acceptance run produces, while leaving
    /// the audit CRUD alone.
    ///
    /// Replacing the whole broker with a bare mock would be simpler, but this one broker now
    /// serves two different purposes: the operational tracing every request emits, and the data
    /// path behind the audits API. Stubbing both means the AuditsApiTests exercise a mock instead
    /// of the database and can never pass. So the logging verbs are dropped and everything the
    /// controllers actually read and write is delegated to the real broker.
    /// </summary>
    internal class QuietAuditAndMetricBroker : IAuditAndMetricBroker
    {
        private readonly IAuditAndMetricBroker auditAndMetricBroker;

        public QuietAuditAndMetricBroker(IAuditAndMetricBroker auditAndMetricBroker) =>
            this.auditAndMetricBroker = auditAndMetricBroker;

        // Dropped: background tracing, which would otherwise leave rows behind after every test.
        public ValueTask LogInformationAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId,
            CancellationToken cancellationToken = default) =>
            ValueTask.CompletedTask;

        public ValueTask LogAuditAsync(Audit audit, CancellationToken cancellationToken = default) =>
            ValueTask.CompletedTask;

        public ValueTask BulkLogAuditsAsync(
            List<Audit> audits,
            int batchSize = 10000,
            CancellationToken cancellationToken = default) =>
            ValueTask.CompletedTask;

        public ValueTask LogMetricAsync(Metric metric, CancellationToken cancellationToken = default) =>
            ValueTask.CompletedTask;

        public ValueTask LogMetricsAsync(List<Metric> metrics, CancellationToken cancellationToken = default) =>
            ValueTask.CompletedTask;

        // An access decision is a compliance record rather than tracing, so it is left to write
        // exactly as it does in production.
        public ValueTask<Audit> RecordAuditAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId,
            string logLevel = "Information",
            CancellationToken cancellationToken = default) =>
            this.auditAndMetricBroker.RecordAuditAsync(
                auditType, title, message, fileName, correlationId, logLevel, cancellationToken);

        // The audits API's own data path, delegated so the endpoints run against the database.
        public ValueTask<Audit> AddAuditAsync(Audit audit, CancellationToken cancellationToken = default) =>
            this.auditAndMetricBroker.AddAuditAsync(audit, cancellationToken);

        public ValueTask<IQueryable<Audit>> RetrieveAllAuditsAsync(CancellationToken cancellationToken = default) =>
            this.auditAndMetricBroker.RetrieveAllAuditsAsync(cancellationToken);

        public ValueTask<Audit> RetrieveAuditByIdAsync(Guid auditId, CancellationToken cancellationToken = default) =>
            this.auditAndMetricBroker.RetrieveAuditByIdAsync(auditId, cancellationToken);

        public ValueTask<Audit> ModifyAuditAsync(Audit audit, CancellationToken cancellationToken = default) =>
            this.auditAndMetricBroker.ModifyAuditAsync(audit, cancellationToken);

        public ValueTask<Audit> RemoveAuditByIdAsync(Guid auditId, CancellationToken cancellationToken = default) =>
            this.auditAndMetricBroker.RemoveAuditByIdAsync(auditId, cancellationToken);

        public ValueTask<int> PurgeMetricsOlderThanRetentionPeriodAsync(
            CancellationToken cancellationToken = default) =>
            this.auditAndMetricBroker.PurgeMetricsOlderThanRetentionPeriodAsync(cancellationToken);
    }
}
