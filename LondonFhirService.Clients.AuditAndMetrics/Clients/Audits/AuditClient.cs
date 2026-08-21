// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Abstractions.Models.Audits;
using LondonFhirService.Clients.AuditAndMetrics.Models.Audits.Exceptions;
using LondonFhirService.Clients.AuditAndMetrics.Services.Foundations.Audits;
using Xeptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Clients.Audits
{
    internal class AuditClient : IAuditClient
    {
        private readonly IAuditService auditService;

        public AuditClient(IAuditService auditService) =>
            this.auditService = auditService;

        public async ValueTask LogAuditAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId,
            string logLevel = "Information",
            CancellationToken cancellationToken = default) =>
            await TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                await this.auditService.LogAuditAsync(
                    auditType, title, message, fileName, correlationId, logLevel, cancellationToken);
            });

        public async ValueTask<IAudit> RecordAuditAsync(
            string auditType,
            string title,
            string message,
            string fileName,
            string correlationId,
            string logLevel = "Information",
            CancellationToken cancellationToken = default) =>
            await TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                return await this.auditService.RecordAuditAsync(
                    auditType, title, message, fileName, correlationId, logLevel, cancellationToken);
            });

        public async ValueTask<IAudit> LogAuditAsync(
            IAudit audit,
            CancellationToken cancellationToken = default) =>
            await TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                return await this.auditService.AddAuditAsync(audit, cancellationToken);
            });

        public async ValueTask BulkLogAuditsAsync(
            List<IAudit> audits,
            int batchSize = 10000,
            CancellationToken cancellationToken = default) =>
            await TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                await this.auditService.BulkAddAuditsAsync(audits, batchSize, cancellationToken);
            });

        public async ValueTask<IQueryable<IAudit>> RetrieveAllAuditsAsync(
            CancellationToken cancellationToken = default) =>
            await TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                return await this.auditService.RetrieveAllAuditsAsync(cancellationToken);
            });

        public async ValueTask<IAudit> RetrieveAuditByIdAsync(
            Guid auditId,
            CancellationToken cancellationToken = default) =>
            await TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                return await this.auditService.RetrieveAuditByIdAsync(auditId, cancellationToken);
            });

        public async ValueTask<IAudit> ModifyAuditAsync(
            IAudit audit,
            CancellationToken cancellationToken = default) =>
            await TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                return await this.auditService.ModifyAuditAsync(audit, cancellationToken);
            });

        public async ValueTask<IAudit> RemoveAuditByIdAsync(
            Guid auditId,
            CancellationToken cancellationToken = default) =>
            await TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                return await this.auditService.RemoveAuditByIdAsync(auditId, cancellationToken);
            });

        private delegate ValueTask<T> ReturningGenericFunction<T>();
        private delegate ValueTask ReturningNothingFunction();

        private static async ValueTask<T> TryCatch<T>(ReturningGenericFunction<T> returningGenericFunction)
        {
            try
            {
                return await returningGenericFunction();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (AuditValidationException auditValidationException)
            {
                throw CreateValidationException(auditValidationException);
            }
            catch (AuditDependencyValidationException auditDependencyValidationException)
            {
                throw CreateValidationException(auditDependencyValidationException);
            }
            catch (AuditDependencyException auditDependencyException)
            {
                throw CreateDependencyException(auditDependencyException);
            }
            catch (AuditServiceException auditServiceException)
            {
                throw CreateServiceException(auditServiceException);
            }
        }

        private static async ValueTask TryCatch(ReturningNothingFunction returningNothingFunction)
        {
            try
            {
                await returningNothingFunction();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (AuditValidationException auditValidationException)
            {
                throw CreateValidationException(auditValidationException);
            }
            catch (AuditDependencyValidationException auditDependencyValidationException)
            {
                throw CreateValidationException(auditDependencyValidationException);
            }
            catch (AuditDependencyException auditDependencyException)
            {
                throw CreateDependencyException(auditDependencyException);
            }
            catch (AuditServiceException auditServiceException)
            {
                throw CreateServiceException(auditServiceException);
            }
        }

        private static AuditClientValidationException CreateValidationException(Xeption exception) =>
            new AuditClientValidationException(
                message: "Audit client validation error occurred, fix errors and try again.",
                innerException: exception.InnerException as Xeption);

        private static AuditClientDependencyException CreateDependencyException(Xeption exception) =>
            new AuditClientDependencyException(
                message: "Audit client dependency error occurred, please contact support.",
                innerException: exception.InnerException as Xeption);

        private static AuditClientServiceException CreateServiceException(Xeption exception) =>
            new AuditClientServiceException(
                message: "Audit client service error occurred, fix errors and try again.",
                innerException: exception.InnerException as Xeption);
    }
}
