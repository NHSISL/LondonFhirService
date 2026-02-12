// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Identifiers;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.Audits;
using Microsoft.EntityFrameworkCore;

namespace LondonFhirService.Core.Services.Foundations.Audits
{
    public partial class AuditService : IAuditService
    {
        private readonly IDbContextFactory<StorageBroker> storageBrokerFactory;
        private readonly IIdentifierBroker identifierBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ISecurityAuditBroker securityAuditBroker;
        private readonly ILoggingBroker loggingBroker;

        public AuditService(
            IDbContextFactory<StorageBroker> storageBrokerFactory,
            IIdentifierBroker identifierBroker,
            IDateTimeBroker dateTimeBroker,
            ISecurityAuditBroker securityAuditBroker,
            ILoggingBroker loggingBroker)
        {
            this.storageBrokerFactory = storageBrokerFactory;
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
            string logLevel = "Information") =>
        TryCatch(async () =>
        {
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

            await using StorageBroker storageBroker =
                await this.storageBrokerFactory.CreateDbContextAsync();

            return await storageBroker.InsertAuditAsync(audit);
        });

        public ValueTask<Audit> AddAuditAsync(Audit audit) =>
            TryCatch(async () =>
            {
                Audit auditWithAddAuditApplied = await this.securityAuditBroker.ApplyAddAuditValuesAsync(audit);
                await ValidateAuditOnAddAsync(auditWithAddAuditApplied);

                await using StorageBroker storageBroker =
                    await this.storageBrokerFactory.CreateDbContextAsync();

                return await storageBroker.InsertAuditAsync(auditWithAddAuditApplied);
            });

        public ValueTask BulkAddAuditsAsync(List<Audit> audits, int batchSize = 10000) =>
        TryCatch(async () =>
        {
            ValidateOnBulkAddAudits(audits);
            await BatchBulkAddAuditsAsync(audits, batchSize);
        });

        public ValueTask<IQueryable<Audit>> RetrieveAllAuditsAsync() =>
        TryCatch(async () =>
        {
            StorageBroker storageBroker =
                await this.storageBrokerFactory.CreateDbContextAsync();

            return await storageBroker.SelectAllAuditsAsync();
        });

        public ValueTask<Audit> RetrieveAuditByIdAsync(Guid auditId) =>
        TryCatch(async () =>
        {
            ValidateAuditId(auditId);

            await using StorageBroker storageBroker =
                await this.storageBrokerFactory.CreateDbContextAsync();

            Audit maybeAudit = await storageBroker
                .SelectAuditByIdAsync(auditId);

            ValidateStorageAudit(maybeAudit, auditId);

            return maybeAudit;
        });

        virtual internal async ValueTask BatchBulkAddAuditsAsync(List<Audit> audits, int batchSize)
        {
            int totalRecords = audits.Count;
            var exceptions = new List<Exception>();

            await using StorageBroker storageBroker =
                await this.storageBrokerFactory.CreateDbContextAsync();

            for (int i = 0; i < totalRecords; i += batchSize)
            {
                try
                {
                    var batch = audits.Skip(i).Take(batchSize).ToList();

                    if (batch.Count != 0)
                    {
                        List<Audit> validatedAudits = await ValidateAuditsAndAssignIdAndAuditAsync(batch);

                        await storageBroker.BulkInsertAuditsAsync(validatedAudits);
                    }
                }
                catch (Exception ex)
                {
                    await this.loggingBroker.LogErrorAsync(ex);
                }
            }
        }

        public ValueTask<Audit> ModifyAuditAsync(Audit audit) =>
            TryCatch(async () =>
            {
                Audit auditWithModifyAuditApplied = await this.securityAuditBroker.ApplyModifyAuditValuesAsync(audit);
                await ValidateAuditOnModifyAsync(auditWithModifyAuditApplied);

                await using StorageBroker storageBroker =
                    await this.storageBrokerFactory.CreateDbContextAsync();

                Audit maybeAudit = await storageBroker.SelectAuditByIdAsync(audit.Id);
                ValidateStorageAudit(maybeAudit, audit.Id);
                ValidateAgainstStorageAuditOnModify(inputAudit: audit, storageAudit: maybeAudit);

                return await storageBroker.UpdateAuditAsync(auditWithModifyAuditApplied);
            });

        public ValueTask<Audit> RemoveAuditByIdAsync(Guid auditId) =>
        TryCatch(async () =>
        {
            ValidateAuditId(auditId);

            await using StorageBroker storageBroker =
                await this.storageBrokerFactory.CreateDbContextAsync();

            Audit maybeAudit = await storageBroker
                .SelectAuditByIdAsync(auditId);

            ValidateStorageAudit(maybeAudit, auditId);

            Audit auditWithDeleteAuditApplied =
                await securityAuditBroker.ApplyRemoveAuditValuesAsync(maybeAudit);

            Audit updatedAudit =
                await storageBroker.UpdateAuditAsync(auditWithDeleteAuditApplied);

            await ValidateAgainstStorageAuditOnDeleteAsync(
                audit: updatedAudit,
                maybeAudit: auditWithDeleteAuditApplied);

            return await storageBroker.DeleteAuditAsync(updatedAudit);
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