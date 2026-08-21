// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Audits;
using Microsoft.EntityFrameworkCore;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial class StorageBroker
    {
        public DbSet<Audit> Audits { get; set; }

        public virtual async ValueTask BulkInsertAuditsAsync(
            List<Audit> audits,
            CancellationToken cancellationToken = default) =>
            await BulkInsertAsync(audits, cancellationToken);

        public virtual async ValueTask<Audit> InsertAuditAsync(
            Audit audit,
            CancellationToken cancellationToken = default) =>
            await InsertAsync(audit, cancellationToken);

        public virtual async ValueTask<IQueryable<Audit>> SelectAllAuditsAsync(
            CancellationToken cancellationToken = default) =>
            await SelectAllAsync<Audit>(cancellationToken);

        public virtual async ValueTask<Audit> SelectAuditByIdAsync(
            Guid auditId,
            CancellationToken cancellationToken = default) =>
            await SelectAsync<Audit>(cancellationToken, auditId);

        public virtual async ValueTask<Audit> UpdateAuditAsync(
            Audit audit,
            CancellationToken cancellationToken = default) =>
            await UpdateAsync(audit, cancellationToken);

        public virtual async ValueTask<Audit> DeleteAuditAsync(
            Audit audit,
            CancellationToken cancellationToken = default) =>
            await DeleteAsync(audit, cancellationToken);
    }
}
