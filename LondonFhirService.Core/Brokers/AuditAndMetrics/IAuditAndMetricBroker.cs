// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Audits;
using LondonFhirService.Core.Models.Foundations.Metrics;

namespace LondonFhirService.Core.Brokers.AuditAndMetrics
{
    /// <summary>
    /// The utility broker any service can call to record an audit entry or a metric span.
    ///
    /// This is what the separate library buys. Recording audits and metrics is behaviour that
    /// belongs in a service, but every layer needs to call it, and a broker may not call a
    /// service. Because those services now live in LondonFhirService.Clients.AuditAndMetrics,
    /// this broker wraps an external dependency - which is what a broker is for.
    ///
    /// It stamps nothing and constructs nothing. Calling services build their own entries and
    /// stamp CreatedDate and CreatedBy when the event happens; doing it here would be business
    /// logic in a broker, and would record when the entry was submitted rather than when it
    /// occurred.
    /// </summary>
    public interface IAuditAndMetricBroker
    {
        /// <summary>
        /// Forwarded straight to the client, which builds and stamps the entry and then defers
        /// only the write. Nothing is constructed or stamped here - that is service work, and
        /// stamping at this layer would also record the submit time rather than the event time.
        /// </summary>
        ValueTask LogInformationAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Awaited, for entries that must not be lost. An access decision records who read a
        /// patient's data; losing one to a process restart is not acceptable, so this one waits
        /// for the write and surfaces failures.
        /// </summary>
        ValueTask<Audit> RecordAuditAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId,
            string logLevel = "Information",
            CancellationToken cancellationToken = default);

        ValueTask LogAuditAsync(Audit audit, CancellationToken cancellationToken = default);

        /// <summary>Dispatched to the background; see LogAuditAsync for the trade offs.</summary>
        ValueTask BulkLogAuditsAsync(
            List<Audit> audits,
            int batchSize = 10000,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Awaited, unlike the logging verbs. This is the API surface, where a caller wants the
        /// stored entity back and needs to see failures.
        /// </summary>
        ValueTask<Audit> AddAuditAsync(Audit audit, CancellationToken cancellationToken = default);

        ValueTask<IQueryable<Audit>> RetrieveAllAuditsAsync(CancellationToken cancellationToken = default);
        ValueTask<Audit> RetrieveAuditByIdAsync(Guid auditId, CancellationToken cancellationToken = default);
        ValueTask<Audit> ModifyAuditAsync(Audit audit, CancellationToken cancellationToken = default);
        ValueTask<Audit> RemoveAuditByIdAsync(Guid auditId, CancellationToken cancellationToken = default);

        /// <summary>Dispatched to the background; see LogAuditAsync for the trade offs.</summary>
        ValueTask LogMetricAsync(Metric metric, CancellationToken cancellationToken = default);

        /// <summary>Dispatched to the background; see LogAuditAsync for the trade offs.</summary>
        ValueTask LogMetricsAsync(List<Metric> metrics, CancellationToken cancellationToken = default);

        ValueTask<int> PurgeMetricsOlderThanRetentionPeriodAsync(CancellationToken cancellationToken = default);
    }
}
