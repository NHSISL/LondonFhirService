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
        [Fact]
        public async Task ShouldThrowValidationExceptionOnCheckConsumerAccessIfRequestIsNullAndLogItAsync()
        {
            // given
            ValidateAccessRequest nullValidateAccessRequest = null;

            var nullConsumerAccessServiceException =
                new NullConsumerAccessServiceException(
                    message: "Consumer access is null.");

            var expectedConsumerAccessServiceValidationException =
                new ConsumerAccessServiceValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: nullConsumerAccessServiceException);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    nullValidateAccessRequest, TestContext.Current.CancellationToken);

            ConsumerAccessServiceValidationException actualConsumerAccessServiceValidationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceValidationException>(
                    testCode: checkConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceValidationException))),
                        Times.Once);

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()),
                        Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnCheckConsumerAccessIfRequestIsInvalidAndLogItAsync(
            string invalidText)
        {
            // given
            var invalidValidateAccessRequest = new ValidateAccessRequest
            {
                ConsumerUserId = invalidText,
                NhsNumber = invalidText,
                CorrelationId = Guid.Empty
            };

            var invalidConsumerAccessServiceException =
                new InvalidConsumerAccessServiceException(
                    message: "Invalid consumer access. Please correct the errors and try again.");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ValidateAccessRequest.ConsumerUserId),
                values: "Text is invalid");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ValidateAccessRequest.NhsNumber),
                values: "Text is invalid");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ValidateAccessRequest.CorrelationId),
                values: "Id is invalid");

            var expectedConsumerAccessServiceValidationException =
                new ConsumerAccessServiceValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: invalidConsumerAccessServiceException);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    invalidValidateAccessRequest, TestContext.Current.CancellationToken);

            ConsumerAccessServiceValidationException actualConsumerAccessServiceValidationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceValidationException>(
                    testCode: checkConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceValidationException))),
                        Times.Once);

            this.consumerAccessBrokerMock.Verify(broker =>
                broker.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()),
                        Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
