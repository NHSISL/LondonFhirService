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
using IAudit = LondonFhirService.Core.Abstractions.Models.Audits.IAudit;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    /// <summary>
    /// Declared over IAudit because the contract is inherited from the storage port. The cast to
    /// the concrete entity happens here, where this broker is the one component that knows what
    /// the ORM is actually mapping.
    /// </summary>
    public partial class StorageBroker
    {
        public DbSet<Audit> Audits { get; set; }

        public IAudit CreateAudit() =>
            new Audit();

        public virtual async ValueTask BulkInsertAuditsAsync(
            List<IAudit> audits,
            CancellationToken cancellationToken = default) =>
            await BulkInsertAsync(audits.Cast<Audit>().ToList(), cancellationToken);

        public virtual async ValueTask<IAudit> InsertAuditAsync(
            IAudit audit,
            CancellationToken cancellationToken = default) =>
            await InsertAsync((Audit)audit, cancellationToken);

        public virtual async ValueTask<IQueryable<IAudit>> SelectAllAuditsAsync(
            CancellationToken cancellationToken = default) =>
            (await SelectAllAsync<Audit>(cancellationToken)).Cast<IAudit>();

        public virtual async ValueTask<IAudit> SelectAuditByIdAsync(
            Guid auditId,
            CancellationToken cancellationToken = default) =>
            await SelectAsync<Audit>(new object[] { auditId }, cancellationToken);

        public virtual async ValueTask<IAudit> UpdateAuditAsync(
            IAudit audit,
            CancellationToken cancellationToken = default) =>
            await UpdateAsync((Audit)audit, cancellationToken);

        public virtual async ValueTask<IAudit> DeleteAuditAsync(
            IAudit audit,
            CancellationToken cancellationToken = default) =>
            await DeleteAsync((Audit)audit, cancellationToken);
    }
}
