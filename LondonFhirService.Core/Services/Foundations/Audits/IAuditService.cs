// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Audits;

namespace LondonFhirService.Core.Services.Foundations.Audits
{
    public interface IAuditService
    {
        ValueTask<Audit> AddAuditAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId,
            string logLevel = "Information",
            CancellationToken cancellationToken = default);

        ValueTask<Audit> AddAuditAsync(Audit audit, CancellationToken cancellationToken = default);

        ValueTask BulkAddAuditsAsync(
            List<Audit> audits,
            int batchSize = 10000,
            CancellationToken cancellationToken = default);

        ValueTask<IQueryable<Audit>> RetrieveAllAuditsAsync(CancellationToken cancellationToken = default);
        ValueTask<Audit> RetrieveAuditByIdAsync(Guid auditId, CancellationToken cancellationToken = default);
        ValueTask<Audit> ModifyAuditAsync(Audit audit, CancellationToken cancellationToken = default);
        ValueTask<Audit> RemoveAuditByIdAsync(Guid auditId, CancellationToken cancellationToken = default);

    }
}