// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions;
using Microsoft.Data.SqlClient;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ConsumerAccesses
{
    public partial class ConsumerAccessesTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnRetrieveByIdIfSqlErrorOccursAndLogItAsync()
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
            ValueTask<ConsumerAccess> retrieveByIdConsumerAccessTask =
                this.consumerAccessService.RetrieveConsumerAccessByIdAsync(randomConsumerAccessId);

            ConsumerAccessServiceDependencyException actualConsumerAccessServiceDependencyException =
                await Assert.ThrowsAsync<ConsumerAccessServiceDependencyException>(
                    testCode: retrieveByIdConsumerAccessTask.AsTask);

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

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnRetrieveByIdWhenServiceErrorOccursAndLogItAsync()
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
            ValueTask<ConsumerAccess> retrieveByIdConsumerAccessTask =
                this.consumerAccessService.RetrieveConsumerAccessByIdAsync(randomConsumerAccessId);

            ConsumerAccessServiceException actualConsumerAccessServiceExcpetion =
                await Assert.ThrowsAsync<ConsumerAccessServiceException>(
                    testCode: retrieveByIdConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceExcpetion.Should().BeEquivalentTo(expectedConsumerAccessServiceException);

            this.storageBroker.Verify(broker =>
                broker.SelectConsumerAccessByIdAsync(randomConsumerAccessId),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceException))),
                        Times.Once);

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
