// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Abstractions.Models.Audits;
using LondonFhirService.Clients.AuditAndMetrics.Models.Audits.Exceptions;
using LondonFhirService.Core.Abstractions.Models.Audits.Exceptions;
using Xeptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Services.Foundations.Audits
{
    internal partial class AuditService
    {
        private delegate ValueTask<IAudit> ReturningAuditFunction();
        private delegate ValueTask<IQueryable<IAudit>> ReturningAuditsFunction();
        private delegate ValueTask ReturningNothingFunction();

        /// <summary>
        /// As with the metric service, the storage exception types are not named here. The
        /// storage broker supplied by the hosting application translates them into
        /// FailedStorageAuditException, AlreadyExistsAuditException and LockedAuditException,
        /// which is the contract between the two.
        /// </summary>
        private async ValueTask<IAudit> TryCatch(ReturningAuditFunction returningAuditFunction)
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
            catch (NotFoundAuditException notFoundAuditException)
            {
                throw await CreateAndLogValidationExceptionAsync(notFoundAuditException);
            }
            catch (OperationCanceledException operationCanceledException)
                when (operationCanceledException.InnerException is TimeoutException)
            {
                throw await CreateAndLogTimedOutExceptionAsync(operationCanceledException);
            }
            catch (TimeoutException timeoutException)
            {
                throw await CreateAndLogTimedOutExceptionAsync(timeoutException);
            }
            catch (OperationCanceledException operationCanceledException)
            {
                throw await CreateAndLogCancelledExceptionAsync(operationCanceledException);
            }
            catch (AlreadyExistsAuditException alreadyExistsAuditException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(alreadyExistsAuditException);
            }
            catch (LockedAuditException lockedAuditException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(lockedAuditException);
            }
            catch (FailedStorageAuditException failedStorageAuditException)
            {
                throw await CreateAndLogCriticalDependencyExceptionAsync(failedStorageAuditException);
            }
            catch (Exception exception)
            {
                throw await CreateAndLogServiceExceptionAsync(exception);
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
            catch (InvalidAuditException invalidAuditException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidAuditException);
            }
            catch (OperationCanceledException operationCanceledException)
                when (operationCanceledException.InnerException is TimeoutException)
            {
                throw await CreateAndLogTimedOutExceptionAsync(operationCanceledException);
            }
            catch (TimeoutException timeoutException)
            {
                throw await CreateAndLogTimedOutExceptionAsync(timeoutException);
            }
            catch (OperationCanceledException operationCanceledException)
            {
                throw await CreateAndLogCancelledExceptionAsync(operationCanceledException);
            }
            catch (AlreadyExistsAuditException alreadyExistsAuditException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(alreadyExistsAuditException);
            }
            catch (FailedStorageAuditException failedStorageAuditException)
            {
                throw await CreateAndLogCriticalDependencyExceptionAsync(failedStorageAuditException);
            }
            catch (Exception exception)
            {
                throw await CreateAndLogServiceExceptionAsync(exception);
            }
        }

        private async ValueTask<IQueryable<IAudit>> TryCatch(ReturningAuditsFunction returningAuditsFunction)
        {
            try
            {
                return await returningAuditsFunction();
            }
            catch (OperationCanceledException operationCanceledException)
                when (operationCanceledException.InnerException is TimeoutException)
            {
                throw await CreateAndLogTimedOutExceptionAsync(operationCanceledException);
            }
            catch (TimeoutException timeoutException)
            {
                throw await CreateAndLogTimedOutExceptionAsync(timeoutException);
            }
            catch (OperationCanceledException operationCanceledException)
            {
                throw await CreateAndLogCancelledExceptionAsync(operationCanceledException);
            }
            catch (FailedStorageAuditException failedStorageAuditException)
            {
                throw await CreateAndLogCriticalDependencyExceptionAsync(failedStorageAuditException);
            }
            catch (Exception exception)
            {
                throw await CreateAndLogServiceExceptionAsync(exception);
            }
        }

        private async ValueTask<AuditDependencyException> CreateAndLogTimedOutExceptionAsync(Exception exception)
        {
            var timedOutAuditServiceException =
                new TimedOutAuditServiceException(
                    message: "Audit request timed out, please try again.",
                    innerException: exception,
                    data: exception.Data);

            return await CreateAndLogDependencyExceptionAsync(timedOutAuditServiceException);
        }

        private async ValueTask<AuditDependencyException> CreateAndLogCancelledExceptionAsync(Exception exception)
        {
            var cancelledAuditServiceException =
                new CancelledAuditServiceException(
                    message: "Audit request was cancelled, please try again.",
                    innerException: exception,
                    data: exception.Data);

            return await CreateAndLogDependencyExceptionAsync(cancelledAuditServiceException);
        }

        private async ValueTask<AuditServiceException> CreateAndLogServiceExceptionAsync(Exception exception)
        {
            var failedAuditServiceException =
                new FailedAuditServiceException(
                    message: "Failed audit service occurred, please contact support.",
                    innerException: exception,
                    data: exception.Data);

            var auditServiceException =
                new AuditServiceException(
                    message: "Audit service error occurred, contact support.",
                    innerException: failedAuditServiceException);

            await this.loggingBroker.LogErrorAsync(auditServiceException);

            return auditServiceException;
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

        private async ValueTask<AuditDependencyException> CreateAndLogCriticalDependencyExceptionAsync(
            Xeption exception)
        {
            var auditDependencyException =
                new AuditDependencyException(
                    message: "Audit dependency error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogCriticalAsync(auditDependencyException);

            return auditDependencyException;
        }

        private async ValueTask<AuditDependencyValidationException> CreateAndLogDependencyValidationExceptionAsync(
            Xeption exception)
        {
            var auditDependencyValidationException =
                new AuditDependencyValidationException(
                    message: "Audit dependency validation occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(auditDependencyValidationException);

            return auditDependencyValidationException;
        }

        private async ValueTask<AuditDependencyException> CreateAndLogDependencyExceptionAsync(Xeption exception)
        {
            var auditDependencyException =
                new AuditDependencyException(
                    message: "Audit dependency error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(auditDependencyException);

            return auditDependencyException;
        }
    }
}
