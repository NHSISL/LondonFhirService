// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ConsumerAccesses
{
    public partial class ConsumerAccessesTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnRemoveByIdIfSqlErrorOccursAndLogItAsync()
        {
            // given
            Guid randomConsumerAccessId = Guid.NewGuid();
            SqlException sqlException = CreateSqlException();

            var failedConsumerAccessStorageException =
                new FailedStorageConsumerAccessException(
                    message: "Failed user access storage error occurred, contact support.",
                        innerException: sqlException);

            var expectedConsumerAccessDependencyException =
                new ConsumerAccessDependencyException(
                    message: "ConsumerAccess dependency error occurred, contact support.",
                        innerException: failedConsumerAccessStorageException);

            this.storageBroker.Setup(broker =>
                broker.SelectConsumerAccessByIdAsync(randomConsumerAccessId))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<ConsumerAccess> removeByIdConsumerAccessTask =
                this.consumerAccessService.RemoveConsumerAccessByIdAsync(randomConsumerAccessId);

            ConsumerAccessDependencyException actualConsumerAccessDependencyException =
                await Assert.ThrowsAsync<ConsumerAccessDependencyException>(
                    testCode: removeByIdConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessDependencyException.Should().BeEquivalentTo(
                expectedConsumerAccessDependencyException);

            this.storageBroker.Verify(broker =>
                broker.SelectConsumerAccessByIdAsync(randomConsumerAccessId),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessDependencyException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.DeleteConsumerAccessAsync(It.IsAny<ConsumerAccess>()),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task
            ShouldThrowDependencyValidationOnRemoveConsumerAccessByIdIfDatabaseUpdateConcurrencyErrorOccursAndLogItAsync()
        {
            // given
            Guid randomConsumerAccessId = Guid.NewGuid();

            var databaseUpdateConcurrencyException =
                new DbUpdateConcurrencyException();

            var lockedConsumerAccessException =
                new LockedConsumerAccessException(
                    message: "Locked user access record error occurred, please try again.",
                    innerException: databaseUpdateConcurrencyException);

            var expectedConsumerAccessDependencyValidationException =
                new ConsumerAccessDependencyValidationException(
                    message: "ConsumerAccess dependency validation error occurred, fix errors and try again.",
                    innerException: lockedConsumerAccessException);

            this.storageBroker.Setup(broker =>
                broker.SelectConsumerAccessByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(databaseUpdateConcurrencyException);

            // when
            ValueTask<ConsumerAccess> removeConsumerAccessByIdTask =
                this.consumerAccessService.RemoveConsumerAccessByIdAsync(randomConsumerAccessId);

            ConsumerAccessDependencyValidationException actualConsumerAccessDependencyValidationException =
                await Assert.ThrowsAsync<ConsumerAccessDependencyValidationException>(
                    testCode: removeConsumerAccessByIdTask.AsTask);

            // then
            actualConsumerAccessDependencyValidationException.Should()
                .BeEquivalentTo(expectedConsumerAccessDependencyValidationException);

            this.storageBroker.Verify(broker =>
                broker.SelectConsumerAccessByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessDependencyValidationException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.DeleteConsumerAccessAsync(It.IsAny<ConsumerAccess>()),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnRemoveConsumerAccessByIdWhenServiceErrorOccursAndLogItAsync()
        {
            // given
            Guid randomConsumerAccessId = Guid.NewGuid();
            Exception serviceError = new Exception();

            var failedServiceConsumerAccessException = new FailedServiceConsumerAccessException(
                message: "Failed service user access error occurred, contact support.",
                innerException: serviceError);

            var expectedConsumerAccessServiceException = new ConsumerAccessServiceException(
                message: "Service error occurred, contact support.",
                innerException: failedServiceConsumerAccessException);

            this.storageBroker.Setup(broker =>
                broker.SelectConsumerAccessByIdAsync(randomConsumerAccessId))
                    .ThrowsAsync(serviceError);

            // when
            ValueTask<ConsumerAccess> removeByIdConsumerAccessTask =
                this.consumerAccessService.RemoveConsumerAccessByIdAsync(randomConsumerAccessId);

            ConsumerAccessServiceException actualConsumerAccessServiceExcpetion =
                await Assert.ThrowsAsync<ConsumerAccessServiceException>(
                    testCode: removeByIdConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceExcpetion.Should().BeEquivalentTo(expectedConsumerAccessServiceException);

            this.storageBroker.Verify(broker =>
                broker.SelectConsumerAccessByIdAsync(randomConsumerAccessId),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.DeleteConsumerAccessAsync(It.IsAny<ConsumerAccess>()),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }
    }
}
