// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
using LondonFhirService.Core.Models.Foundations.PdsDatas;
using LondonFhirService.Core.Models.Foundations.PdsDatas.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.PdsDatas
{
    public partial class PdsDataService
    {
        private delegate ValueTask<PdsData> ReturningPdsDataFunction();
        private delegate ValueTask<bool> ReturningBooleanFunction();
        private delegate ValueTask<IQueryable<PdsData>> ReturningPdsDatasFunction();

        private async ValueTask<PdsData> TryCatch(ReturningPdsDataFunction returningPdsDataFunction)
        {
            try
            {
                return await returningPdsDataFunction();
            }
            catch (NullPdsDataServiceException nullPdsDataException)
            {
                throw CreateAndLogValidationException(nullPdsDataException);
            }
            catch (InvalidPdsDataServiceException invalidPdsDataException)
            {
                throw CreateAndLogValidationException(invalidPdsDataException);
            }
            catch (SqlException sqlException)
            {
                var failedStoragePdsDataException =
                    new FailedStoragePdsDataServiceException(
                        message: "Failed pdsData storage error occurred, contact support.",
                        innerException: sqlException);

                throw CreateAndLogCriticalDependencyException(failedStoragePdsDataException);
            }
            catch (NotFoundPdsDataServiceException notFoundPdsDataException)
            {
                throw CreateAndLogValidationException(notFoundPdsDataException);
            }
            catch (DuplicateKeyException duplicateKeyException)
            {
                var alreadyExistsPdsDataException =
                    new AlreadyExistsPdsDataServiceException(
                        message: "PdsData with the same Id already exists.",
                        innerException: duplicateKeyException,
                        data: duplicateKeyException.Data);

                throw CreateAndLogDependencyValidationException(alreadyExistsPdsDataException);
            }
            catch (ForeignKeyConstraintConflictException foreignKeyConstraintConflictException)
            {
                var invalidPdsDataReferenceException =
                    new InvalidReferencePdsDataServiceException(
                        message: "Invalid pdsData reference error occurred.",
                        innerException: foreignKeyConstraintConflictException);

                throw CreateAndLogDependencyValidationException(invalidPdsDataReferenceException);
            }
            catch (DbUpdateConcurrencyException dbUpdateConcurrencyException)
            {
                var lockedPdsDataException =
                    new LockedPdsDataServiceException(
                        message: "Locked pdsData record exception, please try again later",
                        innerException: dbUpdateConcurrencyException);

                throw CreateAndLogDependencyValidationException(lockedPdsDataException);
            }
            catch (DbUpdateException databaseUpdateException)
            {
                var failedOperationPdsDataException =
                    new FailedOperationPdsDataServiceException(
                        message: "Failed pdsData operation error occurred, contact support.",
                        innerException: databaseUpdateException);

                throw CreateAndLogDependencyException(failedOperationPdsDataException);
            }
            catch (Exception exception)
            {
                var failedPdsDataServiceException =
                    new FailedPdsDataServiceException(
                        message: "Failed pdsData service occurred, please contact support",
                        innerException: exception);

                throw CreateAndLogServiceException(failedPdsDataServiceException);
            }
        }

        private async ValueTask<bool> TryCatch(ReturningBooleanFunction returningBooleanFunction)
        {
            try
            {
                return await returningBooleanFunction();
            }
            catch (InvalidPdsDataServiceException invalidPdsDataException)
            {
                throw CreateAndLogValidationException(invalidPdsDataException);
            }
            catch (SqlException sqlException)
            {
                var failedStoragePdsDataException =
                    new FailedStoragePdsDataServiceException(
                        message: "Failed pdsData storage error occurred, contact support.",
                        innerException: sqlException);

                throw CreateAndLogCriticalDependencyException(failedStoragePdsDataException);
            }
            catch (Exception exception)
            {
                var failedPdsDataServiceException =
                    new FailedPdsDataServiceException(
                        message: "Failed pdsData service occurred, please contact support",
                        innerException: exception);

                throw CreateAndLogServiceException(failedPdsDataServiceException);
            }
        }

        private async ValueTask<IQueryable<PdsData>> TryCatch(ReturningPdsDatasFunction returningPdsDatasFunction)
        {
            try
            {
                return await returningPdsDatasFunction();
            }
            catch (SqlException sqlException)
            {
                var failedStoragePdsDataException =
                    new FailedStoragePdsDataServiceException(
                        message: "Failed pdsData storage error occurred, contact support.",
                        innerException: sqlException);

                throw CreateAndLogCriticalDependencyException(failedStoragePdsDataException);
            }
            catch (Exception exception)
            {
                var failedPdsDataServiceException =
                    new FailedPdsDataServiceException(
                        message: "Failed pdsData service occurred, please contact support",
                        innerException: exception);

                throw CreateAndLogServiceException(failedPdsDataServiceException);
            }
        }

        private PdsDataServiceValidationException CreateAndLogValidationException(Xeption exception)
        {
            var pdsDataValidationException =
                new PdsDataServiceValidationException(
                    message: "PdsData validation error occurred, please fix errors and try again.",
                    innerException: exception);

            this.loggingBroker.LogErrorAsync(pdsDataValidationException);

            return pdsDataValidationException;
        }

        private PdsDataServiceDependencyException CreateAndLogCriticalDependencyException(Xeption exception)
        {
            var pdsDataDependencyException =
                new PdsDataServiceDependencyException(
                    message: "PdsData dependency error occurred, contact support.",
                    innerException: exception);

            this.loggingBroker.LogCriticalAsync(pdsDataDependencyException);

            return pdsDataDependencyException;
        }

        private PdsDataServiceDependencyValidationException CreateAndLogDependencyValidationException(Xeption exception)
        {
            var pdsDataDependencyValidationException =
                new PdsDataServiceDependencyValidationException(
                    message: "PdsData dependency validation occurred, please try again.",
                    innerException: exception);

            this.loggingBroker.LogErrorAsync(pdsDataDependencyValidationException);

            return pdsDataDependencyValidationException;
        }

        private PdsDataServiceDependencyException CreateAndLogDependencyException(Xeption exception)
        {
            var pdsDataDependencyException =
                new PdsDataServiceDependencyException(
                    message: "PdsData dependency error occurred, contact support.",
                    innerException: exception);

            this.loggingBroker.LogErrorAsync(pdsDataDependencyException);

            return pdsDataDependencyException;
        }

        private PdsDataServiceException CreateAndLogServiceException(Xeption exception)
        {
            var pdsDataServiceException =
                new PdsDataServiceException(
                    message: "PdsData service error occurred, contact support.",
                    innerException: exception);

            this.loggingBroker.LogErrorAsync(pdsDataServiceException);

            return pdsDataServiceException;
        }
    }
}