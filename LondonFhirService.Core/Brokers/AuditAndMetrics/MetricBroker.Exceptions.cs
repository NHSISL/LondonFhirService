// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
using LondonFhirService.Core.Abstractions.Models.Metrics.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LondonFhirService.Core.Brokers.AuditAndMetrics
{
    public partial class MetricBroker
    {
        private delegate ValueTask<T> ReturningGenericFunction<T>();
        private delegate ValueTask ReturningNothingFunction();

        /// <summary>
        /// Only metric categories are raised here; see AuditBroker.TryCatchAsync for why
        /// the two entities no longer share a class.
        ///
        /// Metrics carry one category audits do not: a foreign key conflict, because a metric can
        /// name a parent span.
        /// </summary>
        private static async ValueTask<T> TryCatchAsync<T>(ReturningGenericFunction<T> returningFunction)
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

        private static async ValueTask TryCatchAsync(ReturningNothingFunction returningNothingFunction) =>
            await TryCatchAsync<bool>(async () =>
            {
                await returningNothingFunction();

                return true;
            });
    }
}
