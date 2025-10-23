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

            var failedStorageConsumerAccessException =
                new FailedStorageConsumerAccessException(
                    message: "Failed consumer access storage error occurred, contact support.",
                    innerException: sqlException);

            var expectedConsumerAccessDependencyException =
                new ConsumerAccessDependencyException(
                    message: "ConsumerAccess dependency error occurred, contact support.",
                    innerException: failedStorageConsumerAccessException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<ConsumerAccess> addConsumerAccessTask =
                this.consumerAccessService.AddConsumerAccessAsync(
                    someConsumerAccess);

            ConsumerAccessDependencyException actualConsumerAccessDependencyException =
                await Assert.ThrowsAsync<ConsumerAccessDependencyException>(
                    testCode: addConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessDependencyException.Should().BeEquivalentTo(
                expectedConsumerAccessDependencyException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessDependencyException))),
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

            var alreadyExistsConsumerAccessException =
                new AlreadyExistsConsumerAccessException(
                    message: "ConsumerAccess already exists error occurred.",
                    innerException: duplicateKeyException,
                    data: duplicateKeyException.Data);

            var expectedConsumerAccessDependencyValidationException =
                new ConsumerAccessDependencyValidationException(
                    message: "ConsumerAccess dependency validation error occurred, fix errors and try again.",
                    innerException: alreadyExistsConsumerAccessException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ThrowsAsync(duplicateKeyException);

            // when
            ValueTask<ConsumerAccess> addConsumerAccessTask =
                this.consumerAccessService.AddConsumerAccessAsync(someConsumerAccess);

            ConsumerAccessDependencyValidationException actualConsumerAccessDependencyValidationException =
                await Assert.ThrowsAsync<ConsumerAccessDependencyValidationException>(
                    testCode: addConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessDependencyValidationException.Should().BeEquivalentTo(
                expectedConsumerAccessDependencyValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessDependencyValidationException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.InsertConsumerAccessAsync(It.IsAny<ConsumerAccess>()),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnAddIfDependencyErrorOccurredAndLogItAsync()
        {
            // given
            ConsumerAccess someConsumerAccess = CreateRandomConsumerAccess();
            var dbUpdateException = new DbUpdateException();

            var failedOperationConsumerAccessException =
                new FailedOperationConsumerAccessException(
                    message: "Failed operation consumer access error occurred, contact support.",
                    innerException: dbUpdateException);

            var expectedConsumerAccessDependencyException =
                new ConsumerAccessDependencyException(
                    message: "ConsumerAccess dependency error occurred, contact support.",
                    innerException: failedOperationConsumerAccessException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ThrowsAsync(dbUpdateException);

            // when
            ValueTask<ConsumerAccess> addConsumerAccessTask =
                this.consumerAccessService.AddConsumerAccessAsync(
                    someConsumerAccess);

            ConsumerAccessDependencyException actualConsumerAccessDependencyException =
                await Assert.ThrowsAsync<ConsumerAccessDependencyException>(
                    testCode: addConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessDependencyException.Should().BeEquivalentTo(
                expectedConsumerAccessDependencyException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessDependencyException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.InsertConsumerAccessAsync(It.IsAny<ConsumerAccess>()),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnAddIfServiceErrorOccurredAndLogItAsync()
        {
            // given
            ConsumerAccess someConsumerAccess = CreateRandomConsumerAccess();
            var serviceException = new Exception();

            var failedServiceConsumerAccessException =
                new FailedServiceConsumerAccessException(
                    message: "Failed service consumer access error occurred, contact support.",
                    innerException: serviceException);

            var expectedConsumerAccessServiceException =
                new ConsumerAccessServiceException(
                    message: "Service error occurred, contact support.",
                    innerException: failedServiceConsumerAccessException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
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

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
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
            this.securityBrokerMock.VerifyNoOtherCalls();
        }
    }
}
