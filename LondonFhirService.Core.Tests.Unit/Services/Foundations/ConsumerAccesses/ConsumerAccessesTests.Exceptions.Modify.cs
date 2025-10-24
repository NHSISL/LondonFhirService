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
        public async Task ShouldThrowCriticalDependencyExceptionOnModifyIfSqlErrorOccursAndLogItAsync()
        {
            // given
            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess();
            SqlException sqlException = CreateSqlException();

            var failedStorageConsumerAccessServiceException =
                new FailedStorageConsumerAccessServiceException(
                    message: "Failed consumer access storage error occurred, contact support.",
                        innerException: sqlException);

            var expectedConsumerAccessServiceDependencyException =
                new ConsumerAccessServiceDependencyException(
                    message: "ConsumerAccess dependency error occurred, contact support.",
                        innerException: failedStorageConsumerAccessServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<ConsumerAccess>()))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<ConsumerAccess> modifyConsumerAccessTask =
                this.consumerAccessService.ModifyConsumerAccessAsync(randomConsumerAccess);

            ConsumerAccessServiceDependencyException actualConsumerAccessServiceDependencyException =
                await Assert.ThrowsAsync<ConsumerAccessServiceDependencyException>(
                    testCode: modifyConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceDependencyException.Should().BeEquivalentTo(
                expectedConsumerAccessServiceDependencyException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<ConsumerAccess>()),
                    Times.Once);

            this.storageBroker.Verify(broker =>
                broker.SelectConsumerAccessByIdAsync(randomConsumerAccess.Id),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceDependencyException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.UpdateConsumerAccessAsync(randomConsumerAccess),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnModifyIfDependencyErrorOccurredAndLogItAsync()
        {
            // given
            ConsumerAccess someConsumerAccess = CreateRandomConsumerAccess();
            var dbUpdateException = new DbUpdateException();

            var failedOperationConsumerAccessServiceException =
                new FailedOperationConsumerAccessServiceException(
                    message: "Failed operation consumer access error occurred, contact support.",
                    innerException: dbUpdateException);

            var expectedConsumerAccessServiceDependencyException =
                new ConsumerAccessServiceDependencyException(
                    message: "ConsumerAccess dependency error occurred, contact support.",
                    innerException: failedOperationConsumerAccessServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<ConsumerAccess>()))
                    .ThrowsAsync(dbUpdateException);

            // when
            ValueTask<ConsumerAccess> modifyConsumerAccessTask =
                this.consumerAccessService.ModifyConsumerAccessAsync(someConsumerAccess);

            ConsumerAccessServiceDependencyException actualConsumerAccessServiceDependencyException =
                await Assert.ThrowsAsync<ConsumerAccessServiceDependencyException>(
                    testCode: modifyConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceDependencyException.Should().BeEquivalentTo(
                expectedConsumerAccessServiceDependencyException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<ConsumerAccess>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceDependencyException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.InsertConsumerAccessAsync(It.IsAny<ConsumerAccess>()),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnModifyIfDbUpdateConcurrencyOccursAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();
            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess(randomDateTimeOffset, randomUserId);
            var dbUpdateConcurrencyException = new DbUpdateConcurrencyException();

            var lockedConsumerAccessServiceException = new LockedConsumerAccessServiceException(
                message: "Locked consumer access record error occurred, please try again.",
                innerException: dbUpdateConcurrencyException);

            var expectedConsumerAccessServiceDependencyValidationException = new ConsumerAccessServiceDependencyValidationException(
                message: "ConsumerAccess dependency validation error occurred, fix errors and try again.",
                innerException: lockedConsumerAccessServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<ConsumerAccess>()))
                    .ThrowsAsync(dbUpdateConcurrencyException);

            // when
            ValueTask<ConsumerAccess> modifyConsumerAccessTask =
                this.consumerAccessService.ModifyConsumerAccessAsync(randomConsumerAccess);

            ConsumerAccessServiceDependencyValidationException actualConsumerAccessServiceDependencyValidationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceDependencyValidationException>(
                    testCode: modifyConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceDependencyValidationException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceDependencyValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<ConsumerAccess>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceDependencyValidationException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.SelectConsumerAccessByIdAsync(randomConsumerAccess.Id),
                    Times.Never());

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnModifyIfServiceErrorOccurredAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();
            ConsumerAccess someConsumerAccess = CreateRandomModifyConsumerAccess(randomDateTimeOffset, randomUserId);
            var serviceException = new Exception();

            var failedConsumerAccessServiceException =
                new FailedConsumerAccessServiceException(
                    message: "Failed service consumer access error occurred, contact support.",
                    innerException: serviceException);

            var expectedConsumerAccessServiceException =
                new ConsumerAccessServiceException(
                    message: "Service error occurred, contact support.",
                    innerException: failedConsumerAccessServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<ConsumerAccess>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<ConsumerAccess> modifyConsumerAccessTask =
                this.consumerAccessService.ModifyConsumerAccessAsync(someConsumerAccess);

            ConsumerAccessServiceException actualConsumerAccessServiceException =
                await Assert.ThrowsAsync<ConsumerAccessServiceException>(
                    testCode: modifyConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceException.Should().BeEquivalentTo(
                expectedConsumerAccessServiceException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<ConsumerAccess>()),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.InsertConsumerAccessAsync(It.IsAny<ConsumerAccess>()),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
        }
    }
}
