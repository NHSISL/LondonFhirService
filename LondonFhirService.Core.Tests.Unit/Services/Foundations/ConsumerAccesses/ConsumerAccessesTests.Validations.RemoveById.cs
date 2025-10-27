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
        public async Task ShouldThrowValidationExceptionOnRemoveByIdConsumerAccessWithInvalidIdAndLogItAsync()
        {
            // given
            Guid invalidConsumerAccessId = Guid.Empty;

            var invalidConsumerAccessServiceException = new InvalidConsumerAccessServiceException(
                message: "Invalid consumer access. Please correct the errors and try again.");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ConsumerAccess.Id),
                values: "Id is invalid");

            var expectedConsumerAccessServiceValidationException =
                new ConsumerAccessServiceValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: invalidConsumerAccessServiceException);

            // when
            ValueTask<ConsumerAccess> removeByIdConsumerAccessTask = this.consumerAccessService
                .RemoveConsumerAccessByIdAsync(invalidConsumerAccessId);

            ConsumerAccessServiceValidationException actualConsumerAccessServiceValidationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceValidationException>(
                    testCode: removeByIdConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceValidationException.Should().BeEquivalentTo(expectedConsumerAccessServiceValidationException);
            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedConsumerAccessServiceValidationException))),
                    Times.Once());

            this.storageBroker.Verify(broker =>
                broker.SelectConsumerAccessByIdAsync(invalidConsumerAccessId),
                    Times.Never);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnRemoveByIdIfStorageConsumerAccessDoesNotExistAndLogItAsync()
        {
            // given
            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess();
            ConsumerAccess nonExistingConsumerAccess = randomConsumerAccess;
            ConsumerAccess nullConsumerAccess = null;

            var notFoundConsumerAccessException = new NotFoundConsumerAccessServiceException(
                message: $"Consumer access not found with Id: {nonExistingConsumerAccess.Id}");

            var expectedConsumerAccessServiceValidationException = new ConsumerAccessServiceValidationException(
                message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                innerException: notFoundConsumerAccessException);

            this.storageBroker.Setup(broker =>
                broker.SelectConsumerAccessByIdAsync(nonExistingConsumerAccess.Id))
                    .ReturnsAsync(nullConsumerAccess);

            // when
            ValueTask<ConsumerAccess> removeByIdConsumerAccessTask =
                this.consumerAccessService.RemoveConsumerAccessByIdAsync(nonExistingConsumerAccess.Id);

            ConsumerAccessServiceValidationException actualConsumerAccessVaildationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceValidationException>(
                    testCode: removeByIdConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessVaildationException.Should().BeEquivalentTo(expectedConsumerAccessServiceValidationException);

            this.storageBroker.Verify(broker =>
                broker.SelectConsumerAccessByIdAsync(nonExistingConsumerAccess.Id),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(
                   SameExceptionAs(expectedConsumerAccessServiceValidationException))),
                       Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }
    }
}
