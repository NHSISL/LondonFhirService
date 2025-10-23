// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.Consumers;
using LondonFhirService.Core.Models.Foundations.Consumers.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Consumers
{
    public partial class ConsumerServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfConsumerIsNullAndLogItAsync()
        {
            // given
            Consumer nullConsumer = null;

            var nullConsumerServiceException =
                new NullConsumerServiceException(message: "Consumer is null.");

            var expectedConsumerServiceValidationException =
                new ConsumerServiceValidationException(
                    message: "Consumer validation errors occurred, please try again.",
                    innerException: nullConsumerServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                    broker.ApplyAddAuditValuesAsync(nullConsumer))
                .ReturnsAsync(nullConsumer);

            // when
            ValueTask<Consumer> addConsumerTask =
                this.consumerService.AddConsumerAsync(nullConsumer);

            ConsumerServiceValidationException actualConsumerServiceValidationException =
                await Assert.ThrowsAsync<ConsumerServiceValidationException>(() =>
                    addConsumerTask.AsTask());

            // then
            actualConsumerServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                    broker.ApplyAddAuditValuesAsync(nullConsumer),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                    broker.LogErrorAsync(It.Is(SameExceptionAs(
                        expectedConsumerServiceValidationException))),
                Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnAddIfConsumerIsInvalidAndLogItAsync(string invalidText)
        {
            // given
            string randomUserId = GetRandomString();

            var invalidConsumer = new Consumer
            {
                Name = invalidText
            };

            var invalidConsumerServiceException =
                new InvalidConsumerServiceException(
                    message: "Invalid consumer. Please correct the errors and try again.");

            invalidConsumerServiceException.AddData(
                key: nameof(Consumer.Id),
                values: "Id is required");

            invalidConsumerServiceException.AddData(
                key: nameof(Consumer.UserId),
                values: "Text is required");

            invalidConsumerServiceException.AddData(
                key: nameof(Consumer.Name),
                values: "Text is required");

            invalidConsumerServiceException.AddData(
                key: nameof(Consumer.CreatedDate),
                values: "Date is required");

            invalidConsumerServiceException.AddData(
                key: nameof(Consumer.CreatedBy),
                values:
                    [
                        "Text is required",
                        $"Expected value to be '{randomUserId}' but found '{invalidConsumer.CreatedBy}'."
                    ]);

            invalidConsumerServiceException.AddData(
                key: nameof(Consumer.UpdatedDate),
                values: "Date is required");

            invalidConsumerServiceException.AddData(
                key: nameof(Consumer.UpdatedBy),
                values: "Text is required");

            var expectedConsumerServiceValidationException =
                new ConsumerServiceValidationException(
                    message: "Consumer validation errors occurred, please try again.",
                    innerException: invalidConsumerServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(invalidConsumer))
                    .ReturnsAsync(invalidConsumer);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<Consumer> addConsumerTask =
                this.consumerService.AddConsumerAsync(invalidConsumer);

            ConsumerServiceValidationException actualConsumerServiceValidationException =
                await Assert.ThrowsAsync<ConsumerServiceValidationException>(() =>
                    addConsumerTask.AsTask());

            // then
            actualConsumerServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(invalidConsumer),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once());

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerServiceValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertConsumerAsync(It.IsAny<Consumer>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfConsumerHasInvalidLengthProperty()
        {
            // given
            string randomUserId = GetRandomString();
            var invalidConsumer = CreateRandomConsumer(GetRandomDateTimeOffset(), userId: randomUserId);
            invalidConsumer.UserId = GetRandomStringWithLengthOf(256);
            invalidConsumer.Name = GetRandomStringWithLengthOf(256);

            var invalidConsumerServiceException =
                new InvalidConsumerServiceException(
                    message: "Invalid consumer. Please correct the errors and try again.");

            invalidConsumerServiceException.AddData(
                key: nameof(Consumer.UserId),
                values: $"Text exceed max length of {invalidConsumer.UserId.Length - 1} characters");

            invalidConsumerServiceException.AddData(
                key: nameof(Consumer.Name),
                values: $"Text exceed max length of {invalidConsumer.Name.Length - 1} characters");

            var expectedConsumerServiceValidationException =
                new ConsumerServiceValidationException(
                    message: "Consumer validation errors occurred, please try again.",
                    innerException: invalidConsumerServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(invalidConsumer))
                    .ReturnsAsync(invalidConsumer);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<Consumer> addConsumerTask =
                this.consumerService.AddConsumerAsync(invalidConsumer);

            ConsumerServiceValidationException actualConsumerServiceValidationException =
                await Assert.ThrowsAsync<ConsumerServiceValidationException>(
                    addConsumerTask.AsTask);

            // then
            actualConsumerServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(invalidConsumer),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once());

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerServiceValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertConsumerAsync(It.IsAny<Consumer>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfCreateAndUpdateDatesIsNotSameAndLogItAsync()
        {
            // given
            int randomNumber = GetRandomNumber();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();
            Consumer randomConsumer = CreateRandomConsumer(randomDateTimeOffset, userId: randomUserId);
            Consumer invalidConsumer = randomConsumer;

            invalidConsumer.UpdatedDate =
                invalidConsumer.CreatedDate.AddDays(randomNumber);

            var invalidConsumerServiceException =
                new InvalidConsumerServiceException(
                    message: "Invalid consumer. Please correct the errors and try again.");

            invalidConsumerServiceException.AddData(
                key: nameof(Consumer.UpdatedDate),
                values: $"Date is not the same as {nameof(Consumer.CreatedDate)}");

            var expectedConsumerServiceValidationException =
                new ConsumerServiceValidationException(
                    message: "Consumer validation errors occurred, please try again.",
                    innerException: invalidConsumerServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(invalidConsumer))
                    .ReturnsAsync(invalidConsumer);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<Consumer> addConsumerTask =
                this.consumerService.AddConsumerAsync(invalidConsumer);

            ConsumerServiceValidationException actualConsumerServiceValidationException =
                await Assert.ThrowsAsync<ConsumerServiceValidationException>(() =>
                    addConsumerTask.AsTask());

            // then
            actualConsumerServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(invalidConsumer),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once());

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerServiceValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertConsumerAsync(It.IsAny<Consumer>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfCreateAndUpdateUsersIsNotSameAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();
            Consumer randomConsumer = CreateRandomConsumer(randomDateTimeOffset, userId: randomUserId);
            Consumer invalidConsumer = randomConsumer.DeepClone();
            invalidConsumer.UpdatedBy = Guid.NewGuid().ToString();

            var invalidConsumerServiceException =
                new InvalidConsumerServiceException(
                    message: "Invalid consumer. Please correct the errors and try again.");

            invalidConsumerServiceException.AddData(
                key: nameof(Consumer.UpdatedBy),
                values: $"Text is not the same as {nameof(Consumer.CreatedBy)}");

            var expectedConsumerServiceValidationException =
                new ConsumerServiceValidationException(
                    message: "Consumer validation errors occurred, please try again.",
                    innerException: invalidConsumerServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(invalidConsumer))
                    .ReturnsAsync(invalidConsumer);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<Consumer> addConsumerTask =
                this.consumerService.AddConsumerAsync(invalidConsumer);

            ConsumerServiceValidationException actualConsumerServiceValidationException =
                await Assert.ThrowsAsync<ConsumerServiceValidationException>(() =>
                    addConsumerTask.AsTask());

            // then
            actualConsumerServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(invalidConsumer),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once());

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerServiceValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertConsumerAsync(It.IsAny<Consumer>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(MinutesBeforeOrAfter))]
        public async Task ShouldThrowValidationExceptionOnAddIfCreatedDateIsNotRecentAndLogItAsync(
            int minutesBeforeOrAfter)
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();

            DateTimeOffset invalidDateTime =
                randomDateTimeOffset.AddMinutes(minutesBeforeOrAfter);

            DateTimeOffset invalidDate = randomDateTimeOffset.AddMinutes(minutesBeforeOrAfter);
            DateTimeOffset startDate = randomDateTimeOffset.AddSeconds(-90);
            DateTimeOffset endDate = randomDateTimeOffset.AddSeconds(0);
            string randomUserId = GetRandomString();
            Consumer randomConsumer = CreateRandomConsumer(invalidDateTime, userId: randomUserId);
            Consumer invalidConsumer = randomConsumer;

            var invalidConsumerServiceException =
                new InvalidConsumerServiceException(
                    message: "Invalid consumer. Please correct the errors and try again.");

            invalidConsumerServiceException.AddData(
                key: nameof(Consumer.CreatedDate),
                values:
                    $"Date is not recent. Expected a value between {startDate} and {endDate} but found {invalidDate}");

            var expectedConsumerServiceValidationException =
                new ConsumerServiceValidationException(
                    message: "Consumer validation errors occurred, please try again.",
                    innerException: invalidConsumerServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(invalidConsumer))
                    .ReturnsAsync(invalidConsumer);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<Consumer> addConsumerTask =
                this.consumerService.AddConsumerAsync(invalidConsumer);

            ConsumerServiceValidationException actualConsumerServiceValidationException =
                await Assert.ThrowsAsync<ConsumerServiceValidationException>(() =>
                    addConsumerTask.AsTask());

            // then
            actualConsumerServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(invalidConsumer),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once());

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerServiceValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertConsumerAsync(It.IsAny<Consumer>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
