// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Audits;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial interface IStorageBroker
    {
        ValueTask BulkInsertAuditsAsync(List<Audit> audits);
        ValueTask<Audit> InsertAuditAsync(Audit audit);
        ValueTask<IQueryable<Audit>> SelectAllAuditsAsync();
        ValueTask<Audit> SelectAuditByIdAsync(Guid auditId);
        ValueTask<Audit> UpdateAuditAsync(Audit audit);
        ValueTask<Audit> DeleteAuditAsync(Audit audit);
    }
}
