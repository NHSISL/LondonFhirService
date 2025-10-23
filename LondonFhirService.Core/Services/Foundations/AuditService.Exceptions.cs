// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
using LondonFhirService.Core.Models.Foundations.Audits;
using LondonFhirService.Core.Models.Foundations.Audits.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.Audits
{
    public partial class AuditService
    {
        private delegate ValueTask ReturningNothingFunction();
        private delegate ValueTask<Audit> ReturningAuditFunction();
        private delegate ValueTask<IQueryable<Audit>> ReturningAuditsFunction();

        private async ValueTask<Audit> TryCatch(ReturningAuditFunction returningAuditFunction)
        {
            try
            {
                return await returningAuditFunction();
            }
            catch (NullAuditException nullAuditException)
            {
                throw await CreateAndLogValidationExceptionAsync(nullAuditException);
            }
            catch (InvalidAuditException invalidAuditException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidAuditException);
            }
            catch (SqlException sqlException)
            {
                var failedAuditStorageException =
                    new FailedAuditStorageException(
                        message: "Failed audit storage error occurred, please contact support.",
                        innerException: sqlException);

                throw await CreateAndLogCriticalDependencyExceptionAsync(failedAuditStorageException);
            }
            catch (NotFoundAuditException notFoundAuditException)
            {
                throw await CreateAndLogValidationExceptionAsync(notFoundAuditException);
            }
            catch (DuplicateKeyException duplicateKeyException)
            {
                var alreadyExistsAuditException =
                    new AlreadyExistsAuditException(
                        message: "Audit with the same Id already exists.",
                        innerException: duplicateKeyException);

                throw await CreateAndLogDependencyValidationExceptionAsync(alreadyExistsAuditException);
            }
            catch (ForeignKeyConstraintConflictException foreignKeyConstraintConflictException)
            {
                var invalidAuditReferenceException =
                    new InvalidAuditReferenceException(
                        message: "Invalid audit reference error occurred.",
                        innerException: foreignKeyConstraintConflictException);

                throw await CreateAndLogDependencyValidationExceptionAsync(invalidAuditReferenceException);
            }
            catch (DbUpdateConcurrencyException dbUpdateConcurrencyException)
            {
                var lockedAuditException =
                    new LockedAuditException(
                        message: "Locked audit record exception, please try again later",
                        innerException: dbUpdateConcurrencyException);

                throw await CreateAndLogDependencyValidationExceptionAsync(lockedAuditException);
            }
            catch (DbUpdateException databaseUpdateException)
            {
                var failedAuditStorageException =
                    new FailedAuditStorageException(
                        message: "Failed audit storage error occurred, please contact support.",
                        innerException: databaseUpdateException);

                throw await CreateAndLogDependencyExceptionAsync(failedAuditStorageException);
            }
            catch (Exception exception)
            {
                var failedAuditServiceException =
                    new FailedAuditServiceException(
                        message: "Failed audit service error occurred, please contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedAuditServiceException);
            }
        }

        private async ValueTask TryCatch(ReturningNothingFunction returningNothingFunction)
        {
            try
            {
                await returningNothingFunction();
            }
            catch (NullAuditException nullAuditException)
            {
                throw await CreateAndLogValidationExceptionAsync(nullAuditException);
            }
            catch (Exception exception)
            {
                var failedAuditServiceException =
                    new FailedAuditServiceException(
                        message: "Failed audit service error occurred, please contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedAuditServiceException);
            }
        }

        private async ValueTask<IQueryable<Audit>> TryCatch(ReturningAuditsFunction returningAuditsFunction)
        {
            try
            {
                return await returningAuditsFunction();
            }
            catch (SqlException sqlException)
            {
                var failedAuditStorageException =
                    new FailedAuditStorageException(
                        message: "Failed audit storage error occurred, please contact support.",
                        innerException: sqlException);

                throw await CreateAndLogCriticalDependencyExceptionAsync(failedAuditStorageException);
            }
            catch (Exception exception)
            {
                var failedAuditServiceException =
                    new FailedAuditServiceException(
                        message: "Failed audit service error occurred, please contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedAuditServiceException);
            }
        }

        private async ValueTask<AuditValidationException> CreateAndLogValidationExceptionAsync(Xeption exception)
        {
            var auditValidationException =
                new AuditValidationException(
                    message: "Audit validation errors occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(auditValidationException);

            return auditValidationException;
        }

        private async ValueTask<AuditDependencyException> CreateAndLogCriticalDependencyExceptionAsync(Xeption exception)
        {
            var auditDependencyException =
                new AuditDependencyException(
                    message: "Audit dependency error occurred, please contact support.",
                    innerException: exception);

            await this.loggingBroker.LogCriticalAsync(auditDependencyException);

            return auditDependencyException;
        }

        private async ValueTask<AuditDependencyValidationException> CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var auditDependencyValidationException =
                new AuditDependencyValidationException(
                    message: "Audit dependency validation occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(auditDependencyValidationException);

            return auditDependencyValidationException;
        }

        private async ValueTask<AuditDependencyException> CreateAndLogDependencyExceptionAsync(
            Xeption exception)
        {
            var auditDependencyException =
                new AuditDependencyException(
                    message: "Audit dependency error occurred, please contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(auditDependencyException);

            return auditDependencyException;
        }

        private async ValueTask<AuditServiceException> CreateAndLogServiceExceptionAsync(
            Xeption exception)
        {
            var auditServiceException =
                new AuditServiceException(
                    message: "Audit service error occurred, please contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(auditServiceException);

            return auditServiceException;
        }
    }
}