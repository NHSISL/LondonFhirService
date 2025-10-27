// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
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
        public async Task ShouldThrowCriticalDependencyExceptionOnAddIfSqlErrorOccurredAndLogItAsync()
        {
            // given
            ConsumerAccess someConsumerAccess = CreateRandomConsumerAccess();
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
                broker.ApplyAddAuditValuesAsync(It.IsAny<ConsumerAccess>()))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<ConsumerAccess> addConsumerAccessTask =
                this.consumerAccessService.AddConsumerAccessAsync(
                    someConsumerAccess);

            ConsumerAccessServiceDependencyException actualConsumerAccessServiceDependencyException =
                await Assert.ThrowsAsync<ConsumerAccessServiceDependencyException>(
                    testCode: addConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceDependencyException.Should().BeEquivalentTo(
                expectedConsumerAccessServiceDependencyException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<ConsumerAccess>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceDependencyException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.InsertConsumerAccessAsync(It.IsAny<ConsumerAccess>()),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfConsumerAccessAlreadyExistsAndLogItAsync()
        {
            // given
            ConsumerAccess someConsumerAccess = CreateRandomConsumerAccess();

            var duplicateKeyException =
                new DuplicateKeyException(
                    message: "Duplicate key error occurred");

            var alreadyExistsConsumerAccessServiceException =
                new AlreadyExistsConsumerAccessServiceException(
                    message: "ConsumerAccess already exists error occurred.",
                    innerException: duplicateKeyException,
                    data: duplicateKeyException.Data);

            var expectedConsumerAccessServiceDependencyValidationException =
                new ConsumerAccessServiceDependencyValidationException(
                    message: "ConsumerAccess dependency validation error occurred, fix errors and try again.",
                    innerException: alreadyExistsConsumerAccessServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<ConsumerAccess>()))
                    .ThrowsAsync(duplicateKeyException);

            // when
            ValueTask<ConsumerAccess> addConsumerAccessTask =
                this.consumerAccessService.AddConsumerAccessAsync(someConsumerAccess);

            ConsumerAccessServiceDependencyValidationException actualConsumerAccessServiceDependencyValidationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceDependencyValidationException>(
                    testCode: addConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceDependencyValidationException.Should().BeEquivalentTo(
                expectedConsumerAccessServiceDependencyValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<ConsumerAccess>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceDependencyValidationException))),
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
        public async Task ShouldThrowDependencyExceptionOnAddIfDependencyErrorOccurredAndLogItAsync()
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
                broker.ApplyAddAuditValuesAsync(It.IsAny<ConsumerAccess>()))
                    .ThrowsAsync(dbUpdateException);

            // when
            ValueTask<ConsumerAccess> addConsumerAccessTask =
                this.consumerAccessService.AddConsumerAccessAsync(
                    someConsumerAccess);

            ConsumerAccessServiceDependencyException actualConsumerAccessServiceDependencyException =
                await Assert.ThrowsAsync<ConsumerAccessServiceDependencyException>(
                    testCode: addConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceDependencyException.Should().BeEquivalentTo(
                expectedConsumerAccessServiceDependencyException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<ConsumerAccess>()),
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
        public async Task ShouldThrowServiceExceptionOnAddIfServiceErrorOccurredAndLogItAsync()
        {
            // given
            ConsumerAccess someConsumerAccess = CreateRandomConsumerAccess();
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
                broker.ApplyAddAuditValuesAsync(It.IsAny<ConsumerAccess>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<ConsumerAccess> addConsumerAccessTask =
                this.consumerAccessService.AddConsumerAccessAsync(someConsumerAccess);

            ConsumerAccessServiceException actualConsumerAccessServiceException =
                await Assert.ThrowsAsync<ConsumerAccessServiceException>(
                    testCode: addConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceException.Should().BeEquivalentTo(
                expectedConsumerAccessServiceException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<ConsumerAccess>()),
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
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }
    }
}
