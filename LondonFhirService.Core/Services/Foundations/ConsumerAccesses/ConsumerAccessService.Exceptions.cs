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
            catch (NullConsumerAccessServiceException nullConsumerAccessServiceException)
            {
                throw await CreateAndLogValidationExceptionAsync(nullConsumerAccessServiceException);
            }
            catch (InvalidConsumerAccessServiceException invalidConsumerAccessServiceException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidConsumerAccessServiceException);
            }
            catch (NotFoundConsumerAccessServiceException notFoundConsumerAccessServiceException)
            {
                throw await CreateAndLogValidationExceptionAsync(notFoundConsumerAccessServiceException);
            }
            catch (SqlException sqlException)
            {
                var failedStorageConsumerAccessServiceException = new FailedStorageConsumerAccessServiceException(
                    message: "Failed consumer access storage error occurred, contact support.",
                    innerException: sqlException);

                throw await CreateAndLogCriticalDependencyExceptionAsync(failedStorageConsumerAccessServiceException);
            }
            catch (DuplicateKeyException duplicateKeyException)
            {
                var alreadyExistsConsumerAccessServiceException =
                    new AlreadyExistsConsumerAccessServiceException(
                        message: "ConsumerAccess already exists error occurred.",
                        innerException: duplicateKeyException,
                        data: duplicateKeyException.Data);

                throw await CreateAndLogDependencyValidationExceptionAsync(alreadyExistsConsumerAccessServiceException);
            }
            catch (DbUpdateConcurrencyException dbUpdateConcurrencyException)
            {
                var lockedConsumerAccessServiceException =
                    new LockedConsumerAccessServiceException(
                        message: "Locked consumer access record error occurred, please try again.",
                        innerException: dbUpdateConcurrencyException);

                throw await CreateAndLogDependencyValidationExceptionAsync(lockedConsumerAccessServiceException);
            }
            catch (DbUpdateException dbUpdateException)
            {
                var failedOperationConsumerAccessServiceException =
                    new FailedOperationConsumerAccessServiceException(
                        message: "Failed operation consumer access error occurred, contact support.",
                        innerException: dbUpdateException);

                throw await CreateAndLogDependencyExceptionAsync(failedOperationConsumerAccessServiceException);
            }
            catch (Exception exception)
            {
                var failedConsumerAccessServiceException =
                    new FailedConsumerAccessServiceException(
                        message: "Failed service consumer access error occurred, contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedConsumerAccessServiceException);
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
                var failedStorageConsumerAccessServiceException = new FailedStorageConsumerAccessServiceException(
                    message: "Failed consumer access storage error occurred, contact support.",
                    innerException: sqlException);

                throw await CreateAndLogCriticalDependencyExceptionAsync(failedStorageConsumerAccessServiceException);
            }
            catch (Exception exception)
            {
                var failedConsumerAccessServiceException =
                    new FailedConsumerAccessServiceException(
                        message: "Failed service consumer access error occurred, contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedConsumerAccessServiceException);
            }
        }

        private async ValueTask<List<string>> TryCatch(ReturningStringListFunction returningStringListFunction)
        {
            try
            {
                return await returningStringListFunction();
            }
            catch (InvalidConsumerAccessServiceException invalidConsumerAccessServiceException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidConsumerAccessServiceException);
            }
            catch (SqlException sqlException)
            {
                var failedStorageConsumerAccessServiceException = new FailedStorageConsumerAccessServiceException(
                    message: "Failed consumer access storage error occurred, contact support.",
                    innerException: sqlException);

                throw await CreateAndLogCriticalDependencyExceptionAsync(failedStorageConsumerAccessServiceException);
            }
            catch (Exception exception)
            {
                var failedConsumerAccessServiceException =
                    new FailedConsumerAccessServiceException(
                        message: "Failed service consumer access error occurred, contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedConsumerAccessServiceException);
            }
        }

        private async ValueTask<ConsumerAccessServiceValidationException> CreateAndLogValidationExceptionAsync(
            Xeption exception)
        {
            var consumerAccessServiceValidationException = new ConsumerAccessServiceValidationException(
                message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                innerException: exception);

            await this.loggingBroker.LogErrorAsync(consumerAccessServiceValidationException);

            return consumerAccessServiceValidationException;
        }

        private async ValueTask<ConsumerAccessServiceDependencyException> CreateAndLogCriticalDependencyExceptionAsync(
            Xeption exception)
        {
            var consumerAccessServiceDependencyException = new ConsumerAccessServiceDependencyException(
                message: "ConsumerAccess dependency error occurred, contact support.",
                innerException: exception);

            await this.loggingBroker.LogCriticalAsync(consumerAccessServiceDependencyException);

            return consumerAccessServiceDependencyException;
        }

        private async ValueTask<ConsumerAccessServiceDependencyValidationException> CreateAndLogDependencyValidationExceptionAsync(
            Xeption exception)
        {
            var consumerAccessDependencyValidationException = new ConsumerAccessServiceDependencyValidationException(
                message: "ConsumerAccess dependency validation error occurred, fix errors and try again.",
                innerException: exception);

            await this.loggingBroker.LogErrorAsync(consumerAccessDependencyValidationException);

            return consumerAccessDependencyValidationException;
        }

        private async ValueTask<ConsumerAccessServiceDependencyException> CreateAndLogDependencyExceptionAsync(
            Xeption exception)
        {
            var consumerAccessServiceDependencyException = new ConsumerAccessServiceDependencyException(
                message: "ConsumerAccess dependency error occurred, contact support.",
                innerException: exception);

            await this.loggingBroker.LogErrorAsync(consumerAccessServiceDependencyException);

            return consumerAccessServiceDependencyException;
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
