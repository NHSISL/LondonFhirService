// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Audits;

namespace LondonFhirService.Core.Services.Foundations
{
    public interface IAuditService
    {
        ValueTask<Audit> AddAuditAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId,
            string logLevel = "Information");

        ValueTask<Audit> AddAuditAsync(Audit audit);
        ValueTask BulkAddAuditsAsync(List<Audit> audits, int batchSize = 10000);
        ValueTask<IQueryable<Audit>> RetrieveAllAuditsAsync();
        ValueTask<Audit> RetrieveAuditByIdAsync(Guid auditId);
        ValueTask<Audit> ModifyAuditAsync(Audit audit);
        ValueTask<Audit> RemoveAuditByIdAsync(Guid auditId);

    }
}