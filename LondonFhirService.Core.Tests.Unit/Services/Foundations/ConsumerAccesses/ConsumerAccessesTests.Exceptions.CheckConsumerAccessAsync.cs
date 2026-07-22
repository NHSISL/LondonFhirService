// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ConsumerAccesses
{
    public partial class ConsumerAccessesTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnCheckConsumerAccessIfCancellationAlreadyRequestedAndLogItAsync()
        {
            // given
            string randomNhsNumber = GetRandomString();
            Guid randomCorrelationId = GetRandomGuid();
            CancellationToken cancelledToken = new CancellationToken(canceled: true);
            var operationCancelledException = new OperationCanceledException(cancelledToken);

            var failedConsumerAccessServiceException =
                new FailedConsumerAccessServiceException(
                    message: "Failed service consumer access error occurred, contact support.",
                    innerException: operationCancelledException);

            var expectedConsumerAccessServiceException =
                new ConsumerAccessServiceException(
                    message: "Service error occurred, contact support.",
                    innerException: failedConsumerAccessServiceException);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    NhsNumber: randomNhsNumber,
                    CorrelationId: randomCorrelationId,
                    cancellationToken: cancelledToken);

            ConsumerAccessServiceException actualConsumerAccessServiceException =
                await Assert.ThrowsAsync<ConsumerAccessServiceException>(
                    checkConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceException))),
                        Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Never);

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()),
                        Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnCheckConsumerAccessIfGetUserIdFailsAndLogItAsync()
        {
            // given
            string randomNhsNumber = GetRandomString();
            Guid randomCorrelationId = GetRandomGuid();
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
                broker.GetUserIdAsync())
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    NhsNumber: randomNhsNumber,
                    CorrelationId: randomCorrelationId);

            ConsumerAccessServiceException actualConsumerAccessServiceException =
                await Assert.ThrowsAsync<ConsumerAccessServiceException>(
                    checkConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceException))),
                        Times.Once);

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()),
                        Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnCheckConsumerAccessIfBrokerCallFailsAndLogItAsync()
        {
            // given
            string randomUserId = GetRandomString();
            string randomNhsNumber = GetRandomString();
            Guid randomCorrelationId = GetRandomGuid();
            var httpRequestException = new HttpRequestException();

            var failedConsumerAccessServiceException =
                new FailedConsumerAccessServiceException(
                    message: "Failed service consumer access error occurred, contact support.",
                    innerException: httpRequestException);

            var expectedConsumerAccessServiceException =
                new ConsumerAccessServiceException(
                    message: "Service error occurred, contact support.",
                    innerException: failedConsumerAccessServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(httpRequestException);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    NhsNumber: randomNhsNumber,
                    CorrelationId: randomCorrelationId);

            ConsumerAccessServiceException actualConsumerAccessServiceException =
                await Assert.ThrowsAsync<ConsumerAccessServiceException>(
                    checkConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Exactly(2));

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnCheckConsumerAccessIfBrokerCallTimesOutAndLogItAsync()
        {
            // given
            string randomUserId = GetRandomString();
            string randomNhsNumber = GetRandomString();
            Guid randomCorrelationId = GetRandomGuid();

            var timeoutException =
                new TaskCanceledException(
                    message: "A task was canceled.",
                    innerException: new TimeoutException());

            var failedConsumerAccessServiceException =
                new FailedConsumerAccessServiceException(
                    message: "Failed service consumer access error occurred, contact support.",
                    innerException: timeoutException);

            var expectedConsumerAccessServiceException =
                new ConsumerAccessServiceException(
                    message: "Service error occurred, contact support.",
                    innerException: failedConsumerAccessServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(timeoutException);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    NhsNumber: randomNhsNumber,
                    CorrelationId: randomCorrelationId);

            ConsumerAccessServiceException actualConsumerAccessServiceException =
                await Assert.ThrowsAsync<ConsumerAccessServiceException>(
                    checkConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Exactly(2));

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnCheckConsumerAccessIfBrokerCallIsCancelledMidFlightAndLogItAsync()
        {
            // given
            string randomUserId = GetRandomString();
            string randomNhsNumber = GetRandomString();
            Guid randomCorrelationId = GetRandomGuid();
            using var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            var operationCancelledException = new OperationCanceledException(cancellationToken);

            var failedConsumerAccessServiceException =
                new FailedConsumerAccessServiceException(
                    message: "Failed service consumer access error occurred, contact support.",
                    innerException: operationCancelledException);

            var expectedConsumerAccessServiceException =
                new ConsumerAccessServiceException(
                    message: "Service error occurred, contact support.",
                    innerException: failedConsumerAccessServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    cancellationToken))
                        .ThrowsAsync(operationCancelledException);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    NhsNumber: randomNhsNumber,
                    CorrelationId: randomCorrelationId,
                    cancellationToken: cancellationToken);

            ConsumerAccessServiceException actualConsumerAccessServiceException =
                await Assert.ThrowsAsync<ConsumerAccessServiceException>(
                    checkConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Exactly(2));

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    cancellationToken),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnCheckConsumerAccessIfConsumerAccessReturnedIsNullAndLogItAsync()
        {
            // given
            string randomUserId = GetRandomString();
            string randomNhsNumber = GetRandomString();
            Guid randomCorrelationId = GetRandomGuid();
            ConsumerAccess nullConsumerAccess = null;

            var nullConsumerAccessServiceException =
                new NullConsumerAccessServiceException(message: "Consumer access is null.");

            var failedConsumerAccessServiceException =
                new FailedConsumerAccessServiceException(
                    message: "Failed service consumer access error occurred, contact support.",
                    innerException: nullConsumerAccessServiceException);

            var expectedConsumerAccessServiceException =
                new ConsumerAccessServiceException(
                    message: "Service error occurred, contact support.",
                    innerException: failedConsumerAccessServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(nullConsumerAccess);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    NhsNumber: randomNhsNumber,
                    CorrelationId: randomCorrelationId);

            ConsumerAccessServiceException actualConsumerAccessServiceException =
                await Assert.ThrowsAsync<ConsumerAccessServiceException>(
                    checkConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Exactly(2));

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
