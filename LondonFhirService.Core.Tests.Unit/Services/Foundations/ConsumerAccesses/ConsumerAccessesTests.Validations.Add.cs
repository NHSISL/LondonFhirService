// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions;
using LondonFhirService.Core.Services.Foundations.ConsumerAccesses;
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
            var nullConsumerAccessException = new NullConsumerAccessException(message: "User access is null.");

            var consumerAccessServiceMock = new Mock<ConsumerAccessService>(
                storageBroker.Object,
                dateTimeBrokerMock.Object,
                securityBrokerMock.Object,
                loggingBrokerMock.Object)
            {
                CallBase = true
            };

            consumerAccessServiceMock.Setup(service =>
                service.ApplyAddAuditAsync(nullConsumerAccess))
                    .ReturnsAsync(nullConsumerAccess);

            var expectedConsumerAccessValidationException =
                new ConsumerAccessValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: nullConsumerAccessException);

            // when
            ValueTask<ConsumerAccess> addConsumerAccessTask = consumerAccessServiceMock.Object.AddConsumerAccessAsync(nullConsumerAccess);

            ConsumerAccessValidationException actualConsumerAccessValidationException =
                await Assert.ThrowsAsync<ConsumerAccessValidationException>(
                    testCode: addConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessValidationException.Should().BeEquivalentTo(expectedConsumerAccessValidationException);
            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedConsumerAccessValidationException))), Times.Once());

            this.storageBroker.Verify(broker =>
                broker.InsertConsumerAccessAsync(It.IsAny<ConsumerAccess>()),
                    Times.Never);

            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnAddIfConsumerAccessIsInvalidAndLogItAsync(string invalidText)
        {
            // given
            User randomUser = CreateRandomUser();
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

            var consumerAccessServiceMock = new Mock<ConsumerAccessService>(
                storageBroker.Object,
                dateTimeBrokerMock.Object,
                securityBrokerMock.Object,
                loggingBrokerMock.Object)
            {
                CallBase = true
            };

            consumerAccessServiceMock.Setup(service =>
                service.ApplyAddAuditAsync(invalidConsumerAccess))
                    .ReturnsAsync(invalidConsumerAccess);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            var invalidConsumerAccessException =
                new InvalidConsumerAccessException(
                    message: "Invalid user access. Please correct the errors and try again.");

            invalidConsumerAccessException.AddData(
                key: nameof(ConsumerAccess.Id),
                values: "Id is invalid");

            invalidConsumerAccessException.AddData(
                key: nameof(ConsumerAccess.ConsumerId),
                values: "Id is invalid");

            invalidConsumerAccessException.AddData(
                key: nameof(ConsumerAccess.OrgCode),
                values: "Text is invalid");

            invalidConsumerAccessException.AddData(
                key: nameof(ConsumerAccess.CreatedBy),
                values: [
                    "Text is invalid",
                    $"Expected value to be '{randomUser.UserId}' but found '{invalidConsumerAccess.CreatedBy}'."
                ]);

            invalidConsumerAccessException.AddData(
                key: nameof(ConsumerAccess.CreatedDate),
                values: [
                    "Date is invalid",
                    $"Date is not recent. Expected a value between " +
                    $"{startDate} and {endDate} but found {invalidConsumerAccess.CreatedDate}"
                ]);

            invalidConsumerAccessException.AddData(
                key: nameof(ConsumerAccess.UpdatedBy),
                values: "Text is invalid");

            invalidConsumerAccessException.AddData(
                key: nameof(ConsumerAccess.UpdatedDate),
                values: "Date is invalid");

            var expectedConsumerAccessValidationException =
                new ConsumerAccessValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: invalidConsumerAccessException);

            // when
            ValueTask<ConsumerAccess> addConsumerAccessTask =
                consumerAccessServiceMock.Object.AddConsumerAccessAsync(invalidConsumerAccess);

            ConsumerAccessValidationException actualConsumerAccessValidationException =
                await Assert.ThrowsAsync<ConsumerAccessValidationException>(
                    testCode: addConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessValidationException.Should()
                .BeEquivalentTo(expectedConsumerAccessValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessValidationException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.InsertConsumerAccessAsync(It.IsAny<ConsumerAccess>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfConsumerAccessHasInvalidLengthProperty()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            var inputCreatedByUpdatedByString = GetRandomStringWithLength(256);
            User randomUser = CreateRandomUser();

            ConsumerAccess invalidConsumerAccess = CreateRandomConsumerAccess(
                dateTimeOffset: randomDateTimeOffset,
                userId: randomUser.UserId);

            invalidConsumerAccess.OrgCode = GetRandomStringWithLength(16);

            var consumerAccessServiceMock = new Mock<ConsumerAccessService>(
                storageBroker.Object,
                dateTimeBrokerMock.Object,
                securityBrokerMock.Object,
                loggingBrokerMock.Object)
            {
                CallBase = true
            };

            consumerAccessServiceMock.Setup(service =>
                service.ApplyAddAuditAsync(invalidConsumerAccess))
                    .ReturnsAsync(invalidConsumerAccess);

            var invalidConsumerAccessException = new InvalidConsumerAccessException(
                message: "Invalid user access. Please correct the errors and try again.");

            invalidConsumerAccessException.AddData(
                key: nameof(ConsumerAccess.OrgCode),
                values: $"Text exceed max length of {invalidConsumerAccess.OrgCode.Length - 1} characters");

            var expectedConsumerAccessValidationException =
                new ConsumerAccessValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: invalidConsumerAccessException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            // when
            ValueTask<ConsumerAccess> addConsumerAccessTask =
                consumerAccessServiceMock.Object.AddConsumerAccessAsync(invalidConsumerAccess);

            ConsumerAccessValidationException actualConsumerAccessValidationException =
                await Assert.ThrowsAsync<ConsumerAccessValidationException>(
                    testCode: addConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessValidationException.Should()
                .BeEquivalentTo(expectedConsumerAccessValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessValidationException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.InsertConsumerAccessAsync(It.IsAny<ConsumerAccess>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfAuditPropertiesIsNotTheSameAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset startDate = randomDateTimeOffset.AddSeconds(-90);
            DateTimeOffset endDate = randomDateTimeOffset.AddSeconds(0);
            User randomUser = CreateRandomUser();

            ConsumerAccess randomConsumerAccess = CreateRandomModifyConsumerAccess(
                dateTimeOffset: randomDateTimeOffset,
                userId: randomUser.UserId);

            randomConsumerAccess.CreatedBy = GetRandomString();
            randomConsumerAccess.UpdatedBy = GetRandomString();
            randomConsumerAccess.CreatedDate = GetRandomDateTimeOffset();
            randomConsumerAccess.UpdatedDate = GetRandomDateTimeOffset();

            ConsumerAccess invalidConsumerAccess = randomConsumerAccess;

            var consumerAccessServiceMock = new Mock<ConsumerAccessService>(
                storageBroker.Object,
                dateTimeBrokerMock.Object,
                securityBrokerMock.Object,
                loggingBrokerMock.Object)
            {
                CallBase = true
            };

            consumerAccessServiceMock.Setup(service =>
                service.ApplyAddAuditAsync(invalidConsumerAccess))
                    .ReturnsAsync(invalidConsumerAccess);

            var invalidConsumerAccessException = new InvalidConsumerAccessException(
                message: "Invalid user access. Please correct the errors and try again.");

            invalidConsumerAccessException.AddData(
                key: nameof(ConsumerAccess.CreatedBy),
                values:
                    $"Expected value to be '{randomUser.UserId}' " +
                    $"but found '{randomConsumerAccess.CreatedBy}'.");

            invalidConsumerAccessException.AddData(
                key: nameof(ConsumerAccess.UpdatedBy),
                values: $"Text is not the same as {nameof(ConsumerAccess.CreatedBy)}");

            invalidConsumerAccessException.AddData(
                key: nameof(ConsumerAccess.UpdatedDate),
                values: $"Date is not the same as {nameof(ConsumerAccess.CreatedDate)}");

            invalidConsumerAccessException.AddData(
                key: nameof(ConsumerAccess.CreatedDate),
                values:
                    $"Date is not recent." +
                    $" Expected a value between {startDate} and {endDate} but found {invalidConsumerAccess.CreatedDate}");

            var expectedConsumerAccessValidationException =
                new ConsumerAccessValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: invalidConsumerAccessException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            // when
            ValueTask<ConsumerAccess> addConsumerAccessTask =
                consumerAccessServiceMock.Object.AddConsumerAccessAsync(invalidConsumerAccess);

            ConsumerAccessValidationException actualConsumerAccessValidationException =
                await Assert.ThrowsAsync<ConsumerAccessValidationException>(
                    testCode: addConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessValidationException.Should().BeEquivalentTo(
                expectedConsumerAccessValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(
                    SameExceptionAs(expectedConsumerAccessValidationException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.InsertConsumerAccessAsync(It.IsAny<ConsumerAccess>()),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(-91)]
        public async Task ShouldThrowValidationExceptionOnAddIfCreatedDateIsNotRecentAndLogItAsync(
            int invalidSeconds)
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            User randomUser = CreateRandomUser();
            DateTimeOffset startDate = randomDateTimeOffset.AddSeconds(-90);
            DateTimeOffset endDate = randomDateTimeOffset.AddSeconds(0);

            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess(
                dateTimeOffset: randomDateTimeOffset,
                userId: randomUser.UserId);

            ConsumerAccess invalidConsumerAccess = randomConsumerAccess;

            DateTimeOffset invalidDate =
                randomDateTimeOffset.AddSeconds(invalidSeconds);

            invalidConsumerAccess.CreatedDate = invalidDate;
            invalidConsumerAccess.UpdatedDate = invalidDate;

            var invalidConsumerAccessException = new InvalidConsumerAccessException(
                message: "Invalid user access. Please correct the errors and try again.");

            invalidConsumerAccessException.AddData(
            key: nameof(ConsumerAccess.CreatedDate),
                values:
                    $"Date is not recent. Expected a value between " +
                    $"{startDate} and {endDate} but found {invalidDate}");

            var expectedConsumerAccessValidationException =
                new ConsumerAccessValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: invalidConsumerAccessException);

            var consumerAccessServiceMock = new Mock<ConsumerAccessService>(
                storageBroker.Object,
                dateTimeBrokerMock.Object,
                securityBrokerMock.Object,
                loggingBrokerMock.Object)
            {
                CallBase = true
            };

            consumerAccessServiceMock.Setup(service =>
                service.ApplyAddAuditAsync(invalidConsumerAccess))
                    .ReturnsAsync(invalidConsumerAccess);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            // when
            ValueTask<ConsumerAccess> addConsumerAccessTask =
                consumerAccessServiceMock.Object.AddConsumerAccessAsync(invalidConsumerAccess);

            ConsumerAccessValidationException actualConsumerAccessValidationException =
                await Assert.ThrowsAsync<ConsumerAccessValidationException>(
                    testCode: addConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessValidationException.Should().BeEquivalentTo(
                expectedConsumerAccessValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(
                    SameExceptionAs(expectedConsumerAccessValidationException))),
                        Times.Once);

            this.storageBroker.Verify(broker =>
                broker.InsertConsumerAccessAsync(It.IsAny<ConsumerAccess>()),
                    Times.Never);

            this.securityBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
        }
    }
}
