// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Abstractions.Models.Audits;

namespace LondonFhirService.Clients.AuditAndMetrics.Services.Foundations.Audits
{
    /// <summary>
    /// The audit service, replicated here so that it sits behind the client rather than inside
    /// the hosting application. Core keeps its own copy for the API surface, but that copy now
    /// delegates here through the broker instead of reaching for storage itself.
    ///
    /// Every operation takes an IAudit the caller has already constructed. The library holds no
    /// concrete implementation of the contract, so it stamps and persists what it is given
    /// rather than creating it.
    /// </summary>
    internal interface IAuditService
    {
        /// <summary>
        /// Builds the entry, stamps it, and dispatches the write to the background. The stamping
        /// happens on the caller's thread so the entry records when the event occurred and who
        /// caused it; only the database round trip is deferred, so the caller pays microseconds
        /// rather than milliseconds and the spans being measured are not inflated.
        /// </summary>
        ValueTask LogAuditAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId,
            string logLevel = "Information",
            CancellationToken cancellationToken = default);

        /// <summary>
        /// The awaited counterpart, for entries that must not be lost - an access decision is a
        /// record of who read a patient's data, and losing one to a restart is not acceptable.
        /// </summary>
        ValueTask<IAudit> RecordAuditAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId,
            string logLevel = "Information",
            CancellationToken cancellationToken = default);

        ValueTask<IAudit> AddAuditAsync(IAudit audit, CancellationToken cancellationToken = default);

        ValueTask BulkAddAuditsAsync(
            List<IAudit> audits,
            int batchSize = 10000,
            CancellationToken cancellationToken = default);

        ValueTask<IQueryable<IAudit>> RetrieveAllAuditsAsync(CancellationToken cancellationToken = default);
        ValueTask<IAudit> RetrieveAuditByIdAsync(Guid auditId, CancellationToken cancellationToken = default);
        ValueTask<IAudit> ModifyAuditAsync(IAudit audit, CancellationToken cancellationToken = default);
        ValueTask<IAudit> RemoveAuditByIdAsync(Guid auditId, CancellationToken cancellationToken = default);
    }
}
