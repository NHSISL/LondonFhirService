// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
using LondonFhirService.Core.Abstractions.Models.Audits.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LondonFhirService.Core.Brokers.AuditAndMetrics
{
    public partial class AuditBroker
    {
        private delegate ValueTask<T> ReturningGenericFunction<T>();
        private delegate ValueTask ReturningNothingFunction();

        /// <summary>
        /// Only audit categories are raised here. A DuplicateKeyException inserting an audit is
        /// an AlreadyExistsAuditException and nothing else - while the audit and metric
        /// categories lived side by side in one class, picking the wrong TryCatch reported the
        /// failure against the wrong entity and sent whoever read the log to the wrong table.
        /// Splitting the two removes the choice.
        /// </summary>
        private static async ValueTask<T> TryCatchAsync<T>(ReturningGenericFunction<T> returningFunction)
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

        private static async ValueTask TryCatchAsync(ReturningNothingFunction returningNothingFunction) =>
            await TryCatchAsync<bool>(async () =>
            {
                await returningNothingFunction();

                return true;
            });
    }
}
