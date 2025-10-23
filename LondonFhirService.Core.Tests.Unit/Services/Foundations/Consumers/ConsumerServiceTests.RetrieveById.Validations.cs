// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Consumers;
using LondonFhirService.Core.Models.Foundations.Consumers.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Consumers
{
    public partial class ConsumerServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnRetrieveByIdIfIdIsInvalidAndLogItAsync()
        {
            // given
            var invalidConsumerId = Guid.Empty;

            var invalidConsumerServiceException =
                new InvalidConsumerServiceException(
                    message: "Invalid consumer. Please correct the errors and try again.");

            invalidConsumerServiceException.AddData(
                key: nameof(Consumer.Id),
                values: "Id is required");

            var expectedConsumerServiceValidationException =
                new ConsumerServiceValidationException(
                    message: "Consumer validation errors occurred, please try again.",
                    innerException: invalidConsumerServiceException);

            // when
            ValueTask<Consumer> retrieveConsumerByIdTask =
                this.consumerService.RetrieveConsumerByIdAsync(invalidConsumerId);

            ConsumerServiceValidationException actualConsumerServiceValidationException =
                await Assert.ThrowsAsync<ConsumerServiceValidationException>(
                    retrieveConsumerByIdTask.AsTask);

            // then
            actualConsumerServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerServiceValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedConsumerServiceValidationException))),
                    Times.Once());

            this.storageBrokerMock.Verify(broker =>
                broker.SelectConsumerByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowNotFoundExceptionOnRetrieveByIdIfConsumerIsNotFoundAndLogItAsync()
        {
            //given
            Guid someConsumerId = Guid.NewGuid();
            Consumer noConsumer = null;

            var notFoundConsumerServiceException = new NotFoundConsumerServiceException(
                $"Couldn't find consumer with consumerId: {someConsumerId}.");

            var expectedConsumerServiceValidationException =
                new ConsumerServiceValidationException(
                    message: "Consumer validation errors occurred, please try again.",
                    innerException: notFoundConsumerServiceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectConsumerByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(noConsumer);

            //when
            ValueTask<Consumer> retrieveConsumerByIdTask =
                this.consumerService.RetrieveConsumerByIdAsync(someConsumerId);

            ConsumerServiceValidationException actualConsumerServiceValidationException =
                await Assert.ThrowsAsync<ConsumerServiceValidationException>(
                    retrieveConsumerByIdTask.AsTask);

            //then
            actualConsumerServiceValidationException.Should().BeEquivalentTo(expectedConsumerServiceValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectConsumerByIdAsync(It.IsAny<Guid>()),
                    Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedConsumerServiceValidationException))),
                    Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
