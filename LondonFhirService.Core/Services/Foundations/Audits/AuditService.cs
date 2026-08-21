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
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Models.Foundations.Audits;

namespace LondonFhirService.Core.Services.Foundations.Audits
{
    /// <summary>
    /// Delegates to the audit and metrics broker rather than reaching for storage. Validation and
    /// stamping now live in the library behind that broker, so what remains here is the API
    /// surface this application exposes and the localisation of the client's exceptions into this
    /// service's own.
    ///
    /// There is deliberately no Validations partial any more: duplicating the library's rules
    /// here would let the two drift, and the library rejects the same input either way.
    /// </summary>
    internal partial class AuditService : IAuditService
    {
        private readonly IAuditAndMetricBroker auditAndMetricBroker;
        private readonly ISecurityAuditBroker securityAuditBroker;
        private readonly ILoggingBroker loggingBroker;

        public AuditService(
            IAuditAndMetricBroker auditAndMetricBroker,
            ISecurityAuditBroker securityAuditBroker,
            ILoggingBroker loggingBroker)
        {
            this.auditAndMetricBroker = auditAndMetricBroker;
            this.securityAuditBroker = securityAuditBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<Audit> AddAuditAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId,
            string logLevel = "Information",
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            // Awaited, unlike the broker's fire and forget logging verbs. This is the API
            // surface: a caller posting an audit is asking for the stored entity back, so it
            // has to wait for the write and see any failure.
            var audit = new Audit
            {
                AuditType = auditType,
                Title = title,
                Message = message,
                FileName = fileName,
                CorrelationId = correlationId,
                LogLevel = logLevel
            };

            return await this.auditAndMetricBroker.AddAuditAsync(audit, cancellationToken);
        });

        public ValueTask<Audit> AddAuditAsync(Audit audit, CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            // Stamped here, overwriting whatever the caller sent. This entity arrived on the API
            // surface from a request body, so its CreatedBy and CreatedDate are claims rather
            // than facts. The library's stamping only fills gaps, which is correct for entries
            // it builds itself but would let a caller attribute an entry to another user.
            Audit auditWithAddAuditApplied =
                await this.securityAuditBroker.ApplyAddAuditValuesAsync(audit);

            return await this.auditAndMetricBroker.AddAuditAsync(
                auditWithAddAuditApplied, cancellationToken);
        });

        public ValueTask BulkAddAuditsAsync(
            List<Audit> audits,
            int batchSize = 10000,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
            await this.auditAndMetricBroker.BulkLogAuditsAsync(audits, batchSize, cancellationToken));

        public ValueTask<IQueryable<Audit>> RetrieveAllAuditsAsync(CancellationToken cancellationToken = default) =>
            TryCatch(async () => await this.auditAndMetricBroker.RetrieveAllAuditsAsync(cancellationToken));

        public ValueTask<Audit> RetrieveAuditByIdAsync(
            Guid auditId,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
                await this.auditAndMetricBroker.RetrieveAuditByIdAsync(auditId, cancellationToken));

        public ValueTask<Audit> ModifyAuditAsync(Audit audit, CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            // UpdatedBy and UpdatedDate come from the principal, never the request body. The
            // creation stamp is protected further down, where the stored row is available to
            // compare against.
            Audit auditWithModifyAuditApplied =
                await this.securityAuditBroker.ApplyModifyAuditValuesAsync(audit);

            return await this.auditAndMetricBroker.ModifyAuditAsync(
                auditWithModifyAuditApplied, cancellationToken);
        });

        public ValueTask<Audit> RemoveAuditByIdAsync(
            Guid auditId,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
                await this.auditAndMetricBroker.RemoveAuditByIdAsync(auditId, cancellationToken));
    }
}
