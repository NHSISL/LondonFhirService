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

            var failedStorageConsumerAccessServiceException =
                new FailedStorageConsumerAccessServiceException(
                    message: "Failed consumer access storage error occurred, contact support.",
                        innerException: sqlException);

            var expectedConsumerAccessServiceDependencyException =
                new ConsumerAccessServiceDependencyException(
                    message: "ConsumerAccess dependency error occurred, contact support.",
                        innerException: failedStorageConsumerAccessServiceException);

            this.storageBroker.Setup(broker =>
                broker.SelectConsumerAccessByIdAsync(randomConsumerAccessId))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<ConsumerAccess> removeByIdConsumerAccessTask =
                this.consumerAccessService.RemoveConsumerAccessByIdAsync(randomConsumerAccessId);

            ConsumerAccessServiceDependencyException actualConsumerAccessServiceDependencyException =
                await Assert.ThrowsAsync<ConsumerAccessServiceDependencyException>(
                    testCode: removeByIdConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceDependencyException.Should().BeEquivalentTo(
                expectedConsumerAccessServiceDependencyException);

            this.storageBroker.Verify(broker =>
                broker.SelectConsumerAccessByIdAsync(randomConsumerAccessId),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceDependencyException))),
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
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task
            ShouldThrowDependencyValidationOnRemoveConsumerAccessByIdIfDatabaseUpdateConcurrencyErrorOccursAndLogItAsync()
        {
            // given
            Guid randomConsumerAccessId = Guid.NewGuid();

            var databaseUpdateConcurrencyException =
                new DbUpdateConcurrencyException();

            var lockedConsumerAccessServiceException =
                new LockedConsumerAccessServiceException(
                    message: "Locked consumer access record error occurred, please try again.",
                    innerException: databaseUpdateConcurrencyException);

            var expectedConsumerAccessServiceDependencyValidationException =
                new ConsumerAccessServiceDependencyValidationException(
                    message: "ConsumerAccess dependency validation error occurred, fix errors and try again.",
                    innerException: lockedConsumerAccessServiceException);

            this.storageBroker.Setup(broker =>
                broker.SelectConsumerAccessByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(databaseUpdateConcurrencyException);

            // when
            ValueTask<ConsumerAccess> removeConsumerAccessByIdTask =
                this.consumerAccessService.RemoveConsumerAccessByIdAsync(randomConsumerAccessId);

            ConsumerAccessServiceDependencyValidationException actualConsumerAccessServiceDependencyValidationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceDependencyValidationException>(
                    testCode: removeConsumerAccessByIdTask.AsTask);

            // then
            actualConsumerAccessServiceDependencyValidationException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceDependencyValidationException);

            this.storageBroker.Verify(broker =>
                broker.SelectConsumerAccessByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceDependencyValidationException))),
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
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnRemoveConsumerAccessByIdWhenServiceErrorOccursAndLogItAsync()
        {
            // given
            Guid randomConsumerAccessId = Guid.NewGuid();
            Exception serviceError = new Exception();

            var failedConsumerAccessServiceException = new FailedConsumerAccessServiceException(
                message: "Failed service consumer access error occurred, contact support.",
                innerException: serviceError);

            var expectedConsumerAccessServiceException = new ConsumerAccessServiceException(
                message: "Service error occurred, contact support.",
                innerException: failedConsumerAccessServiceException);

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
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }
    }
}
