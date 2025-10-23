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
        public async Task ShouldThrowValidationExceptionOnModifyIfConsumerIsNullAndLogItAsync()
        {
            // given
            Consumer nullConsumer = null;
            var nullConsumerServiceException = new NullConsumerServiceException(message: "Consumer is null.");

            var expectedConsumerServiceValidationException =
                new ConsumerServiceValidationException(
                    message: "Consumer validation errors occurred, please try again.",
                    innerException: nullConsumerServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(nullConsumer))
                    .ReturnsAsync(nullConsumer);

            // when
            ValueTask<Consumer> modifyConsumerTask =
                this.consumerService.ModifyConsumerAsync(nullConsumer);

            ConsumerServiceValidationException actualConsumerServiceValidationException =
                await Assert.ThrowsAsync<ConsumerServiceValidationException>(
                    modifyConsumerTask.AsTask);

            // then
            actualConsumerServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(nullConsumer),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerServiceValidationException))),
                        Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateConsumerAsync(It.IsAny<Consumer>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnModifyIfConsumerIsInvalidAndLogItAsync(string invalidText)
        {
            // given
            string randomUserId = GetRandomString();

            var invalidConsumer = new Consumer
            {
                Name = invalidText,
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
                values: "Text is required");

            invalidConsumerServiceException.AddData(
                key: nameof(Consumer.UpdatedDate),
                values:
                [
                    "Date is required",
                    $"Date is the same as {nameof(Consumer.CreatedDate)}"
                ]);

            invalidConsumerServiceException.AddData(
                key: nameof(Consumer.UpdatedBy),
                values:
                [
                    "Text is required",
                    $"Expected value to be '{randomUserId}' but found '{invalidConsumer.UpdatedBy}'."
                ]);

            var expectedConsumerServiceValidationException =
                new ConsumerServiceValidationException(
                    message: "Consumer validation errors occurred, please try again.",
                    innerException: invalidConsumerServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidConsumer))
                    .ReturnsAsync(invalidConsumer);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<Consumer> modifyConsumerTask =
                this.consumerService.ModifyConsumerAsync(invalidConsumer);

            ConsumerServiceValidationException actualConsumerServiceValidationException =
                await Assert.ThrowsAsync<ConsumerServiceValidationException>(
                    modifyConsumerTask.AsTask);

            //then
            actualConsumerServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidConsumer),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerServiceValidationException))),
                        Times.Once());

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateConsumerAsync(It.IsAny<Consumer>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfConsumerHasInvalidLengthProperty()
        {
            // given
            string randomUserId = GetRandomString();
            var invalidConsumer = CreateRandomModifyConsumer(GetRandomDateTimeOffset(), userId: randomUserId);
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
                broker.ApplyModifyAuditValuesAsync(invalidConsumer))
                    .ReturnsAsync(invalidConsumer);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<Consumer> modifyConsumerTask =
                this.consumerService.ModifyConsumerAsync(invalidConsumer);

            ConsumerServiceValidationException actualConsumerServiceValidationException =
                await Assert.ThrowsAsync<ConsumerServiceValidationException>(
                    modifyConsumerTask.AsTask);

            // then
            actualConsumerServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidConsumer),
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
        public async Task ShouldThrowValidationExceptionOnModifyIfUpdatedDateIsSameAsCreatedDateAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();

            Consumer randomConsumer =
                CreateRandomConsumer(dateTimeOffset: randomDateTimeOffset, userId: randomUserId);

            Consumer invalidConsumer = randomConsumer;

            var invalidConsumerServiceException =
                new InvalidConsumerServiceException(
                    message: "Invalid consumer. Please correct the errors and try again.");

            invalidConsumerServiceException.AddData(
                key: nameof(Consumer.UpdatedDate),
                values: $"Date is the same as {nameof(Consumer.CreatedDate)}");

            var expectedConsumerServiceValidationException =
                new ConsumerServiceValidationException(
                    message: "Consumer validation errors occurred, please try again.",
                    innerException: invalidConsumerServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidConsumer))
                    .ReturnsAsync(invalidConsumer);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<Consumer> modifyConsumerTask = this.consumerService.ModifyConsumerAsync(invalidConsumer);

            ConsumerServiceValidationException actualConsumerServiceValidationException =
                await Assert.ThrowsAsync<ConsumerServiceValidationException>(
                    modifyConsumerTask.AsTask);

            // then
            actualConsumerServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidConsumer),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerServiceValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectConsumerByIdAsync(invalidConsumer.Id),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(MinutesBeforeOrAfter))]
        public async Task ShouldThrowValidationExceptionOnModifyIfUpdatedDateIsNotRecentAndLogItAsync(int minutes)
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset invalidDate = randomDateTimeOffset.AddMinutes(minutes);
            DateTimeOffset startDate = randomDateTimeOffset.AddSeconds(-90);
            DateTimeOffset endDate = randomDateTimeOffset.AddSeconds(0);
            string randomUserId = GetRandomString();

            Consumer randomConsumer =
                CreateRandomConsumer(dateTimeOffset: randomDateTimeOffset, userId: randomUserId);

            randomConsumer.UpdatedDate = randomDateTimeOffset.AddMinutes(minutes);

            var invalidConsumerServiceException =
                new InvalidConsumerServiceException(
                    message: "Invalid consumer. Please correct the errors and try again.");

            invalidConsumerServiceException.AddData(
                key: nameof(Consumer.UpdatedDate),
                values:
                    $"Date is not recent. Expected a value between {startDate} and {endDate} but found {invalidDate}");

            var expectedConsumerValidatonException =
                new ConsumerServiceValidationException(
                    message: "Consumer validation errors occurred, please try again.",
                    innerException: invalidConsumerServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(randomConsumer))
                    .ReturnsAsync(randomConsumer);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<Consumer> modifyConsumerTask =
                this.consumerService.ModifyConsumerAsync(randomConsumer);

            ConsumerServiceValidationException actualConsumerServiceValidationException =
                await Assert.ThrowsAsync<ConsumerServiceValidationException>(
                    modifyConsumerTask.AsTask);

            // then
            actualConsumerServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerValidatonException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(randomConsumer),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerValidatonException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectConsumerByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfConsumerDoesNotExistAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();

            Consumer randomConsumer = CreateRandomModifyConsumer(
                dateTimeOffset: randomDateTimeOffset, userId: randomUserId);

            Consumer nonExistConsumer = randomConsumer;
            Consumer nullConsumer = null;

            var notFoundConsumerServiceException = new NotFoundConsumerServiceException(
                message: $"Couldn't find consumer with consumerId: {nonExistConsumer.Id}.");

            var expectedConsumerServiceValidationException =
                new ConsumerServiceValidationException(
                    message: "Consumer validation errors occurred, please try again.",
                    innerException: notFoundConsumerServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(nonExistConsumer))
                    .ReturnsAsync(nonExistConsumer);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectConsumerByIdAsync(nonExistConsumer.Id))
                    .ReturnsAsync(nullConsumer);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<Consumer> modifyConsumerTask =
                this.consumerService.ModifyConsumerAsync(nonExistConsumer);

            ConsumerServiceValidationException actualConsumerServiceValidationException =
                await Assert.ThrowsAsync<ConsumerServiceValidationException>(
                    modifyConsumerTask.AsTask);

            // then
            actualConsumerServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(nonExistConsumer),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectConsumerByIdAsync(nonExistConsumer.Id),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
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

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfStorageCreatedDateNotSameAsCreatedDateAndLogItAsync()
        {
            // given
            int randomNumber = GetRandomNegativeNumber();
            int randomMinutes = randomNumber;
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();

            Consumer randomConsumer = CreateRandomModifyConsumer(
                dateTimeOffset: randomDateTimeOffset, userId: randomUserId);

            Consumer invalidConsumer = randomConsumer.DeepClone();
            Consumer storageConsumer = invalidConsumer.DeepClone();
            storageConsumer.CreatedDate = storageConsumer.CreatedDate.AddMinutes(randomMinutes);
            storageConsumer.UpdatedDate = storageConsumer.UpdatedDate.AddMinutes(randomMinutes);

            var invalidConsumerServiceException =
                new InvalidConsumerServiceException(
                    message: "Invalid consumer. Please correct the errors and try again.");

            invalidConsumerServiceException.AddData(
                key: nameof(Consumer.CreatedDate),
                values: $"Date is not the same as {nameof(Consumer.CreatedDate)}");

            var expectedConsumerServiceValidationException =
                new ConsumerServiceValidationException(
                    message: "Consumer validation errors occurred, please try again.",
                    innerException: invalidConsumerServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidConsumer))
                    .ReturnsAsync(invalidConsumer);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectConsumerByIdAsync(invalidConsumer.Id))
                    .ReturnsAsync(storageConsumer);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(invalidConsumer, storageConsumer))
                    .ReturnsAsync(invalidConsumer);

            // when
            ValueTask<Consumer> modifyConsumerTask =
                this.consumerService.ModifyConsumerAsync(invalidConsumer);

            ConsumerServiceValidationException actualConsumerServiceValidationException =
                await Assert.ThrowsAsync<ConsumerServiceValidationException>(
                    modifyConsumerTask.AsTask);

            // then
            actualConsumerServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidConsumer),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectConsumerByIdAsync(invalidConsumer.Id),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(invalidConsumer, storageConsumer),
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

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfCreatedUserDoesntMatchStorageAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();

            Consumer randomConsumer =
                CreateRandomModifyConsumer(dateTimeOffset: randomDateTimeOffset, userId: randomUserId);

            Consumer invalidConsumer = randomConsumer.DeepClone();
            Consumer storageConsumer = invalidConsumer.DeepClone();
            invalidConsumer.CreatedBy = Guid.NewGuid().ToString();
            storageConsumer.UpdatedDate = storageConsumer.CreatedDate;

            var invalidConsumerServiceException =
                new InvalidConsumerServiceException(
                    message: "Invalid consumer. Please correct the errors and try again.");

            invalidConsumerServiceException.AddData(
                key: nameof(Consumer.CreatedBy),
                values: $"Text is not the same as {nameof(Consumer.CreatedBy)}");

            var expectedConsumerServiceValidationException =
                new ConsumerServiceValidationException(
                    message: "Consumer validation errors occurred, please try again.",
                    innerException: invalidConsumerServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidConsumer))
                    .ReturnsAsync(invalidConsumer);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectConsumerByIdAsync(invalidConsumer.Id))
                    .ReturnsAsync(storageConsumer);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(invalidConsumer, storageConsumer))
                    .ReturnsAsync(invalidConsumer);

            // when
            ValueTask<Consumer> modifyConsumerTask =
                this.consumerService.ModifyConsumerAsync(invalidConsumer);

            ConsumerServiceValidationException actualConsumerServiceValidationException =
                await Assert.ThrowsAsync<ConsumerServiceValidationException>(
                    modifyConsumerTask.AsTask);

            // then
            actualConsumerServiceValidationException.Should().BeEquivalentTo(expectedConsumerServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidConsumer),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectConsumerByIdAsync(invalidConsumer.Id),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(invalidConsumer, storageConsumer),
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

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfStorageUpdatedDateSameAsUpdatedDateAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();

            Consumer randomConsumer =
                CreateRandomModifyConsumer(dateTimeOffset: randomDateTimeOffset, userId: randomUserId);

            Consumer invalidConsumer = randomConsumer;
            Consumer storageConsumer = randomConsumer.DeepClone();

            var invalidConsumerServiceException =
                new InvalidConsumerServiceException(
                    message: "Invalid consumer. Please correct the errors and try again.");

            invalidConsumerServiceException.AddData(
                key: nameof(Consumer.UpdatedDate),
                values: $"Date is the same as {nameof(Consumer.UpdatedDate)}");

            var expectedConsumerServiceValidationException =
                new ConsumerServiceValidationException(
                    message: "Consumer validation errors occurred, please try again.",
                    innerException: invalidConsumerServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidConsumer))
                    .ReturnsAsync(invalidConsumer);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectConsumerByIdAsync(invalidConsumer.Id))
                    .ReturnsAsync(storageConsumer);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(invalidConsumer, storageConsumer))
                    .ReturnsAsync(invalidConsumer);

            // when
            ValueTask<Consumer> modifyConsumerTask =
                this.consumerService.ModifyConsumerAsync(invalidConsumer);

            // then
            await Assert.ThrowsAsync<ConsumerServiceValidationException>(
                modifyConsumerTask.AsTask);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidConsumer),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerServiceValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectConsumerByIdAsync(invalidConsumer.Id),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(invalidConsumer, storageConsumer),
                    Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
