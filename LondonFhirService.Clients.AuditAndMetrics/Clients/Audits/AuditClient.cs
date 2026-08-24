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

        public async ValueTask LogAuditAsync(
            IAudit audit,
            CancellationToken cancellationToken = default) =>
            await TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                await this.auditService.LogAuditAsync(audit, cancellationToken);
            });

        public async ValueTask<IAudit> AddAuditAsync(
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
                await this.auditService.BulkLogAuditsAsync(audits, batchSize, cancellationToken);
            });

        public async ValueTask BulkAddAuditsAsync(
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
                throw CreateDependencyValidationException(auditDependencyValidationException);
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
                throw CreateDependencyValidationException(auditDependencyValidationException);
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

        /// <summary>
        /// A not-found is surfaced as its own type rather than as a generic validation failure.
        /// The categorized exception behind it is internal to this library, so a caller has no
        /// other way to tell "no such audit" from "the audit you sent is invalid" - and those
        /// are a 404 and a 400.
        /// </summary>
        private static Xeption CreateValidationException(Xeption exception)
        {
            var innerException = exception.InnerException as Xeption;

            if (innerException is NotFoundAuditException)
            {
                return new AuditClientNotFoundException(
                    message: "Audit not found.",
                    innerException: innerException);
            }

            return new AuditClientValidationException(
                message: "Audit client validation error occurred, fix errors and try again.",
                innerException: innerException);
        }

        /// <summary>
        /// The inner exception is one of the storage contract types from Core.Abstractions, which
        /// are public, so the caller can tell an already-exists from a locked row.
        /// </summary>
        private static AuditClientDependencyValidationException CreateDependencyValidationException(
            Xeption exception) =>
            new AuditClientDependencyValidationException(
                message: "Audit client dependency validation error occurred, fix errors and try again.",
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
