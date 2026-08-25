// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
using LondonFhirService.Core.Abstractions.Models.Audits.Exceptions;
using LondonFhirService.Core.Abstractions.Models.Metrics.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LondonFhirService.Core.Services.Foundations.AuditAndMetrics
{
    internal partial class AuditAndMetricsStorageService
    {
        private delegate ValueTask<T> ReturningGenericFunction<T>();
        private delegate ValueTask ReturningNothingFunction();

        /// <summary>
        /// Audit and metric failures are categorised separately. A DuplicateKeyException raised
        /// inserting an audit is an AlreadyExistsAuditException, not a metric one - reporting it
        /// as the wrong entity would send whoever reads the log to the wrong table.
        /// </summary>
        private static async ValueTask<T> TryCatchAuditAsync<T>(ReturningGenericFunction<T> returningFunction)
        {
            try
            {
                return await returningFunction();
            }
            catch (SqlException sqlException)
            {
                throw new FailedStorageAuditException(
                    message: "Failed audit storage error occurred, contact support.",
                    innerException: sqlException,
                    data: sqlException.Data);
            }
            catch (DuplicateKeyException duplicateKeyException)
            {
                throw new AlreadyExistsAuditException(
                    message: "Audit with the same Id already exists.",
                    innerException: duplicateKeyException,
                    data: duplicateKeyException.Data);
            }
            catch (DbUpdateConcurrencyException dbUpdateConcurrencyException)
            {
                throw new LockedAuditException(
                    message: "Locked audit record exception, please try again later.",
                    innerException: dbUpdateConcurrencyException,
                    data: dbUpdateConcurrencyException.Data);
            }
            catch (DbUpdateException dbUpdateException)
            {
                throw new FailedStorageAuditException(
                    message: "Failed audit storage error occurred, contact support.",
                    innerException: dbUpdateException,
                    data: dbUpdateException.Data);
            }
        }

        private static async ValueTask TryCatchAuditAsync(ReturningNothingFunction returningNothingFunction) =>
            await TryCatchAuditAsync<bool>(async () =>
            {
                await returningNothingFunction();

                return true;
            });

        private static async ValueTask<T> TryCatchMetricAsync<T>(ReturningGenericFunction<T> returningFunction)
        {
            try
            {
                return await returningFunction();
            }
            catch (SqlException sqlException)
            {
                throw new FailedStorageMetricException(
                    message: "Failed metric storage error occurred, contact support.",
                    innerException: sqlException,
                    data: sqlException.Data);
            }
            catch (DuplicateKeyException duplicateKeyException)
            {
                throw new AlreadyExistsMetricException(
                    message: "Metric with the same Id already exists.",
                    innerException: duplicateKeyException,
                    data: duplicateKeyException.Data);
            }
            catch (ForeignKeyConstraintConflictException foreignKeyConstraintConflictException)
            {
                throw new InvalidReferenceMetricException(
                    message: "Invalid metric reference error occurred.",
                    innerException: foreignKeyConstraintConflictException,
                    data: foreignKeyConstraintConflictException.Data);
            }
            catch (DbUpdateConcurrencyException dbUpdateConcurrencyException)
            {
                throw new LockedMetricException(
                    message: "Locked metric record exception, please try again later.",
                    innerException: dbUpdateConcurrencyException,
                    data: dbUpdateConcurrencyException.Data);
            }
            catch (DbUpdateException dbUpdateException)
            {
                throw new FailedStorageMetricException(
                    message: "Failed metric storage error occurred, contact support.",
                    innerException: dbUpdateException,
                    data: dbUpdateException.Data);
            }
        }

        private static async ValueTask TryCatchMetricAsync(ReturningNothingFunction returningNothingFunction) =>
            await TryCatchMetricAsync<bool>(async () =>
            {
                await returningNothingFunction();

                return true;
            });
    }
}
