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

namespace LondonFhirService.Core.Brokers.AuditAndMetrics
{
    public partial class AuditAndMetricStorageBroker
    {
        public IAudit CreateAudit() =>
            new Audit();

        /// <summary>
        /// The port accepts any IAudit, because the library that calls it holds no concrete
        /// implementation and must not need one. Storage only knows the Audit entity, so anything
        /// else is copied onto one rather than cast - a cast would throw InvalidCastException on a
        /// perfectly legitimate call.
        ///
        /// This lives in the service and not the storage broker on purpose: a broker is a pass
        /// through, and mapping between a contract and an entity is not pass through work.
        /// </summary>
        private static Audit AsAuditEntity(IAudit audit)
        {
            if (audit is Audit auditEntity)
            {
                return auditEntity;
            }

            return new Audit
            {
                Id = audit.Id,
                CorrelationId = audit.CorrelationId,
                AuditType = audit.AuditType,
                Title = audit.Title,
                Message = audit.Message,
                FileName = audit.FileName,
                LogLevel = audit.LogLevel,
                CreatedBy = audit.CreatedBy,
                CreatedDate = audit.CreatedDate,
                UpdatedBy = audit.UpdatedBy,
                UpdatedDate = audit.UpdatedDate
            };
        }

        public ValueTask<IAudit> InsertAuditAsync(
            IAudit audit,
            CancellationToken cancellationToken = default) =>
            TryCatchAuditAsync(async () =>
            {
                await using IStorageBroker broker = await this.storageBrokerFactory.CreateStorageBrokerAsync();

                return await broker.InsertAuditAsync(AsAuditEntity(audit), cancellationToken);
            });

        public ValueTask BulkInsertAuditsAsync(
            List<IAudit> audits,
            CancellationToken cancellationToken = default) =>
            TryCatchAuditAsync(async () =>
            {
                await using IStorageBroker broker = await this.storageBrokerFactory.CreateStorageBrokerAsync();
                await broker.BulkInsertAuditsAsync(
                    audits.Select(AsAuditEntity).Cast<IAudit>().ToList(),
                    cancellationToken);
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

                return await broker.UpdateAuditAsync(AsAuditEntity(audit), cancellationToken);
            });

        public ValueTask<IAudit> DeleteAuditAsync(
            IAudit audit,
            CancellationToken cancellationToken = default) =>
            TryCatchAuditAsync(async () =>
            {
                await using IStorageBroker broker = await this.storageBrokerFactory.CreateStorageBrokerAsync();

                return await broker.DeleteAuditAsync(AsAuditEntity(audit), cancellationToken);
            });
    }
}
