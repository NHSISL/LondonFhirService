// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
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
        public async Task ShouldThrowCriticalDependencyExceptionOnRemoveIfSqlErrorOccursAndLogItAsync()
        {
            // given
            Consumer randomConsumer = CreateRandomConsumer();
            SqlException sqlException = GetSqlException();

            var failedStorageConsumerServiceException =
                new FailedStorageConsumerServiceException(
                    message: "Failed consumer storage error occurred, contact support.",
                    innerException: sqlException);

            var expectedConsumerServiceDependencyException =
                new ConsumerServiceDependencyException(
                    message: "Consumer dependency error occurred, contact support.",
                    innerException: failedStorageConsumerServiceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectConsumerByIdAsync(randomConsumer.Id))
                    .Throws(sqlException);

            // when
            ValueTask<Consumer> addConsumerTask =
                this.consumerService.RemoveConsumerByIdAsync(randomConsumer.Id);

            ConsumerServiceDependencyException actualConsumerServiceDependencyException =
                await Assert.ThrowsAsync<ConsumerServiceDependencyException>(
                    addConsumerTask.AsTask);

            // then
            actualConsumerServiceDependencyException.Should()
                .BeEquivalentTo(expectedConsumerServiceDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectConsumerByIdAsync(randomConsumer.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedConsumerServiceDependencyException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteConsumerAsync(It.IsAny<Consumer>()),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationOnRemoveIfDatabaseUpdateConcurrencyErrorOccursAndLogItAsync()
        {
            // given
            Guid someConsumerId = Guid.NewGuid();

            var databaseUpdateConcurrencyException =
                new DbUpdateConcurrencyException();

            var lockedConsumerServiceException =
                new LockedConsumerServiceException(
                    message: "Locked consumer record exception, please try again later",
                    innerException: databaseUpdateConcurrencyException);

            var expectedConsumerServiceDependencyValidationException =
                new ConsumerServiceDependencyValidationException(
                    message: "Consumer dependency validation occurred, please try again.",
                    innerException: lockedConsumerServiceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectConsumerByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(databaseUpdateConcurrencyException);

            // when
            ValueTask<Consumer> removeConsumerByIdTask =
                this.consumerService.RemoveConsumerByIdAsync(someConsumerId);

            ConsumerServiceDependencyValidationException actualConsumerServiceDependencyValidationException =
                await Assert.ThrowsAsync<ConsumerServiceDependencyValidationException>(
                    removeConsumerByIdTask.AsTask);

            // then
            actualConsumerServiceDependencyValidationException.Should()
                .BeEquivalentTo(expectedConsumerServiceDependencyValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectConsumerByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerServiceDependencyValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteConsumerAsync(It.IsAny<Consumer>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnRemoveWhenSqlExceptionOccursAndLogItAsync()
        {
            // given
            Guid someConsumerId = Guid.NewGuid();
            SqlException sqlException = GetSqlException();

            var failedStorageConsumerServiceException =
                new FailedStorageConsumerServiceException(
                    message: "Failed consumer storage error occurred, contact support.",
                    innerException: sqlException);

            var expectedConsumerServiceDependencyException =
                new ConsumerServiceDependencyException(
                    message: "Consumer dependency error occurred, contact support.",
                    innerException: failedStorageConsumerServiceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectConsumerByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<Consumer> deleteConsumerTask =
                this.consumerService.RemoveConsumerByIdAsync(someConsumerId);

            ConsumerServiceDependencyException actualConsumerServiceDependencyException =
                await Assert.ThrowsAsync<ConsumerServiceDependencyException>(
                    deleteConsumerTask.AsTask);

            // then
            actualConsumerServiceDependencyException.Should()
                .BeEquivalentTo(expectedConsumerServiceDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectConsumerByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedConsumerServiceDependencyException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnRemoveIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Guid someConsumerId = Guid.NewGuid();
            var serviceException = new Exception();

            var failedConsumerServiceException =
                new FailedConsumerServiceException(
                    message: "Failed consumer service occurred, please contact support",
                    innerException: serviceException);

            var expectedConsumerServiceException =
                new ConsumerServiceException(
                    message: "Consumer service error occurred, contact support.",
                    innerException: failedConsumerServiceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectConsumerByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<Consumer> removeConsumerByIdTask =
                this.consumerService.RemoveConsumerByIdAsync(someConsumerId);

            ConsumerServiceException actualConsumerServiceException =
                await Assert.ThrowsAsync<ConsumerServiceException>(
                    removeConsumerByIdTask.AsTask);

            // then
            actualConsumerServiceException.Should()
                .BeEquivalentTo(expectedConsumerServiceException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectConsumerByIdAsync(It.IsAny<Guid>()),
                    Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerServiceException))),
                        Times.Once());

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
