// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Consumers;
using LondonFhirService.Core.Models.Foundations.Consumers.Exceptions;
using Microsoft.Data.SqlClient;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Consumers
{
    public partial class ConsumerServiceTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnRetrieveAllWhenSqlExceptionOccursAndLogIt()
        {
            // given
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
                broker.SelectAllConsumersAsync())
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<IQueryable<Consumer>> retrieveAllConsumersTask = this.consumerService.RetrieveAllConsumersAsync();

            ConsumerServiceDependencyException actualConsumerServiceDependencyException =
                await Assert.ThrowsAsync<ConsumerServiceDependencyException>(
                    retrieveAllConsumersTask.AsTask);

            // then
            actualConsumerServiceDependencyException.Should()
                .BeEquivalentTo(expectedConsumerServiceDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllConsumersAsync(),
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
        public async Task ShouldThrowServiceExceptionOnRetrieveAllIfServiceErrorOccursAndLogItAsync()
        {
            // given
            string exceptionMessage = GetRandomString();
            var serviceException = new Exception(exceptionMessage);

            var failedConsumerServiceException =
                new FailedConsumerServiceException(
                    message: "Failed consumer service occurred, please contact support",
                    innerException: serviceException);

            var expectedConsumerServiceException =
                new ConsumerServiceException(
                    message: "Consumer service error occurred, contact support.",
                    innerException: failedConsumerServiceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllConsumersAsync())
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<IQueryable<Consumer>> retrieveAllConsumersTask =
                this.consumerService.RetrieveAllConsumersAsync();

            ConsumerServiceException actualConsumerServiceException =
                await Assert.ThrowsAsync<ConsumerServiceException>(retrieveAllConsumersTask.AsTask);

            // then
            actualConsumerServiceException.Should()
                .BeEquivalentTo(expectedConsumerServiceException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllConsumersAsync(),
                    Times.Once);

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
