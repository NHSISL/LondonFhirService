// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Abstractions.Models.Audits;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.Audits;

namespace LondonFhirService.Core.Services.Foundations.AuditAndMetrics
{
    public partial class AuditAndMetricsStorageService
    {
        public IAudit CreateAudit() =>
            new Audit();

        public ValueTask<IAudit> InsertAuditAsync(
            IAudit audit,
            CancellationToken cancellationToken = default) =>
            TryCatchAuditAsync(async () =>
            {
                await using IStorageBroker broker = await this.storageBrokerFactory.CreateStorageBrokerAsync();

                return await broker.InsertAuditAsync(audit, cancellationToken);
            });

        public ValueTask BulkInsertAuditsAsync(
            List<IAudit> audits,
            CancellationToken cancellationToken = default) =>
            TryCatchAuditAsync(async () =>
            {
                await using IStorageBroker broker = await this.storageBrokerFactory.CreateStorageBrokerAsync();
                await broker.BulkInsertAuditsAsync(audits, cancellationToken);
            });

        // Reads keep the scoped broker: they hand back an IQueryable the caller enumerates, and a
        // disposed context underneath it would kill the query.
        public ValueTask<IQueryable<IAudit>> SelectAllAuditsAsync(
            CancellationToken cancellationToken = default) =>
            TryCatchAuditAsync(async () =>
                await this.storageBroker.SelectAllAuditsAsync(cancellationToken));

        public ValueTask<IAudit> SelectAuditByIdAsync(
            Guid auditId,
            CancellationToken cancellationToken = default) =>
            TryCatchAuditAsync(async () =>
                await this.storageBroker.SelectAuditByIdAsync(auditId, cancellationToken));

        public ValueTask<IAudit> UpdateAuditAsync(
            IAudit audit,
            CancellationToken cancellationToken = default) =>
            TryCatchAuditAsync(async () =>
            {
                await using IStorageBroker broker = await this.storageBrokerFactory.CreateStorageBrokerAsync();

                return await broker.UpdateAuditAsync(audit, cancellationToken);
            });

        public ValueTask<IAudit> DeleteAuditAsync(
            IAudit audit,
            CancellationToken cancellationToken = default) =>
            TryCatchAuditAsync(async () =>
            {
                await using IStorageBroker broker = await this.storageBrokerFactory.CreateStorageBrokerAsync();

                return await broker.DeleteAuditAsync(audit, cancellationToken);
            });
    }
}
