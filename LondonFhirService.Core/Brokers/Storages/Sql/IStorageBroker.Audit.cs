// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Audits;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial interface IStorageBroker
    {
        ValueTask BulkInsertAuditsAsync(List<Audit> audits, CancellationToken cancellationToken = default);
        ValueTask<Audit> InsertAuditAsync(Audit audit, CancellationToken cancellationToken = default);
        ValueTask<IQueryable<Audit>> SelectAllAuditsAsync(CancellationToken cancellationToken = default);
        ValueTask<Audit> SelectAuditByIdAsync(Guid auditId, CancellationToken cancellationToken = default);
        ValueTask<Audit> UpdateAuditAsync(Audit audit, CancellationToken cancellationToken = default);
        ValueTask<Audit> DeleteAuditAsync(Audit audit, CancellationToken cancellationToken = default);
    }
}
