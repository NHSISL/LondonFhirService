// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
using LondonFhirService.Core.Models.Foundations.OdsDatas;
using LondonFhirService.Core.Models.Foundations.OdsDatas.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.OdsDatas
{
    public partial class OdsDataService
    {
        private delegate ValueTask<OdsData> ReturningOdsDataFunction();
        private delegate ValueTask<List<OdsData>> ReturningOdsDataListFunction();
        private delegate ValueTask<IQueryable<OdsData>> ReturningOdsDatasFunction();

        private async ValueTask<OdsData> TryCatch(ReturningOdsDataFunction returningOdsDataFunction)
        {
            try
            {
                return await returningOdsDataFunction();
            }
            catch (NullOdsDataServiceException nullOdsDataException)
            {
                throw CreateAndLogValidationException(nullOdsDataException);
            }
            catch (InvalidOdsDataServiceException invalidOdsDataException)
            {
                throw CreateAndLogValidationException(invalidOdsDataException);
            }
            catch (SqlException sqlException)
            {
                var failedStorageOdsDataException =
                    new FailedStorageOdsDataServiceException(
                        message: "Failed odsData storage error occurred, contact support.",
                        innerException: sqlException);

                throw CreateAndLogCriticalDependencyException(failedStorageOdsDataException);
            }
            catch (NotFoundOdsDataServiceException notFoundOdsDataException)
            {
                throw CreateAndLogValidationException(notFoundOdsDataException);
            }
            catch (DuplicateKeyException duplicateKeyException)
            {
                var alreadyExistsOdsDataException =
                    new AlreadyExistsOdsDataServiceException(
                        message: "OdsData with the same Id already exists.",
                        innerException: duplicateKeyException,
                        data: duplicateKeyException.Data);

                throw CreateAndLogDependencyValidationException(alreadyExistsOdsDataException);
            }
            catch (ForeignKeyConstraintConflictException foreignKeyConstraintConflictException)
            {
                var invalidOdsDataReferenceException =
                    new InvalidReferenceOdsDataServiceException(
                        message: "Invalid odsData reference error occurred.",
                        innerException: foreignKeyConstraintConflictException);

                throw CreateAndLogDependencyValidationException(invalidOdsDataReferenceException);
            }
            catch (DbUpdateConcurrencyException dbUpdateConcurrencyException)
            {
                var lockedOdsDataException =
                    new LockedOdsDataServiceException(
                        message: "Locked odsData record exception, please try again later",
                        innerException: dbUpdateConcurrencyException);

                throw CreateAndLogDependencyValidationException(lockedOdsDataException);
            }
            catch (DbUpdateException databaseUpdateException)
            {
                var failedOperationOdsDataException =
                    new FailedOperationOdsDataServiceException(
                        message: "Failed odsData operation error occurred, contact support.",
                        innerException: databaseUpdateException);

                throw CreateAndLogDependencyException(failedOperationOdsDataException);
            }
            catch (Exception exception)
            {
                var failedOdsDataServiceException =
                    new FailedOdsDataServiceException(
                        message: "Failed odsData service occurred, please contact support",
                        innerException: exception);

                throw CreateAndLogServiceException(failedOdsDataServiceException);
            }
        }

        private async ValueTask<IQueryable<OdsData>> TryCatch(ReturningOdsDatasFunction returningOdsDatasFunction)
        {
            try
            {
                return await returningOdsDatasFunction();
            }
            catch (SqlException sqlException)
            {
                var failedStorageOdsDataException =
                    new FailedStorageOdsDataServiceException(
                        message: "Failed odsData storage error occurred, contact support.",
                        innerException: sqlException);

                throw CreateAndLogCriticalDependencyException(failedStorageOdsDataException);
            }
            catch (Exception exception)
            {
                var failedOdsDataServiceException =
                    new FailedOdsDataServiceException(
                        message: "Failed odsData service occurred, please contact support",
                        innerException: exception);

                throw CreateAndLogServiceException(failedOdsDataServiceException);
            }
        }

        private async ValueTask<List<OdsData>> TryCatch(ReturningOdsDataListFunction returningOdsDataListFunction)
        {
            try
            {
                return await returningOdsDataListFunction();
            }
            catch (NullOdsDataServiceException nullOdsDataException)
            {
                throw CreateAndLogValidationException(nullOdsDataException);
            }
            catch (InvalidOdsDataServiceException invalidOdsDataException)
            {
                throw CreateAndLogValidationException(invalidOdsDataException);
            }
            catch (NotFoundOdsDataServiceException notFoundOdsDataException)
            {
                throw CreateAndLogValidationException(notFoundOdsDataException);
            }
            catch (SqlException sqlException)
            {
                var failedStorageOdsDataException =
                    new FailedStorageOdsDataServiceException(
                        message: "Failed odsData storage error occurred, contact support.",
                        innerException: sqlException);

                throw CreateAndLogCriticalDependencyException(failedStorageOdsDataException);
            }
            catch (Exception exception)
            {
                var failedOdsDataServiceException =
                    new FailedOdsDataServiceException(
                        message: "Failed odsData service occurred, please contact support",
                        innerException: exception);

                throw CreateAndLogServiceException(failedOdsDataServiceException);
            }
        }

        private OdsDataServiceValidationException CreateAndLogValidationException(Xeption exception)
        {
            var odsDataValidationException =
                new OdsDataServiceValidationException(
                    message: "OdsData validation error occurred, please fix errors and try again.",
                    innerException: exception);

            this.loggingBroker.LogErrorAsync(odsDataValidationException);

            return odsDataValidationException;
        }

        private OdsDataServiceDependencyException CreateAndLogCriticalDependencyException(Xeption exception)
        {
            var odsDataDependencyException =
                new OdsDataServiceDependencyException(
                    message: "OdsData dependency error occurred, contact support.",
                    innerException: exception);

            this.loggingBroker.LogCriticalAsync(odsDataDependencyException);

            return odsDataDependencyException;
        }

        private OdsDataServiceDependencyValidationException CreateAndLogDependencyValidationException(Xeption exception)
        {
            var odsDataDependencyValidationException =
                new OdsDataServiceDependencyValidationException(
                    message: "OdsData dependency validation occurred, please try again.",
                    innerException: exception);

            this.loggingBroker.LogErrorAsync(odsDataDependencyValidationException);

            return odsDataDependencyValidationException;
        }

        private OdsDataServiceDependencyException CreateAndLogDependencyException(Xeption exception)
        {
            var odsDataDependencyException =
                new OdsDataServiceDependencyException(
                    message: "OdsData dependency error occurred, contact support.",
                    innerException: exception);

            this.loggingBroker.LogErrorAsync(odsDataDependencyException);

            return odsDataDependencyException;
        }

        private OdsDataServiceException CreateAndLogServiceException(Xeption exception)
        {
            var odsDataServiceException =
                new OdsDataServiceException(
                    message: "OdsData service error occurred, contact support.",
                    innerException: exception);

            this.loggingBroker.LogErrorAsync(odsDataServiceException);

            return odsDataServiceException;
        }
    }
}