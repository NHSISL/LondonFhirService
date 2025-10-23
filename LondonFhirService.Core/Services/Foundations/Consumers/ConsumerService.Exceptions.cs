// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
using LondonFhirService.Core.Models.Foundations.Consumers;
using LondonFhirService.Core.Models.Foundations.Consumers.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.Consumers
{
    public partial class ConsumerService
    {
        private delegate ValueTask<Consumer> ReturningConsumerFunction();
        private delegate ValueTask<IQueryable<Consumer>> ReturningConsumersFunction();

        private async ValueTask<Consumer> TryCatch(ReturningConsumerFunction returningConsumerFunction)
        {
            try
            {
                return await returningConsumerFunction();
            }
            catch (NullConsumerServiceException nullConsumerServiceException)
            {
                throw await CreateAndLogValidationException(nullConsumerServiceException);
            }
            catch (InvalidConsumerServiceException invalidConsumerServiceException)
            {
                throw await CreateAndLogValidationException(invalidConsumerServiceException);
            }
            catch (SqlException sqlException)
            {
                var failedStorageConsumerServiceException =
                    new FailedStorageConsumerServiceException(
                        message: "Failed consumer storage error occurred, contact support.",
                        innerException: sqlException);

                throw await CreateAndLogCriticalDependencyException(failedStorageConsumerServiceException);
            }
            catch (NotFoundConsumerServiceException notFoundConsumerServiceException)
            {
                throw await CreateAndLogValidationException(notFoundConsumerServiceException);
            }
            catch (DuplicateKeyException duplicateKeyException)
            {
                var alreadyExistsConsumerServiceException =
                    new AlreadyExistsConsumerServiceException(
                        message: "Consumer with the same Id already exists.",
                        innerException: duplicateKeyException);

                throw await CreateAndLogDependencyValidationException(alreadyExistsConsumerServiceException);
            }
            catch (ForeignKeyConstraintConflictException foreignKeyConstraintConflictException)
            {
                var invalidConsumerReferenceException =
                    new InvalidConsumerReferenceException(
                        message: "Invalid consumer reference error occurred.",
                        innerException: foreignKeyConstraintConflictException);

                throw await CreateAndLogDependencyValidationException(invalidConsumerReferenceException);
            }
            catch (DbUpdateConcurrencyException dbUpdateConcurrencyException)
            {
                var lockedConsumerServiceException =
                    new LockedConsumerServiceException(
                        message: "Locked consumer record exception, please try again later",
                        innerException: dbUpdateConcurrencyException);

                throw await CreateAndLogDependencyValidationException(lockedConsumerServiceException);
            }
            catch (DbUpdateException databaseUpdateException)
            {
                var failedStorageConsumerServiceException =
                    new FailedStorageConsumerServiceException(
                        message: "Failed consumer storage error occurred, contact support.",
                        innerException: databaseUpdateException);

                throw await CreateAndLogDependencyException(failedStorageConsumerServiceException);
            }
            catch (Exception exception)
            {
                var failedConsumerServiceException =
                    new FailedConsumerServiceException(
                        message: "Failed consumer service occurred, please contact support",
                        innerException: exception);

                throw await CreateAndLogServiceException(failedConsumerServiceException);
            }
        }

        private async ValueTask<IQueryable<Consumer>> TryCatch(
            ReturningConsumersFunction returningConsumersFunction)
        {
            try
            {
                return await returningConsumersFunction();
            }
            catch (SqlException sqlException)
            {
                var failedStorageConsumerServiceException =
                    new FailedStorageConsumerServiceException(
                        message: "Failed consumer storage error occurred, contact support.",
                        innerException: sqlException);

                throw await CreateAndLogCriticalDependencyException(failedStorageConsumerServiceException);
            }
            catch (Exception exception)
            {
                var failedConsumerServiceException =
                    new FailedConsumerServiceException(
                        message: "Failed consumer service occurred, please contact support",
                        innerException: exception);

                throw await CreateAndLogServiceException(failedConsumerServiceException);
            }
        }

        private async ValueTask<ConsumerServiceValidationException> CreateAndLogValidationException(Xeption exception)
        {
            var consumerServiceValidationException =
                new ConsumerServiceValidationException(
                    message: "Consumer validation errors occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(consumerServiceValidationException);

            return consumerServiceValidationException;
        }

        private async ValueTask<ConsumerServiceDependencyException> CreateAndLogCriticalDependencyException(
            Xeption exception)
        {
            var consumerServiceDependencyException =
                new ConsumerServiceDependencyException(
                    message: "Consumer dependency error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogCriticalAsync(consumerServiceDependencyException);

            return consumerServiceDependencyException;
        }

        private async ValueTask<ConsumerServiceDependencyValidationException> CreateAndLogDependencyValidationException(
            Xeption exception)
        {
            var consumerServiceDependencyValidationException =
                new ConsumerServiceDependencyValidationException(
                    message: "Consumer dependency validation occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(consumerServiceDependencyValidationException);

            return consumerServiceDependencyValidationException;
        }

        private async ValueTask<ConsumerServiceDependencyException> CreateAndLogDependencyException(
            Xeption exception)
        {
            var consumerServiceDependencyException =
                new ConsumerServiceDependencyException(
                    message: "Consumer dependency error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(consumerServiceDependencyException);

            return consumerServiceDependencyException;
        }

        private async ValueTask<ConsumerServiceException> CreateAndLogServiceException(
            Xeption exception)
        {
            var consumerServiceException =
                new ConsumerServiceException(
                    message: "Consumer service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(consumerServiceException);

            return consumerServiceException;
        }
    }
}
