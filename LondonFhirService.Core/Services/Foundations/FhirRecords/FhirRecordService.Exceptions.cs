// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.FhirRecords
{
    public partial class FhirRecordService
    {
        private delegate ValueTask<FhirRecord> ReturningFhirRecordFunction();
        private delegate ValueTask<IQueryable<FhirRecord>> ReturningFhirRecordsFunction();

        private async ValueTask<FhirRecord> TryCatch(ReturningFhirRecordFunction returningFhirRecordFunction)
        {
            try
            {
                return await returningFhirRecordFunction();
            }
            catch (NullFhirRecordException nullFhirRecordException)
            {
                throw await CreateAndLogValidationException(nullFhirRecordException);
            }
            catch (InvalidFhirRecordException invalidFhirRecordException)
            {
                throw await CreateAndLogValidationException(invalidFhirRecordException);
            }
            catch (SqlException sqlException)
            {
                var failedFhirRecordStorageException =
                    new FailedStorageFhirRecordException(
                        message: "Failed fhirRecord storage error occurred, contact support.",
                        innerException: sqlException);

                throw await CreateAndLogCriticalDependencyException(failedFhirRecordStorageException);
            }
            catch (NotFoundFhirRecordException notFoundFhirRecordException)
            {
                throw await CreateAndLogValidationException(notFoundFhirRecordException);
            }
            catch (DuplicateKeyException duplicateKeyException)
            {
                var alreadyExistsFhirRecordException =
                    new AlreadyExistsFhirRecordException(
                        message: "FhirRecord with the same Id already exists.",
                        innerException: duplicateKeyException);

                throw await CreateAndLogDependencyValidationException(alreadyExistsFhirRecordException);
            }
            catch (ForeignKeyConstraintConflictException foreignKeyConstraintConflictException)
            {
                var invalidFhirRecordReferenceException =
                    new InvalidReferenceFhirRecordException(
                        message: "Invalid fhirRecord reference error occurred.",
                        innerException: foreignKeyConstraintConflictException);

                throw await CreateAndLogDependencyValidationException(invalidFhirRecordReferenceException);
            }
            catch (DbUpdateConcurrencyException dbUpdateConcurrencyException)
            {
                var lockedFhirRecordException =
                    new LockedFhirRecordException(
                        message: "Locked fhirRecord record exception, please try again later",
                        innerException: dbUpdateConcurrencyException);

                throw await CreateAndLogDependencyValidationException(lockedFhirRecordException);
            }
            catch (DbUpdateException databaseUpdateException)
            {
                var failedFhirRecordStorageException =
                    new FailedStorageFhirRecordException(
                        message: "Failed fhirRecord storage error occurred, contact support.",
                        innerException: databaseUpdateException);

                throw await CreateAndLogDependencyException(failedFhirRecordStorageException);
            }
            catch (Exception exception)
            {
                var failedFhirRecordServiceException =
                    new FailedFhirRecordServiceException(
                        message: "Failed fhirRecord service occurred, please contact support",
                        innerException: exception);

                throw await CreateAndLogServiceException(failedFhirRecordServiceException);
            }
        }

        private async ValueTask<IQueryable<FhirRecord>> TryCatch(
            ReturningFhirRecordsFunction returningFhirRecordsFunction)
        {
            try
            {
                return await returningFhirRecordsFunction();
            }
            catch (SqlException sqlException)
            {
                var failedFhirRecordStorageException =
                    new FailedStorageFhirRecordException(
                        message: "Failed fhirRecord storage error occurred, contact support.",
                        innerException: sqlException);

                throw await CreateAndLogCriticalDependencyException(failedFhirRecordStorageException);
            }
            catch (Exception exception)
            {
                var failedFhirRecordServiceException =
                    new FailedFhirRecordServiceException(
                        message: "Failed fhirRecord service occurred, please contact support",
                        innerException: exception);

                throw await CreateAndLogServiceException(failedFhirRecordServiceException);
            }
        }

        private async ValueTask<FhirRecordValidationException> CreateAndLogValidationException(Xeption exception)
        {
            var fhirRecordValidationException =
                new FhirRecordValidationException(
                    message: "FhirRecord validation errors occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(fhirRecordValidationException);

            return fhirRecordValidationException;
        }

        private async ValueTask<FhirRecordDependencyException> CreateAndLogCriticalDependencyException(
            Xeption exception)
        {
            var fhirRecordDependencyException =
                new FhirRecordDependencyException(
                    message: "FhirRecord dependency error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogCriticalAsync(fhirRecordDependencyException);

            return fhirRecordDependencyException;
        }

        private async ValueTask<FhirRecordDependencyValidationException> CreateAndLogDependencyValidationException(
            Xeption exception)
        {
            var fhirRecordDependencyValidationException =
                new FhirRecordDependencyValidationException(
                    message: "FhirRecord dependency validation occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(fhirRecordDependencyValidationException);

            return fhirRecordDependencyValidationException;
        }

        private async ValueTask<FhirRecordDependencyException> CreateAndLogDependencyException(
            Xeption exception)
        {
            var fhirRecordDependencyException =
                new FhirRecordDependencyException(
                    message: "FhirRecord dependency error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(fhirRecordDependencyException);

            return fhirRecordDependencyException;
        }

        private async ValueTask<FhirRecordServiceException> CreateAndLogServiceException(
            Xeption exception)
        {
            var fhirRecordServiceException =
                new FhirRecordServiceException(
                    message: "FhirRecord service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(fhirRecordServiceException);

            return fhirRecordServiceException;
        }
    }
}