// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.FhirRecordDifferences
{
    public partial class FhirRecordDifferenceService
    {
        private delegate ValueTask<FhirRecordDifference> ReturningFhirRecordDifferenceFunction();
        private delegate ValueTask<IQueryable<FhirRecordDifference>> ReturningFhirRecordDifferencesFunction();

        private async ValueTask<FhirRecordDifference> TryCatch(ReturningFhirRecordDifferenceFunction returningFhirRecordDifferenceFunction)
        {
            try
            {
                return await returningFhirRecordDifferenceFunction();
            }
            catch (NullFhirRecordDifferenceException nullFhirRecordDifferenceException)
            {
                throw await CreateAndLogValidationException(nullFhirRecordDifferenceException);
            }
            catch (InvalidFhirRecordDifferenceException invalidFhirRecordDifferenceException)
            {
                throw await CreateAndLogValidationException(invalidFhirRecordDifferenceException);
            }
            catch (SqlException sqlException)
            {
                var failedFhirRecordDifferenceStorageException =
                    new FailedStorageFhirRecordDifferenceException(
                        message: "Failed fhirRecordDifference storage error occurred, contact support.",
                        innerException: sqlException);

                throw await CreateAndLogCriticalDependencyException(failedFhirRecordDifferenceStorageException);
            }
            catch (NotFoundFhirRecordDifferenceException notFoundFhirRecordDifferenceException)
            {
                throw await CreateAndLogValidationException(notFoundFhirRecordDifferenceException);
            }
            catch (DuplicateKeyException duplicateKeyException)
            {
                var alreadyExistsFhirRecordDifferenceException =
                    new AlreadyExistsFhirRecordDifferenceException(
                        message: "FhirRecordDifference with the same Id already exists.",
                        innerException: duplicateKeyException);

                throw await CreateAndLogDependencyValidationException(alreadyExistsFhirRecordDifferenceException);
            }
            catch (ForeignKeyConstraintConflictException foreignKeyConstraintConflictException)
            {
                var invalidFhirRecordDifferenceReferenceException =
                    new InvalidReferenceFhirRecordDifferenceException(
                        message: "Invalid fhirRecordDifference reference error occurred.",
                        innerException: foreignKeyConstraintConflictException);

                throw await CreateAndLogDependencyValidationException(invalidFhirRecordDifferenceReferenceException);
            }
            catch (DbUpdateConcurrencyException dbUpdateConcurrencyException)
            {
                var lockedFhirRecordDifferenceException =
                    new LockedFhirRecordDifferenceException(
                        message: "Locked fhirRecordDifference record exception, please try again later",
                        innerException: dbUpdateConcurrencyException);

                throw await CreateAndLogDependencyValidationException(lockedFhirRecordDifferenceException);
            }
            catch (DbUpdateException databaseUpdateException)
            {
                var failedFhirRecordDifferenceStorageException =
                    new FailedStorageFhirRecordDifferenceException(
                        message: "Failed fhirRecordDifference storage error occurred, contact support.",
                        innerException: databaseUpdateException);

                throw await CreateAndLogDependencyException(failedFhirRecordDifferenceStorageException);
            }
            catch (Exception exception)
            {
                var failedFhirRecordDifferenceServiceException =
                    new FailedFhirRecordDifferenceServiceException(
                        message: "Failed fhirRecordDifference service occurred, please contact support",
                        innerException: exception);

                throw await CreateAndLogServiceException(failedFhirRecordDifferenceServiceException);
            }
        }

        private async ValueTask<IQueryable<FhirRecordDifference>> TryCatch(
            ReturningFhirRecordDifferencesFunction returningFhirRecordDifferencesFunction)
        {
            try
            {
                return await returningFhirRecordDifferencesFunction();
            }
            catch (SqlException sqlException)
            {
                var failedFhirRecordDifferenceStorageException =
                    new FailedStorageFhirRecordDifferenceException(
                        message: "Failed fhirRecordDifference storage error occurred, contact support.",
                        innerException: sqlException);

                throw await CreateAndLogCriticalDependencyException(failedFhirRecordDifferenceStorageException);
            }
            catch (Exception exception)
            {
                var failedFhirRecordDifferenceServiceException =
                    new FailedFhirRecordDifferenceServiceException(
                        message: "Failed fhirRecordDifference service occurred, please contact support",
                        innerException: exception);

                throw await CreateAndLogServiceException(failedFhirRecordDifferenceServiceException);
            }
        }

        private async ValueTask<FhirRecordDifferenceValidationException> CreateAndLogValidationException(Xeption exception)
        {
            var fhirRecordDifferenceValidationException =
                new FhirRecordDifferenceValidationException(
                    message: "FhirRecordDifference validation errors occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(fhirRecordDifferenceValidationException);

            return fhirRecordDifferenceValidationException;
        }

        private async ValueTask<FhirRecordDifferenceDependencyException> CreateAndLogCriticalDependencyException(
            Xeption exception)
        {
            var fhirRecordDifferenceDependencyException =
                new FhirRecordDifferenceDependencyException(
                    message: "FhirRecordDifference dependency error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogCriticalAsync(fhirRecordDifferenceDependencyException);

            return fhirRecordDifferenceDependencyException;
        }

        private async ValueTask<FhirRecordDifferenceDependencyValidationException> CreateAndLogDependencyValidationException(
            Xeption exception)
        {
            var fhirRecordDifferenceDependencyValidationException =
                new FhirRecordDifferenceDependencyValidationException(
                    message: "FhirRecordDifference dependency validation occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(fhirRecordDifferenceDependencyValidationException);

            return fhirRecordDifferenceDependencyValidationException;
        }

        private async ValueTask<FhirRecordDifferenceDependencyException> CreateAndLogDependencyException(
            Xeption exception)
        {
            var fhirRecordDifferenceDependencyException =
                new FhirRecordDifferenceDependencyException(
                    message: "FhirRecordDifference dependency error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(fhirRecordDifferenceDependencyException);

            return fhirRecordDifferenceDependencyException;
        }

        private async ValueTask<FhirRecordDifferenceServiceException> CreateAndLogServiceException(
            Xeption exception)
        {
            var fhirRecordDifferenceServiceException =
                new FhirRecordDifferenceServiceException(
                    message: "FhirRecordDifference service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(fhirRecordDifferenceServiceException);

            return fhirRecordDifferenceServiceException;
        }
    }
}