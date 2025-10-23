// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.ConsumerAccesses
{
    public partial class ConsumerAccessService
    {
        private delegate ValueTask<ConsumerAccess> ReturningConsumerAccessFunction();
        private delegate ValueTask<IQueryable<ConsumerAccess>> ReturningConsumerAccessesFunction();
        private delegate ValueTask<List<string>> ReturningStringListFunction();

        private async ValueTask<ConsumerAccess> TryCatch(ReturningConsumerAccessFunction returningConsumerAccessFunction)
        {
            try
            {
                return await returningConsumerAccessFunction();
            }
            catch (NullConsumerAccessException nullConsumerAccessException)
            {
                throw await CreateAndLogValidationExceptionAsync(nullConsumerAccessException);
            }
            catch (InvalidConsumerAccessException invalidConsumerAccessException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidConsumerAccessException);
            }
            catch (NotFoundConsumerAccessException notFoundConsumerAccessException)
            {
                throw await CreateAndLogValidationExceptionAsync(notFoundConsumerAccessException);
            }
            catch (SqlException sqlException)
            {
                var failedStorageConsumerAccessException = new FailedStorageConsumerAccessException(
                    message: "Failed user access storage error occurred, contact support.",
                    innerException: sqlException);

                throw await CreateAndLogCriticalDependencyExceptionAsync(failedStorageConsumerAccessException);
            }
            catch (DuplicateKeyException duplicateKeyException)
            {
                var alreadyExistsConsumerAccessException =
                    new AlreadyExistsConsumerAccessException(
                        message: "ConsumerAccess already exists error occurred.",
                        innerException: duplicateKeyException,
                        data: duplicateKeyException.Data);

                throw await CreateAndLogDependencyValidationExceptionAsync(alreadyExistsConsumerAccessException);
            }
            catch (DbUpdateConcurrencyException dbUpdateConcurrencyException)
            {
                var lockedConsumerAccessException =
                    new LockedConsumerAccessException(
                        message: "Locked user access record error occurred, please try again.",
                        innerException: dbUpdateConcurrencyException);

                throw await CreateAndLogDependencyValidationExceptionAsync(lockedConsumerAccessException);
            }
            catch (DbUpdateException dbUpdateException)
            {
                var failedOperationConsumerAccessException =
                    new FailedOperationConsumerAccessException(
                        message: "Failed operation user access error occurred, contact support.",
                        innerException: dbUpdateException);

                throw await CreateAndLogDependencyExceptionAsync(failedOperationConsumerAccessException);
            }
            catch (Exception exception)
            {
                var failedServiceConsumerAccessException =
                    new FailedServiceConsumerAccessException(
                        message: "Failed service user access error occurred, contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedServiceConsumerAccessException);
            }
        }

        private async ValueTask<IQueryable<ConsumerAccess>> TryCatch(
            ReturningConsumerAccessesFunction returningConsumerAccessesFunction)
        {
            try
            {
                return await returningConsumerAccessesFunction();
            }
            catch (SqlException sqlException)
            {
                var failedStorageConsumerAccessException = new FailedStorageConsumerAccessException(
                    message: "Failed user access storage error occurred, contact support.",
                    innerException: sqlException);

                throw await CreateAndLogCriticalDependencyExceptionAsync(failedStorageConsumerAccessException);
            }
            catch (Exception exception)
            {
                var failedServiceConsumerAccessException =
                    new FailedServiceConsumerAccessException(
                        message: "Failed service user access error occurred, contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedServiceConsumerAccessException);
            }
        }

        private async ValueTask<List<string>> TryCatch(ReturningStringListFunction returningStringListFunction)
        {
            try
            {
                return await returningStringListFunction();
            }
            catch (InvalidConsumerAccessException invalidConsumerAccessException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidConsumerAccessException);
            }
            catch (SqlException sqlException)
            {
                var failedStorageConsumerAccessException = new FailedStorageConsumerAccessException(
                    message: "Failed user access storage error occurred, contact support.",
                    innerException: sqlException);

                throw await CreateAndLogCriticalDependencyExceptionAsync(failedStorageConsumerAccessException);
            }
            catch (Exception exception)
            {
                var failedServiceConsumerAccessException =
                    new FailedServiceConsumerAccessException(
                        message: "Failed service user access error occurred, contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedServiceConsumerAccessException);
            }
        }

        private async ValueTask<ConsumerAccessValidationException> CreateAndLogValidationExceptionAsync(
            Xeption exception)
        {
            var consumerAccessValidationException = new ConsumerAccessValidationException(
                message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                innerException: exception);

            await this.loggingBroker.LogErrorAsync(consumerAccessValidationException);

            return consumerAccessValidationException;
        }

        private async ValueTask<ConsumerAccessDependencyException> CreateAndLogCriticalDependencyExceptionAsync(
            Xeption exception)
        {
            var consumerAccessDependencyException = new ConsumerAccessDependencyException(
                message: "ConsumerAccess dependency error occurred, contact support.",
                innerException: exception);

            await this.loggingBroker.LogCriticalAsync(consumerAccessDependencyException);

            return consumerAccessDependencyException;
        }

        private async ValueTask<ConsumerAccessDependencyValidationException> CreateAndLogDependencyValidationExceptionAsync(
            Xeption exception)
        {
            var consumerAccessDependencyValidationException = new ConsumerAccessDependencyValidationException(
                message: "ConsumerAccess dependency validation error occurred, fix errors and try again.",
                innerException: exception);

            await this.loggingBroker.LogErrorAsync(consumerAccessDependencyValidationException);

            return consumerAccessDependencyValidationException;
        }

        private async ValueTask<ConsumerAccessDependencyException> CreateAndLogDependencyExceptionAsync(
            Xeption exception)
        {
            var consumerAccessDependencyException = new ConsumerAccessDependencyException(
                message: "ConsumerAccess dependency error occurred, contact support.",
                innerException: exception);

            await this.loggingBroker.LogErrorAsync(consumerAccessDependencyException);

            return consumerAccessDependencyException;
        }

        private async ValueTask<ConsumerAccessServiceException> CreateAndLogServiceExceptionAsync(
           Xeption exception)
        {
            var consumerAccessServiceException = new ConsumerAccessServiceException(
                message: "Service error occurred, contact support.",
                innerException: exception);

            await this.loggingBroker.LogErrorAsync(consumerAccessServiceException);

            return consumerAccessServiceException;
        }
    }
}
