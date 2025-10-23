// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ConsumerAccesses
{
    public partial class ConsumerAccessesTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnRetrieveByIdWhenConsumerAccessIdIsInvalidAndLogItAsync()
        {
            // given
            Guid invalidConsumerAccessId = Guid.Empty;

            var invalidConsumerAccessException = new InvalidConsumerAccessException(
                message: "Invalid consumer access. Please correct the errors and try again.");

            invalidConsumerAccessException.AddData(
                key: nameof(ConsumerAccess.Id),
                values: "Id is invalid");

            var expectedConsumerAccessValidationException =
                new ConsumerAccessValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: invalidConsumerAccessException);

            // when
            ValueTask<ConsumerAccess> retrieveByIdConsumerAccessTask =
                this.consumerAccessService.RetrieveConsumerAccessByIdAsync(invalidConsumerAccessId);

            ConsumerAccessValidationException actualConsumerAccessValidationException =
                await Assert.ThrowsAsync<ConsumerAccessValidationException>(
                    testCode: retrieveByIdConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessValidationException.Should().BeEquivalentTo(expectedConsumerAccessValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessValidationException))), Times.Once());

            this.storageBroker.Verify(broker =>
                broker.SelectConsumerAccessByIdAsync(invalidConsumerAccessId),
                    Times.Never);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnRetrieveByIdIfConsumerAccessNotFoundAndLogItAsync()
        {
            // given
            Guid someConsumerAccessId = Guid.NewGuid();
            ConsumerAccess nullConsumerAccess = null;

            var notFoundConsumerAccessException = new NotFoundConsumerAccessException(
                message: $"Consumer access not found with Id: {someConsumerAccessId}");

            var expectedConsumerAccessValidationException = new ConsumerAccessValidationException(
                message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                innerException: notFoundConsumerAccessException);

            this.storageBroker.Setup(broker =>
                broker.SelectConsumerAccessByIdAsync(someConsumerAccessId))
                    .ReturnsAsync(nullConsumerAccess);

            // when
            ValueTask<ConsumerAccess> retrieveByIdConsumerAccessTask =
                this.consumerAccessService.RetrieveConsumerAccessByIdAsync(someConsumerAccessId);

            ConsumerAccessValidationException actualConsumerAccessValidationException =
                await Assert.ThrowsAsync<ConsumerAccessValidationException>(
                    testCode: retrieveByIdConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessValidationException.Should().BeEquivalentTo(expectedConsumerAccessValidationException);

            this.storageBroker.Verify(broker =>
                broker.SelectConsumerAccessByIdAsync(someConsumerAccessId),
                    Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessValidationException))), Times.Once());

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }
    }
}
