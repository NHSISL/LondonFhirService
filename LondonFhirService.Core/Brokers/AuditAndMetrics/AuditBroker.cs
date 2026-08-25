// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Abstractions.Brokers;
using LondonFhirService.Core.Abstractions.Models.Audits;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.Audits;

namespace LondonFhirService.Core.Brokers.AuditAndMetrics
{
    /// <summary>
    /// Satisfies the audit storage port the audit and metrics library declares, in the same way
    /// AuditUserBroker satisfies its identity port. Adapting an external dependency's contract to
    /// this application is broker work - this is not a foundation service, and it used to sit
    /// under Services/Foundations as AuditAndMetricsStorageService looking like one.
    ///
    /// Classifying storage failures is part of the port's contract rather than logic of its own:
    /// the library carries no ORM, so it cannot name SqlException or DbUpdateException and needs
    /// them arriving as the exceptions in Abstractions.Models.Audits.Exceptions. IStorageBroker
    /// stays a thin pass through and lets the raw exceptions escape, exactly as it does for every
    /// other entity.
    ///
    /// One entity per adapter: metrics have their own MetricBroker against their own port.
    /// The two used to share one class, which left it owning two entities and two sets of
    /// exception categories, and left the library's MetricService holding a dependency that could
    /// also write audits.
    ///
    /// Not to be confused with AuditService, which sits above the library and calls into it. This
    /// sits below it and is called by it.
    ///
    /// Writes go through the factory and get their own short lived context. That is what makes
    /// them safe to fire and forget: a write dispatched to the background outlives the request
    /// scope, and using the request's context would fail once that scope is disposed. Reads use
    /// the scoped broker, because they return an IQueryable the caller enumerates and disposing
    /// the context underneath it would kill the query.
    /// </summary>
    public partial class AuditBroker : IAuditBroker
    {
        private readonly IStorageBrokerFactory storageBrokerFactory;
        private readonly IStorageBroker storageBroker;

        public AuditBroker(
            IStorageBrokerFactory storageBrokerFactory,
            IStorageBroker storageBroker)
        {
            this.storageBrokerFactory = storageBrokerFactory;
            this.storageBroker = storageBroker;
        }

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
            TryCatchAsync(async () =>
            {
                await using IStorageBroker broker = await this.storageBrokerFactory.CreateStorageBrokerAsync();

                return await broker.InsertAuditAsync(AsAuditEntity(audit), cancellationToken);
            });

        public ValueTask BulkInsertAuditsAsync(
            List<IAudit> audits,
            CancellationToken cancellationToken = default) =>
            TryCatchAsync(async () =>
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
            TryCatchAsync(async () =>
                await this.storageBroker.SelectAllAuditsAsync(cancellationToken));

        public ValueTask<IAudit> SelectAuditByIdAsync(
            Guid auditId,
            CancellationToken cancellationToken = default) =>
            TryCatchAsync(async () =>
                await this.storageBroker.SelectAuditByIdAsync(auditId, cancellationToken));

        public ValueTask<IAudit> UpdateAuditAsync(
            IAudit audit,
            CancellationToken cancellationToken = default) =>
            TryCatchAsync(async () =>
            {
                await using IStorageBroker broker = await this.storageBrokerFactory.CreateStorageBrokerAsync();

                return await broker.UpdateAuditAsync(AsAuditEntity(audit), cancellationToken);
            });

        public ValueTask<IAudit> DeleteAuditAsync(
            IAudit audit,
            CancellationToken cancellationToken = default) =>
            TryCatchAsync(async () =>
            {
                await using IStorageBroker broker = await this.storageBrokerFactory.CreateStorageBrokerAsync();

                return await broker.DeleteAuditAsync(AsAuditEntity(audit), cancellationToken);
            });
    }
}
