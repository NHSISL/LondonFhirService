// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Consumers;
using LondonFhirService.Core.Models.Foundations.Consumers.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Consumers
{
    public partial class ConsumerServiceTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnAddIfSqlErrorOccursAndLogItAsync()
        {
            // given
            Consumer someConsumer = CreateRandomConsumer();
            SqlException sqlException = GetSqlException();

            var failedStorageConsumerServiceException =
                new FailedStorageConsumerServiceException(
                    message: "Failed consumer storage error occurred, contact support.",
                    innerException: sqlException);

            var expectedConsumerServiceDependencyException =
                new ConsumerServiceDependencyException(
                    message: "Consumer dependency error occurred, contact support.",
                    innerException: failedStorageConsumerServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Consumer>()))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<Consumer> addConsumerTask =
                this.consumerService.AddConsumerAsync(someConsumer);

            ConsumerServiceDependencyException actualConsumerServiceDependencyException =
                await Assert.ThrowsAsync<ConsumerServiceDependencyException>(
                    addConsumerTask.AsTask);

            // then
            actualConsumerServiceDependencyException.Should()
                .BeEquivalentTo(expectedConsumerServiceDependencyException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Consumer>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedConsumerServiceDependencyException))),
                        Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never());

            this.storageBrokerMock.Verify(broker =>
                broker.InsertConsumerAsync(It.IsAny<Consumer>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfConsumerAlreadyExistsAndLogItAsync()
        {
            // given
            Consumer randomConsumer = CreateRandomConsumer();
            Consumer alreadyExistsConsumer = randomConsumer;
            string randomMessage = GetRandomString();

            var duplicateKeyException =
                new DuplicateKeyException(randomMessage);

            var alreadyExistsConsumerServiceException =
                new AlreadyExistsConsumerServiceException(
                    message: "Consumer with the same Id already exists.",
                    innerException: duplicateKeyException);

            var expectedConsumerServiceDependencyValidationException =
                new ConsumerServiceDependencyValidationException(
                    message: "Consumer dependency validation occurred, please try again.",
                    innerException: alreadyExistsConsumerServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Consumer>()))
                    .ThrowsAsync(duplicateKeyException);

            // when
            ValueTask<Consumer> addConsumerTask =
                this.consumerService.AddConsumerAsync(alreadyExistsConsumer);

            // then
            ConsumerServiceDependencyValidationException actualConsumerServiceDependencyValidationException =
                await Assert.ThrowsAsync<ConsumerServiceDependencyValidationException>(
                    addConsumerTask.AsTask);

            actualConsumerServiceDependencyValidationException.Should()
                .BeEquivalentTo(expectedConsumerServiceDependencyValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Consumer>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerServiceDependencyValidationException))),
                        Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never());

            this.storageBrokerMock.Verify(broker =>
                broker.InsertConsumerAsync(It.IsAny<Consumer>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfReferenceErrorOccursAndLogItAsync()
        {
            // given
            Consumer someConsumer = CreateRandomConsumer();
            string randomMessage = GetRandomString();
            string exceptionMessage = randomMessage;

            var foreignKeyConstraintConflictException =
                new ForeignKeyConstraintConflictException(exceptionMessage);

            var invalidConsumerReferenceException =
                new InvalidConsumerReferenceException(
                    message: "Invalid consumer reference error occurred.",
                    innerException: foreignKeyConstraintConflictException);

            var expectedConsumerServiceValidationException =
                new ConsumerServiceDependencyValidationException(
                    message: "Consumer dependency validation occurred, please try again.",
                    innerException: invalidConsumerReferenceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Consumer>()))
                    .ThrowsAsync(foreignKeyConstraintConflictException);

            // when
            ValueTask<Consumer> addConsumerTask =
                this.consumerService.AddConsumerAsync(someConsumer);

            // then
            ConsumerServiceDependencyValidationException actualConsumerServiceDependencyValidationException =
                await Assert.ThrowsAsync<ConsumerServiceDependencyValidationException>(
                    addConsumerTask.AsTask);

            actualConsumerServiceDependencyValidationException.Should()
                .BeEquivalentTo(expectedConsumerServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Consumer>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerServiceValidationException))),
                        Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never());

            this.storageBrokerMock.Verify(broker =>
                broker.InsertConsumerAsync(It.IsAny<Consumer>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnAddIfDatabaseUpdateErrorOccursAndLogItAsync()
        {
            // given
            Consumer someConsumer = CreateRandomConsumer();

            var databaseUpdateException = new DbUpdateException();

            var failedStorageConsumerServiceException =
                new FailedStorageConsumerServiceException(
                    message: "Failed consumer storage error occurred, contact support.",
                    innerException: databaseUpdateException);

            var expectedConsumerServiceDependencyException =
                new ConsumerServiceDependencyException(
                    message: "Consumer dependency error occurred, contact support.",
                    innerException: failedStorageConsumerServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Consumer>()))
                    .ThrowsAsync(databaseUpdateException);

            // when
            ValueTask<Consumer> addConsumerTask =
                this.consumerService.AddConsumerAsync(someConsumer);

            ConsumerServiceDependencyException actualConsumerServiceDependencyException =
                await Assert.ThrowsAsync<ConsumerServiceDependencyException>(
                    addConsumerTask.AsTask);

            // then
            actualConsumerServiceDependencyException.Should()
                .BeEquivalentTo(expectedConsumerServiceDependencyException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Consumer>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerServiceDependencyException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnAddIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Consumer someConsumer = CreateRandomConsumer();
            var serviceException = new Exception();

            var failedConsumerServiceException =
                new FailedConsumerServiceException(
                    message: "Failed consumer service occurred, please contact support",
                    innerException: serviceException);

            var expectedConsumerServiceException =
                new ConsumerServiceException(
                    message: "Consumer service error occurred, contact support.",
                    innerException: failedConsumerServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Consumer>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<Consumer> addConsumerTask = this.consumerService.AddConsumerAsync(someConsumer);

            ConsumerServiceException actualConsumerServiceException =
                await Assert.ThrowsAsync<ConsumerServiceException>(
                    addConsumerTask.AsTask);

            // then
            actualConsumerServiceException.Should()
                .BeEquivalentTo(expectedConsumerServiceException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Consumer>()),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertConsumerAsync(It.IsAny<Consumer>()),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerServiceException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
