// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Clients.AuditAndMetrics.Brokers.DateTimes;
using LondonFhirService.Clients.AuditAndMetrics.Brokers.Identifiers;
using LondonFhirService.Clients.AuditAndMetrics.Brokers.Loggings;
using LondonFhirService.Core.Abstractions.Brokers;
using LondonFhirService.Core.Abstractions.Models.Audits;

namespace LondonFhirService.Clients.AuditAndMetrics.Services.Foundations.Audits
{
    internal partial class AuditService : IAuditService
    {
        private readonly IAuditAndMetricsStorageBroker storageBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly IIdentifierBroker identifierBroker;
        private readonly ILoggingBroker loggingBroker;
        private readonly IAuditUserBroker auditUserBroker;

        public AuditService(
            IAuditAndMetricsStorageBroker storageBroker,
            IDateTimeBroker dateTimeBroker,
            IIdentifierBroker identifierBroker,
            ILoggingBroker loggingBroker,
            IAuditUserBroker auditUserBroker)
        {
            this.storageBroker = storageBroker;
            this.dateTimeBroker = dateTimeBroker;
            this.identifierBroker = identifierBroker;
            this.loggingBroker = loggingBroker;
            this.auditUserBroker = auditUserBroker;
        }

        public ValueTask LogAuditAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId,
            string logLevel = "Information",
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            IAudit audit = await BuildStampedAuditAsync(
                auditType, title, message, fileName, correlationId, logLevel);

            ValidateAuditOnAdd(audit);

            // Only the write is deferred. Everything above ran on the caller's thread, so the
            // entry carries the time and user of the moment it happened rather than of whenever
            // the thread pool got round to it.
            FireAndForget(async () =>
                await this.storageBroker.InsertAuditAsync(audit, cancellationToken));
        });

        public ValueTask<IAudit> RecordAuditAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId,
            string logLevel = "Information",
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            IAudit audit = await BuildStampedAuditAsync(
                auditType, title, message, fileName, correlationId, logLevel);

            ValidateAuditOnAdd(audit);

            return await this.storageBroker.InsertAuditAsync(audit, cancellationToken);
        });

        public ValueTask<IAudit> AddAuditAsync(IAudit audit, CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidateAuditIsNotNull(audit);
            await StampAsync(audit);
            ValidateAuditOnAdd(audit);

            return await this.storageBroker.InsertAuditAsync(audit, cancellationToken);
        });

        public ValueTask BulkAddAuditsAsync(
            List<IAudit> audits,
            int batchSize = 10000,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidateAuditsIsNotNull(audits);
            ValidateBatchSize(batchSize);

            if (audits.Count == 0)
            {
                return;
            }

            // One clock reading for the whole batch, so entries flushed together are not spread
            // across the boundary of a tick.
            DateTimeOffset currentDateTime = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

            int unstampedCount = 0;
            var unstampedTypes = new List<string>();
            var unstampedCorrelationIds = new List<string>();

            foreach (IAudit audit in audits)
            {
                ValidateAuditIsNotNull(audit);

                if (await StampAsync(audit, currentDateTime))
                {
                    unstampedCount++;
                    unstampedTypes.Add(audit.AuditType);
                    unstampedCorrelationIds.Add(audit.CorrelationId);
                }

                ValidateAuditOnAdd(audit);
            }

            // One warning for the batch rather than one per entry, so a caller that stamps
            // nothing produces a single actionable line instead of thousands.
            //
            // The audit types and correlation ids are included because they identify the calling
            // service far better than the stack trace does: these writes are dispatched to the
            // background, so by the time this runs the original caller's frames are gone and the
            // trace shows only the thread pool and this library. The trace is still carried in
            // case the caller invoked the service directly.
            if (unstampedCount > 0)
            {
                string affectedTypes = string.Join(
                    ", ",
                    audits.Where(audit => unstampedTypes.Contains(audit.AuditType))
                        .Select(audit => audit.AuditType)
                        .Distinct());

                await this.loggingBroker.LogWarningAsync(
                    $"{unstampedCount} of {audits.Count} audit entries arrived without a creation " +
                    "timestamp and were stamped with the write time instead, so their order " +
                    "within the batch is not reliable. The calling service should stamp " +
                    $"CreatedDate when the entry is created. Audit types affected: {affectedTypes}. " +
                    $"Correlation ids: {string.Join(", ", unstampedCorrelationIds.Distinct())}. " +
                    $"Stack trace: {Environment.StackTrace}");
            }

            for (int index = 0; index < audits.Count; index += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                List<IAudit> batch = audits.Skip(index).Take(batchSize).ToList();
                await this.storageBroker.BulkInsertAuditsAsync(batch, cancellationToken);
            }
        });

        public ValueTask<IQueryable<IAudit>> RetrieveAllAuditsAsync(CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await this.storageBroker.SelectAllAuditsAsync(cancellationToken);
        });

        public ValueTask<IAudit> RetrieveAuditByIdAsync(
            Guid auditId,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidateAuditId(auditId);
            IAudit maybeAudit = await this.storageBroker.SelectAuditByIdAsync(auditId, cancellationToken);
            ValidateStorageAudit(maybeAudit, auditId);

            return maybeAudit;
        });

        public ValueTask<IAudit> ModifyAuditAsync(IAudit audit, CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidateAuditIsNotNull(audit);
            audit.UpdatedDate = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();
            ValidateAuditOnModify(audit);
            IAudit maybeAudit = await this.storageBroker.SelectAuditByIdAsync(audit.Id, cancellationToken);
            ValidateStorageAudit(maybeAudit, audit.Id);
            ValidateAgainstStorageAuditOnModify(inputAudit: audit, storageAudit: maybeAudit);

            return await this.storageBroker.UpdateAuditAsync(audit, cancellationToken);
        });

        public ValueTask<IAudit> RemoveAuditByIdAsync(
            Guid auditId,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidateAuditId(auditId);
            IAudit maybeAudit = await this.storageBroker.SelectAuditByIdAsync(auditId, cancellationToken);
            ValidateStorageAudit(maybeAudit, auditId);

            return await this.storageBroker.DeleteAuditAsync(maybeAudit, cancellationToken);
        });

        /// <summary>
        /// Builds an entry stamped with the moment it happened and the caller who caused it.
        /// Both are read here, synchronously, because the entry is about now - reading them after
        /// the write is dispatched would record when the row was written instead, and two entries
        /// a millisecond apart could then be stamped in either order.
        /// </summary>
        private async ValueTask<IAudit> BuildStampedAuditAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId,
            string logLevel)
        {
            DateTimeOffset createdDate = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();
            string currentUserId = await this.auditUserBroker.GetCurrentUserIdAsync();

            IAudit audit = this.storageBroker.CreateAudit();
            audit.Id = await this.identifierBroker.GetIdentifierAsync();
            audit.AuditType = auditType;
            audit.Title = title;
            audit.Message = message;
            audit.FileName = fileName;
            audit.CorrelationId = correlationId;
            audit.LogLevel = logLevel;
            audit.CreatedBy = currentUserId;
            audit.UpdatedBy = currentUserId;
            audit.CreatedDate = createdDate;
            audit.UpdatedDate = createdDate;

            return audit;
        }

        /// <summary>
        /// Dispatches the write and returns. Failures are logged rather than thrown: a caller
        /// recording an audit entry has no way to react to the recording failing, and throwing
        /// would turn an observability problem into an outage.
        /// </summary>
        private void FireAndForget(Func<Task> work) =>
            _ = Task.Run(async () =>
            {
                try
                {
                    await work();
                }
                catch (Exception exception)
                {
                    await this.loggingBroker.LogErrorAsync(exception);
                }
            });

        /// <summary>
        /// Fills in what the caller is not expected to know: the identity and the timestamps. The
        /// caller supplies the entry because only it holds a concrete implementation of IAudit.
        /// </summary>
        private async ValueTask<bool> StampAsync(IAudit audit, DateTimeOffset? currentDateTime = null)
        {
            DateTimeOffset stampedDate =
                currentDateTime ?? await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

            if (audit.Id == Guid.Empty)
            {
                audit.Id = await this.identifierBroker.GetIdentifierAsync();
            }

            // Only filled when the caller left it unset. A caller recording an event knows when
            // it happened; this service only knows when it got round to writing it. Overwriting
            // here would replace event time with write time, and for a backgrounded write those
            // are not the same thing - two entries logged a millisecond apart could be stamped
            // in either order depending on thread scheduling.
            bool fellBackToWriteTime = audit.CreatedDate == default;

            if (fellBackToWriteTime)
            {
                audit.CreatedDate = stampedDate;
            }

            if (audit.UpdatedDate == default)
            {
                audit.UpdatedDate = audit.CreatedDate;
            }

            // The user is filled the same way - gaps only. A caller that recorded who caused the
            // event is authoritative; this service can only see who the request belongs to now.
            if (string.IsNullOrWhiteSpace(audit.CreatedBy) || string.IsNullOrWhiteSpace(audit.UpdatedBy))
            {
                string currentUserId = await this.auditUserBroker.GetCurrentUserIdAsync();

                audit.CreatedBy = string.IsNullOrWhiteSpace(audit.CreatedBy)
                    ? currentUserId
                    : audit.CreatedBy;

                audit.UpdatedBy = string.IsNullOrWhiteSpace(audit.UpdatedBy)
                    ? currentUserId
                    : audit.UpdatedBy;
            }

            return fellBackToWriteTime;
        }
    }
}
