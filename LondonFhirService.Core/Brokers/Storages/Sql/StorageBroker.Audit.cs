// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Audits;
using Microsoft.EntityFrameworkCore;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial class StorageBroker
    {
        public DbSet<Audit> Audits { get; set; }

        public virtual async ValueTask BulkInsertAuditsAsync(List<Audit> audits) =>
            await BulkInsertAsync(audits);

        public virtual async ValueTask<Audit> InsertAuditAsync(Audit audit) =>
            await InsertAsync(audit);

        public virtual async ValueTask<IQueryable<Audit>> SelectAllAuditsAsync() =>
            await SelectAllAsync<Audit>();

        public virtual async ValueTask<Audit> SelectAuditByIdAsync(Guid auditId) =>
            await SelectAsync<Audit>(auditId);

        public virtual async ValueTask<Audit> UpdateAuditAsync(Audit audit) =>
            await UpdateAsync(audit);

        public virtual async ValueTask<Audit> DeleteAuditAsync(Audit audit) =>
            await DeleteAsync(audit);
    }
}
