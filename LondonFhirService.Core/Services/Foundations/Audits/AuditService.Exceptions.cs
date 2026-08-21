// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
// Only the three client exceptions are imported. A blanket using would collide with
// Core's own AuditServiceException, since both layers name their categories the same way.
using AbstractionExceptions = LondonFhirService.Core.Abstractions.Models.Audits.Exceptions;
using ClientExceptions = LondonFhirService.Clients.AuditAndMetrics.Models.Audits.Exceptions;
using LondonFhirService.Core.Models.Foundations.Audits;
using LondonFhirService.Core.Models.Foundations.Audits.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.Audits
{
    internal partial class AuditService
    {
        private delegate ValueTask<Audit> ReturningAuditFunction();
        private delegate ValueTask<IQueryable<Audit>> ReturningAuditsFunction();
        private delegate ValueTask ReturningNothingFunction();

        /// <summary>
        /// The client has already categorised the failure; this localises its exceptions into
        /// this service's own so callers keep depending on Core's contract rather than the
        /// library's. Cancellation is not translated - it travels out as cancellation.
        /// </summary>
        private async ValueTask<Audit> TryCatch(ReturningAuditFunction returningAuditFunction)
        {
            try
            {
                return await returningAuditFunction();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (ClientExceptions.AuditClientNotFoundException auditClientNotFoundException)
            {
                throw await CreateAndLogNotFoundExceptionAsync(auditClientNotFoundException);
            }
            catch (ClientExceptions.AuditClientValidationException auditClientValidationException)
            {
                throw await CreateAndLogValidationExceptionAsync(auditClientValidationException);
            }
            catch (ClientExceptions.AuditClientDependencyValidationException
                auditClientDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    auditClientDependencyValidationException);
            }
            catch (ClientExceptions.AuditClientDependencyException auditClientDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(auditClientDependencyException);
            }
            catch (ClientExceptions.AuditClientServiceException auditClientServiceException)
            {
                throw await CreateAndLogServiceExceptionAsync(auditClientServiceException);
            }
            catch (Exception exception)
            {
                throw await CreateAndLogServiceExceptionAsync(exception);
            }
        }

        private async ValueTask<IQueryable<Audit>> TryCatch(ReturningAuditsFunction returningAuditsFunction)
        {
            try
            {
                return await returningAuditsFunction();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (ClientExceptions.AuditClientNotFoundException auditClientNotFoundException)
            {
                throw await CreateAndLogNotFoundExceptionAsync(auditClientNotFoundException);
            }
            catch (ClientExceptions.AuditClientValidationException auditClientValidationException)
            {
                throw await CreateAndLogValidationExceptionAsync(auditClientValidationException);
            }
            catch (ClientExceptions.AuditClientDependencyValidationException
                auditClientDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    auditClientDependencyValidationException);
            }
            catch (ClientExceptions.AuditClientDependencyException auditClientDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(auditClientDependencyException);
            }
            catch (ClientExceptions.AuditClientServiceException auditClientServiceException)
            {
                throw await CreateAndLogServiceExceptionAsync(auditClientServiceException);
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
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (ClientExceptions.AuditClientNotFoundException auditClientNotFoundException)
            {
                throw await CreateAndLogNotFoundExceptionAsync(auditClientNotFoundException);
            }
            catch (ClientExceptions.AuditClientValidationException auditClientValidationException)
            {
                throw await CreateAndLogValidationExceptionAsync(auditClientValidationException);
            }
            catch (ClientExceptions.AuditClientDependencyValidationException
                auditClientDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    auditClientDependencyValidationException);
            }
            catch (ClientExceptions.AuditClientDependencyException auditClientDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(auditClientDependencyException);
            }
            catch (ClientExceptions.AuditClientServiceException auditClientServiceException)
            {
                throw await CreateAndLogServiceExceptionAsync(auditClientServiceException);
            }
            catch (Exception exception)
            {
                throw await CreateAndLogServiceExceptionAsync(exception);
            }
        }

        /// <summary>
        /// The client's category is translated into this application's own, because the
        /// controllers dispatch on Core's categorization types to choose a status code. Without
        /// this the library's category is carried in a type the controller cannot name, and every
        /// not-found, duplicate and locked row collapses into a 400.
        /// </summary>
        private async ValueTask<AuditServiceValidationException> CreateAndLogNotFoundExceptionAsync(
            Xeption exception)
        {
            var notFoundAuditServiceException =
                new NotFoundAuditServiceException(
                    message: exception.InnerException?.Message ?? "Audit not found.");

            var auditServiceValidationException =
                new AuditServiceValidationException(
                    message: "Audit validation errors occurred, please try again.",
                    innerException: notFoundAuditServiceException);

            await this.loggingBroker.LogErrorAsync(auditServiceValidationException);

            return auditServiceValidationException;
        }

        private async ValueTask<AuditServiceDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            Xeption categorised = exception.InnerException switch
            {
                AbstractionExceptions.AlreadyExistsAuditException alreadyExists =>
                    new AlreadyExistsAuditServiceException(
                        message: "Audit with the same id already exists.",
                        innerException: alreadyExists,
                        data: alreadyExists.Data),

                AbstractionExceptions.LockedAuditException locked =>
                    new LockedAuditServiceException(
                        message: "Locked audit record exception, please try again later.",
                        innerException: locked),

                Xeption inner => inner,
                _ => exception,
            };

            var auditServiceDependencyValidationException =
                new AuditServiceDependencyValidationException(
                    message: "Audit dependency validation occurred, please try again.",
                    innerException: categorised);

            await this.loggingBroker.LogErrorAsync(auditServiceDependencyValidationException);

            return auditServiceDependencyValidationException;
        }

        private async ValueTask<AuditServiceValidationException> CreateAndLogValidationExceptionAsync(
            Xeption exception)
        {
            var auditServiceValidationException =
                new AuditServiceValidationException(
                    message: "Audit validation errors occurred, please try again.",
                    innerException: exception.InnerException as Xeption ?? exception);

            await this.loggingBroker.LogErrorAsync(auditServiceValidationException);

            return auditServiceValidationException;
        }

        private async ValueTask<AuditServiceDependencyException> CreateAndLogDependencyExceptionAsync(
            Xeption exception)
        {
            var auditServiceDependencyException =
                new AuditServiceDependencyException(
                    message: "Audit dependency error occurred, please contact support.",
                    innerException: exception.InnerException as Xeption ?? exception);

            await this.loggingBroker.LogCriticalAsync(auditServiceDependencyException);

            return auditServiceDependencyException;
        }

        private async ValueTask<AuditServiceException> CreateAndLogServiceExceptionAsync(Exception exception)
        {
            var failedAuditServiceException =
                new FailedAuditServiceException(
                    message: "Failed audit service error occurred, please contact support.",
                    innerException: exception);

            var auditServiceException =
                new AuditServiceException(
                    message: "Audit service error occurred, please contact support.",
                    innerException: failedAuditServiceException);

            await this.loggingBroker.LogErrorAsync(auditServiceException);

            return auditServiceException;
        }
    }
}
