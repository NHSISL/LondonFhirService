// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions;
using LondonFhirService.Core.Models.Foundations.Consumers;
using LondonFhirService.Core.Services.Foundations.ConsumerAccesses;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ConsumerAccesses
{
    public partial class ConsumerAccessesTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfConsumerAccessIsNullAndLogItAsync()
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
                service.ApplyModifyAuditAsync(nullConsumerAccess))
                    .ReturnsAsync(nullConsumerAccess);

            var expectedConsumerAccessValidationException =
                new ConsumerAccessValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: nullConsumerAccessException);

            // when
            ValueTask<ConsumerAccess> modifyConsumerAccessTask =
                consumerAccessServiceMock.Object.ModifyConsumerAccessAsync(nullConsumerAccess);

            ConsumerAccessValidationException actualConsumerAccessValidationException =
                await Assert.ThrowsAsync<ConsumerAccessValidationException>(
                    testCode: modifyConsumerAccessTask.AsTask);

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
        public async Task ShouldThrowValidationExceptionOnModifyIfConsumerAccessIsInvalidAndLogItAsync(string invalidText)
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
                service.ApplyModifyAuditAsync(invalidConsumerAccess))
                    .ReturnsAsync(invalidConsumerAccess);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            var invalidConsumerAccessException =
                new InvalidConsumerAccessException(
                    message: "Invalid consumer access. Please correct the errors and try again.");

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
                values: "Text is invalid");

            invalidConsumerAccessException.AddData(
                key: nameof(ConsumerAccess.CreatedDate),
                values: "Date is invalid");

            invalidConsumerAccessException.AddData(
                key: nameof(ConsumerAccess.UpdatedBy),
                values:
                    [
                        "Text is invalid",
                        $"Expected value to be '{randomUser.UserId}' but found '{invalidText}'."
                    ]);

            invalidConsumerAccessException.AddData(
                key: nameof(ConsumerAccess.UpdatedDate),
                values:
                    [
                        "Date is invalid",
                        "Date is the same as CreatedDate",
                        $"Date is not recent." +
                        $" Expected a value between {startDate} and {endDate} but found {invalidConsumerAccess.UpdatedDate}"
                    ]);

            var expectedConsumerAccessValidationException =
                new ConsumerAccessValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: invalidConsumerAccessException);

            // when
            ValueTask<ConsumerAccess> modifyConsumerAccessTask =
                consumerAccessServiceMock.Object.ModifyConsumerAccessAsync(invalidConsumerAccess);

            ConsumerAccessValidationException actualConsumerAccessValidationException =
                await Assert.ThrowsAsync<ConsumerAccessValidationException>(
                    testCode: modifyConsumerAccessTask.AsTask);

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

        [Theory]
        [InlineData(1)]
        [InlineData(-91)]
        public async Task ShouldThrowValidationExceptionOnModifyIfUpdatedDateIsNotRecentAndLogItAsync(
            int invalidSeconds)
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            User randomUser = CreateRandomUser();
            DateTimeOffset startDate = randomDateTimeOffset.AddSeconds(-90);
            DateTimeOffset endDate = randomDateTimeOffset.AddSeconds(0);
            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess(randomDateTimeOffset, randomUser.UserId);
            ConsumerAccess invalidConsumerAccess = randomConsumerAccess;
            invalidConsumerAccess.UpdatedDate = randomDateTimeOffset.AddSeconds(invalidSeconds);

            var invalidConsumerAccessException =
                new InvalidConsumerAccessException(
                    message: "Invalid consumer access. Please correct the errors and try again.");

            invalidConsumerAccessException.AddData(
                key: nameof(ConsumerAccess.UpdatedDate),
                values:
                [
                    $"Date is not recent." +
                    $" Expected a value between {startDate} and {endDate} but found {randomConsumerAccess.UpdatedDate}"
                ]);

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
                service.ApplyModifyAuditAsync(invalidConsumerAccess))
                    .ReturnsAsync(invalidConsumerAccess);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            // when
            ValueTask<ConsumerAccess> modifyConsumerAccessTask =
                consumerAccessServiceMock.Object.ModifyConsumerAccessAsync(invalidConsumerAccess);

            ConsumerAccessValidationException actualConsumerAccessValidationException =
                await Assert.ThrowsAsync<ConsumerAccessValidationException>(
                    testCode: modifyConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessValidationException.Should().BeEquivalentTo(expectedConsumerAccessValidationException);

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

            this.securityBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfConsumerAccessHasInvalidLengthProperty()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            User randomUser = CreateRandomUser(userId: GetRandomStringWithLengthOf(256));
            Consumer consumer = CreateRandomConsumer(dateTimeOffset: randomDateTimeOffset, userId: randomUser.UserId);
            string randomUserId = randomUser.UserId;

            ConsumerAccess invalidConsumerAccess = CreateRandomModifyConsumerAccess(
                dateTimeOffset: randomDateTimeOffset,
                userId: randomUser.UserId);

            invalidConsumerAccess.ConsumerId = consumer.Id;
            invalidConsumerAccess.OrgCode = GetRandomStringWithLength(16);
            invalidConsumerAccess.CreatedBy = randomUserId;
            invalidConsumerAccess.UpdatedBy = randomUserId;

            var consumerAccessServiceMock = new Mock<ConsumerAccessService>(
                storageBroker.Object,
                dateTimeBrokerMock.Object,
                securityBrokerMock.Object,
                loggingBrokerMock.Object)
            {
                CallBase = true
            };

            consumerAccessServiceMock.Setup(service =>
                service.ApplyModifyAuditAsync(invalidConsumerAccess))
                    .ReturnsAsync(invalidConsumerAccess);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            var invalidConsumerAccessException = new InvalidConsumerAccessException(
                message: "Invalid consumer access. Please correct the errors and try again.");

            invalidConsumerAccessException.AddData(
                key: nameof(ConsumerAccess.OrgCode),
                values: $"Text exceed max length of {invalidConsumerAccess.OrgCode.Length - 1} characters");

            invalidConsumerAccessException.AddData(
                key: nameof(ConsumerAccess.CreatedBy),
                values: $"Text exceed max length of {invalidConsumerAccess.CreatedBy.Length - 1} characters");

            invalidConsumerAccessException.AddData(
                key: nameof(ConsumerAccess.UpdatedBy),
                values: $"Text exceed max length of {invalidConsumerAccess.UpdatedBy.Length - 1} characters");

            var expectedConsumerAccessException = new
                ConsumerAccessValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: invalidConsumerAccessException);

            // when
            ValueTask<ConsumerAccess> modifyConsumerAccessTask =
                consumerAccessServiceMock.Object.ModifyConsumerAccessAsync(invalidConsumerAccess);

            ConsumerAccessValidationException actualConsumerAccessValidationException =
                await Assert.ThrowsAsync<ConsumerAccessValidationException>(
                    testCode: modifyConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessValidationException.Should().BeEquivalentTo(expectedConsumerAccessException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConsumerAccessException))),
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
        public async Task ShouldThrowValidationExceptionOnModiyIfConsumerAccessHasSameCreatedDateUpdatedDateAndLogItAsync()
        {
            // given
            DateTimeOffset randomDatTimeOffset = GetRandomDateTimeOffset();
            User randomUser = CreateRandomUser();
            string randomUserId = randomUser.UserId;
            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess(randomDatTimeOffset, randomUserId);
            var invalidConsumerAccess = randomConsumerAccess;

            var consumerAccessServiceMock = new Mock<ConsumerAccessService>(
                storageBroker.Object,
                dateTimeBrokerMock.Object,
                securityBrokerMock.Object,
                loggingBrokerMock.Object)
            {
                CallBase = true
            };

            consumerAccessServiceMock.Setup(service =>
                service.ApplyModifyAuditAsync(invalidConsumerAccess))
                    .ReturnsAsync(invalidConsumerAccess);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDatTimeOffset);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            var invalidConsumerAccessException = new InvalidConsumerAccessException(
                message: "Invalid consumer access. Please correct the errors and try again.");

            invalidConsumerAccessException.AddData(
                key: nameof(ConsumerAccess.UpdatedDate),
                values: $"Date is the same as {nameof(ConsumerAccess.CreatedDate)}");

            var expectedConsumerAccessValidationException = new ConsumerAccessValidationException(
                message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                innerException: invalidConsumerAccessException);

            // when
            ValueTask<ConsumerAccess> modifyConsumerAccessTask =
                consumerAccessServiceMock.Object.ModifyConsumerAccessAsync(invalidConsumerAccess);

            ConsumerAccessValidationException actualConsumerAccessVaildationException =
                await Assert.ThrowsAsync<ConsumerAccessValidationException>(
                    testCode: modifyConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessVaildationException.Should().BeEquivalentTo(expectedConsumerAccessValidationException);

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

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfStorageConsumerAccessDoesNotExistAndLogItAsync()
        {
            // given
            int randomNegativeNumber = GetRandomNegativeNumber();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            User randomUser = CreateRandomUser();
            string randomUserId = randomUser.UserId;
            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess(randomDateTimeOffset, randomUserId);
            ConsumerAccess nonExistingConsumerAccess = randomConsumerAccess;
            nonExistingConsumerAccess.CreatedDate = randomDateTimeOffset.AddMinutes(randomNegativeNumber);
            ConsumerAccess nullConsumerAccess = null;

            var consumerAccessServiceMock = new Mock<ConsumerAccessService>(
                storageBroker.Object,
                dateTimeBrokerMock.Object,
                securityBrokerMock.Object,
                loggingBrokerMock.Object)
            {
                CallBase = true
            };

            consumerAccessServiceMock.Setup(service =>
                service.ApplyModifyAuditAsync(nonExistingConsumerAccess))
                    .ReturnsAsync(nonExistingConsumerAccess);

            var notFoundConsumerAccessException = new NotFoundConsumerAccessException(
                message: $"Consumer access not found with Id: {nonExistingConsumerAccess.Id}");

            var expectedConsumerAccessValidationException = new ConsumerAccessValidationException(
                message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                innerException: notFoundConsumerAccessException);

            this.storageBroker.Setup(broker =>
                broker.SelectConsumerAccessByIdAsync(nonExistingConsumerAccess.Id))
                    .ReturnsAsync(nullConsumerAccess);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            // when
            ValueTask<ConsumerAccess> modifyConsumerAccessTask =
                consumerAccessServiceMock.Object.ModifyConsumerAccessAsync(nonExistingConsumerAccess);

            ConsumerAccessValidationException actualConsumerAccessVaildationException =
                await Assert.ThrowsAsync<ConsumerAccessValidationException>(
                    testCode: modifyConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessVaildationException.Should().BeEquivalentTo(expectedConsumerAccessValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.storageBroker.Verify(broker =>
                broker.SelectConsumerAccessByIdAsync(nonExistingConsumerAccess.Id),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(
                   SameExceptionAs(expectedConsumerAccessValidationException))),
                       Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task
            ShouldThrowValidationExceptionIfStorageConsumerAccessCreatedDateIsNotSameAsConsumerAccessCreatedDateAndLogItAsync()
        {
            // given
            int randomNegativeNumber = GetRandomNegativeNumber();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            User randomUser = CreateRandomUser();

            ConsumerAccess randomConsumerAccess =
                CreateRandomModifyConsumerAccess(randomDateTimeOffset, randomUser.UserId.ToString());

            ConsumerAccess invalidConsumerAccess = randomConsumerAccess;
            ConsumerAccess storageConsumerAccess = invalidConsumerAccess.DeepClone();
            storageConsumerAccess.CreatedDate = randomDateTimeOffset.AddMinutes(randomNegativeNumber);
            storageConsumerAccess.UpdatedDate = randomDateTimeOffset.AddMinutes(randomNegativeNumber);

            var consumerAccessServiceMock = new Mock<ConsumerAccessService>(
                storageBroker.Object,
                dateTimeBrokerMock.Object,
                securityBrokerMock.Object,
                loggingBrokerMock.Object)
            {
                CallBase = true
            };

            consumerAccessServiceMock.Setup(service =>
                service.ApplyModifyAuditAsync(invalidConsumerAccess))
                    .ReturnsAsync(invalidConsumerAccess);

            var invalidConsumerAccessException = new InvalidConsumerAccessException(
                message: "Invalid consumer access. Please correct the errors and try again.");

            invalidConsumerAccessException.AddData(
                key: nameof(ConsumerAccess.CreatedDate),
                values: $"Date is not the same as {nameof(ConsumerAccess.CreatedDate)}");

            var expectedConsumerAccessValidationException = new ConsumerAccessValidationException(
                message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                innerException: invalidConsumerAccessException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            this.storageBroker.Setup(broker =>
                broker.SelectConsumerAccessByIdAsync(invalidConsumerAccess.Id))
                    .ReturnsAsync(storageConsumerAccess);

            // when
            ValueTask<ConsumerAccess> modifyConsumerAccessTask =
                consumerAccessServiceMock.Object.ModifyConsumerAccessAsync(invalidConsumerAccess);

            ConsumerAccessValidationException actualConsumerAccessValidationException =
                await Assert.ThrowsAsync<ConsumerAccessValidationException>(
                    testCode: modifyConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessValidationException.Should().BeEquivalentTo(expectedConsumerAccessValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.storageBroker.Verify(broker =>
                broker.SelectConsumerAccessByIdAsync(invalidConsumerAccess.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedConsumerAccessValidationException))),
                    Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfStorageUpdatedDateSameAsUpdatedDateAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            User randomUser = CreateRandomUser();
            string randomUserId = randomUser.UserId;
            ConsumerAccess randomConsumerAccess = CreateRandomModifyConsumerAccess(randomDateTimeOffset, randomUserId);
            ConsumerAccess invalidConsumerAccess = randomConsumerAccess;
            ConsumerAccess storageConsumerAccess = invalidConsumerAccess.DeepClone();
            invalidConsumerAccess.CreatedDate = storageConsumerAccess.CreatedDate.AddMinutes(-GetRandomNumber());
            invalidConsumerAccess.UpdatedDate = storageConsumerAccess.UpdatedDate;

            var consumerAccessServiceMock = new Mock<ConsumerAccessService>(
                storageBroker.Object,
                dateTimeBrokerMock.Object,
                securityBrokerMock.Object,
                loggingBrokerMock.Object)
            {
                CallBase = true
            };

            consumerAccessServiceMock.Setup(service =>
                service.ApplyModifyAuditAsync(invalidConsumerAccess))
                    .ReturnsAsync(invalidConsumerAccess);

            var invalidConsumerAccessValidationException = new InvalidConsumerAccessException(
                message: "Invalid consumer access. Please correct the errors and try again.");

            invalidConsumerAccessValidationException.AddData(
                key: nameof(ConsumerAccess.CreatedDate),
                values: $"Date is not the same as {nameof(ConsumerAccess.CreatedDate)}");

            invalidConsumerAccessValidationException.AddData(
                key: nameof(ConsumerAccess.UpdatedDate),
                values: $"Date is the same as {nameof(ConsumerAccess.UpdatedDate)}");

            var expectedConsumerAccessValidationException = new ConsumerAccessValidationException(
                message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                innerException: invalidConsumerAccessValidationException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            this.storageBroker.Setup(broker =>
                broker.SelectConsumerAccessByIdAsync(invalidConsumerAccess.Id))
                    .ReturnsAsync(storageConsumerAccess);

            // when
            ValueTask<ConsumerAccess> modifyConsumerAccessTask =
                consumerAccessServiceMock.Object.ModifyConsumerAccessAsync(invalidConsumerAccess);

            ConsumerAccessValidationException actualConsumerAccessValidationException =
                await Assert.ThrowsAsync<ConsumerAccessValidationException>(
                    testCode: modifyConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessValidationException.Should().BeEquivalentTo(expectedConsumerAccessValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.storageBroker.Verify(broker =>
                broker.SelectConsumerAccessByIdAsync(invalidConsumerAccess.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedConsumerAccessValidationException))),
                    Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }
    }
}
