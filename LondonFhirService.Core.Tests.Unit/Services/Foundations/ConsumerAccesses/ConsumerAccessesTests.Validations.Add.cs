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
        public async Task ShouldThrowValidationExceptionOnAddConsumerAccessAsync()
        {
            // given
            ConsumerAccess nullConsumerAccess = null;
            var nullConsumerAccessServiceException = new NullConsumerAccessServiceException(message: "Consumer access is null.");

            securityAuditBrokerMock.Setup(service =>
                service.ApplyAddAuditValuesAsync(nullConsumerAccess))
                    .ReturnsAsync(nullConsumerAccess);

            var expectedConsumerAccessServiceValidationException =
                new ConsumerAccessServiceValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: nullConsumerAccessServiceException);

            // when
            ValueTask<ConsumerAccess> addConsumerAccessTask = consumerAccessService.
                AddConsumerAccessAsync(nullConsumerAccess);

            ConsumerAccessServiceValidationException actualConsumerAccessServiceValidationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceValidationException>(
                    testCode: addConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceValidationException.Should().BeEquivalentTo(expectedConsumerAccessServiceValidationException);

            securityAuditBrokerMock.Verify(service =>
                service.ApplyAddAuditValuesAsync(nullConsumerAccess),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedConsumerAccessServiceValidationException))), Times.Once());

            this.storageBroker.Verify(broker =>
                broker.InsertConsumerAccessAsync(It.IsAny<ConsumerAccess>()),
                    Times.Never);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnAddIfConsumerAccessIsInvalidAndLogItAsync(string invalidText)
        {
            // given
            string randomUserId = Guid.NewGuid().ToString();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset startDate = randomDateTimeOffset.AddSeconds(-90);
            DateTimeOffset endDate = randomDateTimeOffset.AddSeconds(0);

            var invalidConsumerAccess = new ConsumerAccess
            {
                ConsumerId = Guid.Empty,
                OrgCode = invalidText,
                CreatedBy = invalidText,
                UpdatedBy = invalidText,
            };

            securityAuditBrokerMock.Setup(service =>
                service.ApplyAddAuditValuesAsync(invalidConsumerAccess))
                    .ReturnsAsync(invalidConsumerAccess);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            var invalidConsumerAccessServiceException =
                new InvalidConsumerAccessServiceException(
                    message: "Invalid consumer access. Please correct the errors and try again.");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ConsumerAccess.Id),
                values: "Id is invalid");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ConsumerAccess.ConsumerId),
                values: "Id is invalid");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ConsumerAccess.OrgCode),
                values: "Text is invalid");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ConsumerAccess.CreatedBy),
                values: [
                    "Text is invalid",
                    $"Expected value to be '{randomUserId}' but found '{invalidConsumerAccess.CreatedBy}'."
                ]);

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ConsumerAccess.CreatedDate),
                values: [
                    "Date is invalid",
                    $"Date is not recent. Expected a value between " +
                    $"{startDate} and {endDate} but found {invalidConsumerAccess.CreatedDate}"
                ]);

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ConsumerAccess.UpdatedBy),
                values: "Text is invalid");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ConsumerAccess.UpdatedDate),
                values: "Date is invalid");

            var expectedConsumerAccessServiceValidationException =
                new ConsumerAccessServiceValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: invalidConsumerAccessServiceException);

            // when
            ValueTask<ConsumerAccess> addConsumerAccessTask =
                consumerAccessService.AddConsumerAccessAsync(invalidConsumerAccess);

            ConsumerAccessServiceValidationException actualConsumerAccessServiceValidationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceValidationException>(
                    testCode: addConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceValidationException);

            securityAuditBrokerMock.Verify(service =>
                service.ApplyAddAuditValuesAsync(invalidConsumerAccess),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceValidationException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.InsertConsumerAccessAsync(It.IsAny<ConsumerAccess>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfConsumerAccessHasInvalidLengthProperty()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            var inputCreatedByUpdatedByString = GetRandomStringWithLength(256);
            string randomUserId = Guid.NewGuid().ToString();

            ConsumerAccess invalidConsumerAccess = CreateRandomConsumerAccess(
                dateTimeOffset: randomDateTimeOffset,
                userId: randomUserId);

            invalidConsumerAccess.OrgCode = GetRandomStringWithLength(16);

            securityAuditBrokerMock.Setup(service =>
                service.ApplyAddAuditValuesAsync(invalidConsumerAccess))
                    .ReturnsAsync(invalidConsumerAccess);

            var invalidConsumerAccessServiceException = new InvalidConsumerAccessServiceException(
                message: "Invalid consumer access. Please correct the errors and try again.");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ConsumerAccess.OrgCode),
                values: $"Text exceed max length of {invalidConsumerAccess.OrgCode.Length - 1} characters");

            var expectedConsumerAccessServiceValidationException =
                new ConsumerAccessServiceValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: invalidConsumerAccessServiceException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<ConsumerAccess> addConsumerAccessTask =
                consumerAccessService.AddConsumerAccessAsync(invalidConsumerAccess);

            ConsumerAccessServiceValidationException actualConsumerAccessServiceValidationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceValidationException>(
                    testCode: addConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceValidationException);

            securityAuditBrokerMock.Verify(service =>
                service.ApplyAddAuditValuesAsync(invalidConsumerAccess),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessServiceValidationException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.InsertConsumerAccessAsync(It.IsAny<ConsumerAccess>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfAuditPropertiesIsNotTheSameAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset startDate = randomDateTimeOffset.AddSeconds(-90);
            DateTimeOffset endDate = randomDateTimeOffset.AddSeconds(0);
            string randomUserId = Guid.NewGuid().ToString();

            ConsumerAccess randomConsumerAccess = CreateRandomModifyConsumerAccess(
                dateTimeOffset: randomDateTimeOffset,
                userId: randomUserId);

            randomConsumerAccess.CreatedBy = GetRandomString();
            randomConsumerAccess.UpdatedBy = GetRandomString();
            randomConsumerAccess.CreatedDate = GetRandomDateTimeOffset();
            randomConsumerAccess.UpdatedDate = GetRandomDateTimeOffset();

            ConsumerAccess invalidConsumerAccess = randomConsumerAccess;

            securityAuditBrokerMock.Setup(service =>
                service.ApplyAddAuditValuesAsync(invalidConsumerAccess))
                    .ReturnsAsync(invalidConsumerAccess);

            var invalidConsumerAccessServiceException = new InvalidConsumerAccessServiceException(
                message: "Invalid consumer access. Please correct the errors and try again.");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ConsumerAccess.CreatedBy),
                values:
                    $"Expected value to be '{randomUserId}' " +
                    $"but found '{randomConsumerAccess.CreatedBy}'.");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ConsumerAccess.UpdatedBy),
                values: $"Text is not the same as {nameof(ConsumerAccess.CreatedBy)}");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ConsumerAccess.UpdatedDate),
                values: $"Date is not the same as {nameof(ConsumerAccess.CreatedDate)}");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ConsumerAccess.CreatedDate),
                values:
                    $"Date is not recent." +
                    $" Expected a value between {startDate} and {endDate} but found {invalidConsumerAccess.CreatedDate}");

            var expectedConsumerAccessServiceValidationException =
                new ConsumerAccessServiceValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: invalidConsumerAccessServiceException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<ConsumerAccess> addConsumerAccessTask =
                consumerAccessService.AddConsumerAccessAsync(invalidConsumerAccess);

            ConsumerAccessServiceValidationException actualConsumerAccessServiceValidationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceValidationException>(
                    testCode: addConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceValidationException.Should().BeEquivalentTo(
                expectedConsumerAccessServiceValidationException);

            securityAuditBrokerMock.Verify(service =>
                service.ApplyAddAuditValuesAsync(invalidConsumerAccess),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(
                    SameExceptionAs(expectedConsumerAccessServiceValidationException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.InsertConsumerAccessAsync(It.IsAny<ConsumerAccess>()),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(-91)]
        public async Task ShouldThrowValidationExceptionOnAddIfCreatedDateIsNotRecentAndLogItAsync(
            int invalidSeconds)
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = Guid.NewGuid().ToString();
            DateTimeOffset startDate = randomDateTimeOffset.AddSeconds(-90);
            DateTimeOffset endDate = randomDateTimeOffset.AddSeconds(0);

            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess(
                dateTimeOffset: randomDateTimeOffset,
                userId: randomUserId);

            ConsumerAccess invalidConsumerAccess = randomConsumerAccess;

            DateTimeOffset invalidDate =
                randomDateTimeOffset.AddSeconds(invalidSeconds);

            invalidConsumerAccess.CreatedDate = invalidDate;
            invalidConsumerAccess.UpdatedDate = invalidDate;

            var invalidConsumerAccessServiceException = new InvalidConsumerAccessServiceException(
                message: "Invalid consumer access. Please correct the errors and try again.");

            invalidConsumerAccessServiceException.AddData(
            key: nameof(ConsumerAccess.CreatedDate),
                values:
                    $"Date is not recent. Expected a value between " +
                    $"{startDate} and {endDate} but found {invalidDate}");

            var expectedConsumerAccessServiceValidationException =
                new ConsumerAccessServiceValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: invalidConsumerAccessServiceException);

            securityAuditBrokerMock.Setup(service =>
                service.ApplyAddAuditValuesAsync(invalidConsumerAccess))
                    .ReturnsAsync(invalidConsumerAccess);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<ConsumerAccess> addConsumerAccessTask =
                consumerAccessService.AddConsumerAccessAsync(invalidConsumerAccess);

            ConsumerAccessServiceValidationException actualConsumerAccessServiceValidationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceValidationException>(
                    testCode: addConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceValidationException.Should().BeEquivalentTo(
                expectedConsumerAccessServiceValidationException);

            securityAuditBrokerMock.Verify(service =>
                service.ApplyAddAuditValuesAsync(invalidConsumerAccess),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(
                    SameExceptionAs(expectedConsumerAccessServiceValidationException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.InsertConsumerAccessAsync(It.IsAny<ConsumerAccess>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
        }
    }
}
