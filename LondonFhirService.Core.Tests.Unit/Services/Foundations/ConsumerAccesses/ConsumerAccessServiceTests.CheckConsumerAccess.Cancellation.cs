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
    /// <summary>
    /// The exception tests next door drive cancellation by making the broker throw, which proves
    /// the mapping but not that the token is honoured. These drive it with a token that is
    /// genuinely cancelled on the way in.
    /// </summary>
    public partial class ConsumerAccessServiceTests
    {
        [Fact]
        public async Task ShouldThrowDependencyExceptionOnCheckConsumerAccessIfTokenIsAlreadyCancelledAndLogItAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            CancellationToken cancelledToken = cancellationTokenSource.Token;
            ValidateAccessRequest randomValidateAccessRequest = CreateRandomValidateAccessRequest();

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(randomValidateAccessRequest, cancelledToken);

            ConsumerAccessServiceDependencyException actualConsumerAccessServiceDependencyException =
                await Assert.ThrowsAsync<ConsumerAccessServiceDependencyException>(
                    testCode: checkConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceDependencyException.InnerException.Should()
                .BeOfType<CancelledConsumerAccessServiceException>();

            actualConsumerAccessServiceDependencyException.InnerException.InnerException.Should()
                .BeAssignableTo<OperationCanceledException>();

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
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
        public async Task ShouldThrowBeforeValidationOnCheckConsumerAccessIfTokenIsAlreadyCancelledAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            CancellationToken cancelledToken = cancellationTokenSource.Token;
            ValidateAccessRequest nullValidateAccessRequest = null;

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(nullValidateAccessRequest, cancelledToken);

            ConsumerAccessServiceDependencyException actualConsumerAccessServiceDependencyException =
                await Assert.ThrowsAsync<ConsumerAccessServiceDependencyException>(
                    testCode: checkConsumerAccessTask.AsTask);

            // then
            // The token is checked first, so an abandoned request is reported as cancelled rather
            // than as whatever happens to be wrong with its arguments.
            actualConsumerAccessServiceDependencyException.InnerException.Should()
                .BeOfType<CancelledConsumerAccessServiceException>();

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
                    Times.Once);

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()),
                        Times.Never);

            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
