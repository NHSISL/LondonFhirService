// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ConsumerAccesses
{
    public partial class ConsumerAccessesTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnCheckConsumerAccessIfConsumerUserIdIsInvalidAndLogItAsync(
            string invalidConsumerUserId)
        {
            // given
            string randomNhsNumber = GetRandomString();
            Guid randomCorrelationId = GetRandomGuid();

            var invalidConsumerAccessServiceException =
                new InvalidConsumerAccessServiceException(
                    message: "Invalid consumer access. Please correct the errors and try again.");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ValidateAccessRequest.ConsumerUserId),
                values: "Text is invalid");

            var expectedConsumerAccessServiceValidationException =
                new ConsumerAccessServiceValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: invalidConsumerAccessServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(invalidConsumerUserId);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    NhsNumber: randomNhsNumber,
                    CorrelationId: randomCorrelationId);

            ConsumerAccessServiceValidationException actualConsumerAccessServiceValidationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceValidationException>(
                    checkConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Exactly(2));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceValidationException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnCheckConsumerAccessIfNhsNumberIsInvalidAndLogItAsync(
            string invalidNhsNumber)
        {
            // given
            string randomUserId = GetRandomString();
            Guid randomCorrelationId = GetRandomGuid();

            var invalidConsumerAccessServiceException =
                new InvalidConsumerAccessServiceException(
                    message: "Invalid consumer access. Please correct the errors and try again.");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ValidateAccessRequest.NhsNumber),
                values: "Text is invalid");

            var expectedConsumerAccessServiceValidationException =
                new ConsumerAccessServiceValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: invalidConsumerAccessServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    NhsNumber: invalidNhsNumber,
                    CorrelationId: randomCorrelationId);

            ConsumerAccessServiceValidationException actualConsumerAccessServiceValidationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceValidationException>(
                    checkConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Exactly(2));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceValidationException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnCheckConsumerAccessIfCorrelationIdIsInvalidAndLogItAsync()
        {
            // given
            string randomUserId = GetRandomString();
            string randomNhsNumber = GetRandomString();
            Guid invalidCorrelationId = Guid.Empty;

            var invalidConsumerAccessServiceException =
                new InvalidConsumerAccessServiceException(
                    message: "Invalid consumer access. Please correct the errors and try again.");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ValidateAccessRequest.CorrelationId),
                values: "Id is invalid");

            var expectedConsumerAccessServiceValidationException =
                new ConsumerAccessServiceValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: invalidConsumerAccessServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<ConsumerAccess> checkConsumerAccessTask =
                this.consumerAccessService.CheckConsumerAccessAsync(
                    NhsNumber: randomNhsNumber,
                    CorrelationId: invalidCorrelationId);

            ConsumerAccessServiceValidationException actualConsumerAccessServiceValidationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceValidationException>(
                    checkConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Exactly(2));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceValidationException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.consumerAccessBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
