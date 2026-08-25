// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ConsumerAccesses
{
    public partial class ConsumerAccessServiceTests
    {
        [Theory]
        [MemberData(nameof(DependencyExceptions))]
        public async Task ShouldThrowCriticalDependencyExceptionOnCheckConsumerAccessAndLogItAsync(
            Exception dependencyException)
        {
            // given
            ValidateAccessRequest randomValidateAccessRequest = CreateRandomValidateAccessRequest();
            ValidateAccessRequest inputValidateAccessRequest = randomValidateAccessRequest;

            var failedConsumerAccessDependencyException =
                new FailedConsumerAccessDependencyException(
                    message: "Failed consumer access dependency error occurred, contact support.",
                    innerException: dependencyException,
                    data: dependencyException.Data);

            var expectedConsumerAccessServiceDependencyException =
                new ConsumerAccessServiceDependencyException(
                    message: "ConsumerAccess dependency error occurred, contact support.",
                    innerException: failedConsumerAccessDependencyException);

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(dependencyException);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, TestContext.Current.CancellationToken);

            ConsumerAccessServiceDependencyException actualConsumerAccessServiceDependencyException =
                await Assert.ThrowsAsync<ConsumerAccessServiceDependencyException>(
                    testCode: checkConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceDependencyException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceDependencyException);

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceDependencyException))),
                        Times.Once);

            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(TimeoutExceptions))]
        public async Task ShouldThrowDependencyExceptionOnCheckConsumerAccessIfTimesOutAndLogItAsync(
            Exception timeoutException)
        {
            // given
            ValidateAccessRequest randomValidateAccessRequest = CreateRandomValidateAccessRequest();
            ValidateAccessRequest inputValidateAccessRequest = randomValidateAccessRequest;

            var timedOutConsumerAccessServiceException =
                new TimedOutConsumerAccessServiceException(
                    message: "Consumer access request timed out, please try again.",
                    innerException: timeoutException,
                    data: timeoutException.Data);

            var expectedConsumerAccessServiceDependencyException =
                new ConsumerAccessServiceDependencyException(
                    message: "ConsumerAccess dependency error occurred, contact support.",
                    innerException: timedOutConsumerAccessServiceException);

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(timeoutException);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, TestContext.Current.CancellationToken);

            ConsumerAccessServiceDependencyException actualConsumerAccessServiceDependencyException =
                await Assert.ThrowsAsync<ConsumerAccessServiceDependencyException>(
                    testCode: checkConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceDependencyException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceDependencyException);

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceDependencyException))),
                        Times.Once);

            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(CancellationExceptions))]
        public async Task ShouldThrowDependencyExceptionOnCheckConsumerAccessIfCancelledAndLogItAsync(
            Exception cancellationException)
        {
            // given
            ValidateAccessRequest randomValidateAccessRequest = CreateRandomValidateAccessRequest();
            ValidateAccessRequest inputValidateAccessRequest = randomValidateAccessRequest;

            var cancelledConsumerAccessServiceException =
                new CancelledConsumerAccessServiceException(
                    message: "Consumer access request was cancelled, please try again.",
                    innerException: cancellationException,
                    data: cancellationException.Data);

            var expectedConsumerAccessServiceDependencyException =
                new ConsumerAccessServiceDependencyException(
                    message: "ConsumerAccess dependency error occurred, contact support.",
                    innerException: cancelledConsumerAccessServiceException);

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(cancellationException);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, TestContext.Current.CancellationToken);

            ConsumerAccessServiceDependencyException actualConsumerAccessServiceDependencyException =
                await Assert.ThrowsAsync<ConsumerAccessServiceDependencyException>(
                    testCode: checkConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceDependencyException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceDependencyException);

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceDependencyException))),
                        Times.Once);

            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnCheckConsumerAccessIfTokenIsAlreadyCancelledAndLogItAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            CancellationToken cancelledToken = cancellationTokenSource.Token;
            ValidateAccessRequest randomValidateAccessRequest = CreateRandomValidateAccessRequest();
            ValidateAccessRequest inputValidateAccessRequest = randomValidateAccessRequest;
            var operationCanceledException = new OperationCanceledException(cancelledToken);

            var cancelledConsumerAccessServiceException =
                new CancelledConsumerAccessServiceException(
                    message: "Consumer access request was cancelled, please try again.",
                    innerException: operationCanceledException,
                    data: operationCanceledException.Data);

            var expectedConsumerAccessServiceDependencyException =
                new ConsumerAccessServiceDependencyException(
                    message: "ConsumerAccess dependency error occurred, contact support.",
                    innerException: cancelledConsumerAccessServiceException);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, cancelledToken);

            ConsumerAccessServiceDependencyException actualConsumerAccessServiceDependencyException =
                await Assert.ThrowsAsync<ConsumerAccessServiceDependencyException>(
                    testCode: checkConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceDependencyException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceDependencyException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceDependencyException))),
                        Times.Once);

            // The outbound call is never made. A caller that has already given up should not
            // cost a round trip to the access service.
            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()),
                        Times.Never);

            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionBeforeValidationOnCheckConsumerAccessIfTokenIsAlreadyCancelledAndLogItAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            CancellationToken cancelledToken = cancellationTokenSource.Token;
            ValidateAccessRequest nullValidateAccessRequest = null;
            var operationCanceledException = new OperationCanceledException(cancelledToken);

            var cancelledConsumerAccessServiceException =
                new CancelledConsumerAccessServiceException(
                    message: "Consumer access request was cancelled, please try again.",
                    innerException: operationCanceledException,
                    data: operationCanceledException.Data);

            var expectedConsumerAccessServiceDependencyException =
                new ConsumerAccessServiceDependencyException(
                    message: "ConsumerAccess dependency error occurred, contact support.",
                    innerException: cancelledConsumerAccessServiceException);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    nullValidateAccessRequest, cancelledToken);

            ConsumerAccessServiceDependencyException actualConsumerAccessServiceDependencyException =
                await Assert.ThrowsAsync<ConsumerAccessServiceDependencyException>(
                    testCode: checkConsumerAccessTask.AsTask);

            // then
            // The token is checked first, so an abandoned request is reported as cancelled rather
            // than as whatever happens to be wrong with its arguments.
            actualConsumerAccessServiceDependencyException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceDependencyException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceDependencyException))),
                        Times.Once);

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()),
                        Times.Never);

            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnCheckConsumerAccessIfServiceErrorOccursAndLogItAsync()
        {
            // given
            ValidateAccessRequest randomValidateAccessRequest = CreateRandomValidateAccessRequest();
            ValidateAccessRequest inputValidateAccessRequest = randomValidateAccessRequest;
            var serviceException = new Exception();

            var failedConsumerAccessServiceException =
                new FailedConsumerAccessServiceException(
                    message: "Failed service consumer access error occurred, contact support.",
                    innerException: serviceException);

            var expectedConsumerAccessServiceException =
                new ConsumerAccessServiceException(
                    message: "Service error occurred, contact support.",
                    innerException: failedConsumerAccessServiceException);

            this.consumerAccessBrokerMock.Setup(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(serviceException);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, TestContext.Current.CancellationToken);

            ConsumerAccessServiceException actualConsumerAccessServiceException =
                await Assert.ThrowsAsync<ConsumerAccessServiceException>(
                    testCode: checkConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceException);

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(
                    inputValidateAccessRequest, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceException))),
                        Times.Once);

            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
