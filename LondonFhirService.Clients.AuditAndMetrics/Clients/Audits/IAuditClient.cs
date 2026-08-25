// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Abstractions.Models.Audits;

namespace LondonFhirService.Clients.AuditAndMetrics.Clients.Audits
{
    /// <summary>
    /// Service exceptions are re-thrown as client exceptions so callers depend on this contract
    /// rather than on the service layer's. Cancellation is the exception: it travels out
    /// untouched, so a caller that cancels gets the cancellation it asked for.
    ///
    /// Every operation takes an IAudit the caller constructed, since the library holds no
    /// concrete implementation of the contract.
    /// </summary>
    public interface IAuditClient
    {
        /// <summary>Built, stamped and dispatched; the caller pays microseconds, not a round trip.</summary>
        ValueTask LogAuditAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId,
            string logLevel = "Information",
            CancellationToken cancellationToken = default);

        /// <summary>The awaited counterpart, for entries that must not be lost.</summary>
        ValueTask<IAudit> RecordAuditAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId,
            string logLevel = "Information",
            CancellationToken cancellationToken = default);

        /// <summary>Dispatched: the entry is validated and stamped now, written later.</summary>
        ValueTask LogAuditAsync(IAudit audit, CancellationToken cancellationToken = default);

        /// <summary>Awaited, for the API surface, which hands the stored entity back.</summary>
        ValueTask<IAudit> AddAuditAsync(IAudit audit, CancellationToken cancellationToken = default);

        /// <summary>Dispatched; see LogAuditAsync.</summary>
        ValueTask BulkLogAuditsAsync(
            List<IAudit> audits,
            int batchSize = 10000,
            CancellationToken cancellationToken = default);

        /// <summary>Awaited counterpart of BulkLogAuditsAsync.</summary>
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
