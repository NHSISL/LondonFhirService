// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Identifiers;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.Audits;

namespace LondonFhirService.Core.Services.Foundations.Audits
{
    public partial class AuditService : IAuditService
    {
        private readonly IStorageBrokerFactory storageBrokerFactory;
        private readonly IStorageBroker storageBroker;
        private readonly IIdentifierBroker identifierBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ISecurityAuditBroker securityAuditBroker;
        private readonly ILoggingBroker loggingBroker;

        public AuditService(
            IStorageBrokerFactory storageBrokerFactory,
            IStorageBroker storageBroker,
            IIdentifierBroker identifierBroker,
            IDateTimeBroker dateTimeBroker,
            ISecurityAuditBroker securityAuditBroker,
            ILoggingBroker loggingBroker)
        {
            this.storageBrokerFactory = storageBrokerFactory;
            this.storageBroker = storageBroker;
            this.identifierBroker = identifierBroker;
            this.dateTimeBroker = dateTimeBroker;
            this.securityAuditBroker = securityAuditBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<Audit> AddAuditAsync(
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
            DateTimeOffset dateTimeOffset = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();
            var auditUserId = await this.securityAuditBroker.GetUserIdAsync();

            Audit audit = new Audit
            {
                Id = await this.identifierBroker.GetIdentifierAsync(),
                AuditType = auditType,
                Title = title,
                Message = message,
                CorrelationId = correlationId,
                FileName = fileName,
                LogLevel = logLevel,
                CreatedBy = auditUserId ?? string.Empty,
                CreatedDate = dateTimeOffset,
                UpdatedBy = auditUserId ?? string.Empty,
                UpdatedDate = dateTimeOffset,
            };

            await ValidateAuditOnAddAsync(audit);

            await using IStorageBroker storageBroker =
                await this.storageBrokerFactory.CreateStorageBrokerAsync();

            return await storageBroker.InsertAuditAsync(audit, cancellationToken);
        });

        public ValueTask<Audit> AddAuditAsync(Audit audit, CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                Audit auditWithAddAuditApplied = await this.securityAuditBroker.ApplyAddAuditValuesAsync(audit);
                await ValidateAuditOnAddAsync(auditWithAddAuditApplied);

                await using IStorageBroker storageBroker =
                    await this.storageBrokerFactory.CreateStorageBrokerAsync();

                return await storageBroker.InsertAuditAsync(auditWithAddAuditApplied, cancellationToken);
            });

        public ValueTask BulkAddAuditsAsync(
            List<Audit> audits,
            int batchSize = 10000,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidateOnBulkAddAudits(audits);
            await BatchBulkAddAuditsAsync(audits, batchSize, cancellationToken);
        });

        public ValueTask<IQueryable<Audit>> RetrieveAllAuditsAsync(
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await this.storageBroker.SelectAllAuditsAsync(cancellationToken);
        });

        public ValueTask<Audit> RetrieveAuditByIdAsync(
            Guid auditId,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidateAuditId(auditId);

            await using IStorageBroker storageBroker =
                await this.storageBrokerFactory.CreateStorageBrokerAsync();

            Audit maybeAudit = await storageBroker
                .SelectAuditByIdAsync(auditId, cancellationToken);

            ValidateStorageAudit(maybeAudit, auditId);

            return maybeAudit;
        });

        virtual internal async ValueTask BatchBulkAddAuditsAsync(
            List<Audit> audits,
            int batchSize,
            CancellationToken cancellationToken = default)
        {
            int totalRecords = audits.Count;
            var exceptions = new List<Exception>();

            await using IStorageBroker storageBroker =
                await this.storageBrokerFactory.CreateStorageBrokerAsync();

            for (int i = 0; i < totalRecords; i += batchSize)
            {
                try
                {
                    var batch = audits.Skip(i).Take(batchSize).ToList();

                    if (batch.Count != 0)
                    {
                        List<Audit> validatedAudits = await ValidateAuditsAndAssignIdAndAuditAsync(batch);

                        await storageBroker.BulkInsertAuditsAsync(validatedAudits, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    await this.loggingBroker.LogErrorAsync(ex);
                }
            }
        }

        public ValueTask<Audit> ModifyAuditAsync(Audit audit, CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                Audit auditWithModifyAuditApplied = await this.securityAuditBroker.ApplyModifyAuditValuesAsync(audit);
                await ValidateAuditOnModifyAsync(auditWithModifyAuditApplied);

                await using IStorageBroker storageBroker =
                    await this.storageBrokerFactory.CreateStorageBrokerAsync();

                Audit maybeAudit = await storageBroker.SelectAuditByIdAsync(audit.Id, cancellationToken);
                ValidateStorageAudit(maybeAudit, audit.Id);
                ValidateAgainstStorageAuditOnModify(inputAudit: audit, storageAudit: maybeAudit);

                return await storageBroker.UpdateAuditAsync(auditWithModifyAuditApplied, cancellationToken);
            });

        public ValueTask<Audit> RemoveAuditByIdAsync(
            Guid auditId,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidateAuditId(auditId);

            await using IStorageBroker storageBroker =
                await this.storageBrokerFactory.CreateStorageBrokerAsync();

            Audit maybeAudit = await storageBroker
                .SelectAuditByIdAsync(auditId, cancellationToken);

            ValidateStorageAudit(maybeAudit, auditId);

            Audit auditWithDeleteAuditApplied =
                await securityAuditBroker.ApplyRemoveAuditValuesAsync(maybeAudit);

            Audit updatedAudit =
                await storageBroker.UpdateAuditAsync(auditWithDeleteAuditApplied, cancellationToken);

            await ValidateAgainstStorageAuditOnDeleteAsync(
                audit: updatedAudit,
                maybeAudit: auditWithDeleteAuditApplied);

            return await storageBroker.DeleteAuditAsync(updatedAudit, cancellationToken);
        });

        virtual internal async ValueTask<List<Audit>> ValidateAuditsAndAssignIdAndAuditAsync(List<Audit> audits)
        {
            List<Audit> validatedAudits = new List<Audit>();

            foreach (Audit address in audits)
            {
                try
                {
                    string currentUserId = await this.securityAuditBroker.GetUserIdAsync();
                    var currentDateTime = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();
                    address.Id = await this.identifierBroker.GetIdentifierAsync();
                    address.CreatedDate = currentDateTime;
                    address.CreatedBy = currentUserId;
                    address.UpdatedDate = address.CreatedDate;
                    address.UpdatedBy = currentUserId;
                    await ValidateAuditOnAddAsync(address);
                    validatedAudits.Add(address);
                }
                catch (Exception ex)
                {
                    await this.loggingBroker.LogErrorAsync(ex);
                }
            }

            return validatedAudits;
        }
    }
}