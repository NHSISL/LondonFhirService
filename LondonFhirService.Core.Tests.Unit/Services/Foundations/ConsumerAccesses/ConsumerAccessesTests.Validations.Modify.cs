// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions;
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
            var nullConsumerAccessServiceException = new NullConsumerAccessServiceException(message: "Consumer access is null.");

            securityAuditBrokerMock.Setup(service =>
                service.ApplyModifyAuditValuesAsync(nullConsumerAccess))
                    .ReturnsAsync(nullConsumerAccess);

            var expectedConsumerAccessServiceValidationException =
                new ConsumerAccessServiceValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: nullConsumerAccessServiceException);

            // when
            ValueTask<ConsumerAccess> modifyConsumerAccessTask =
                consumerAccessService.ModifyConsumerAccessAsync(nullConsumerAccess);

            ConsumerAccessServiceValidationException actualConsumerAccessServiceValidationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceValidationException>(
                    testCode: modifyConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceValidationException.Should().BeEquivalentTo(expectedConsumerAccessServiceValidationException);

            securityAuditBrokerMock.Verify(service =>
                service.ApplyModifyAuditValuesAsync(nullConsumerAccess),
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
        public async Task ShouldThrowValidationExceptionOnModifyIfConsumerAccessIsInvalidAndLogItAsync(
            string invalidText)
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
                service.ApplyModifyAuditValuesAsync(invalidConsumerAccess))
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
                values: "Text is invalid");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ConsumerAccess.CreatedDate),
                values: "Date is invalid");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ConsumerAccess.UpdatedBy),
                values:
                    [
                        "Text is invalid",
                        $"Expected value to be '{randomUserId}' but found '{invalidText}'."
                    ]);

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ConsumerAccess.UpdatedDate),
                values:
                    [
                        "Date is invalid",
                        "Date is the same as CreatedDate",
                        $"Date is not recent." +
                        $" Expected a value between {startDate} and {endDate} " +
                        $"but found {invalidConsumerAccess.UpdatedDate}"
                    ]);

            var expectedConsumerAccessServiceValidationException =
                new ConsumerAccessServiceValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: invalidConsumerAccessServiceException);

            // when
            ValueTask<ConsumerAccess> modifyConsumerAccessTask =
                consumerAccessService.ModifyConsumerAccessAsync(invalidConsumerAccess);

            ConsumerAccessServiceValidationException actualConsumerAccessServiceValidationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceValidationException>(
                    testCode: modifyConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceValidationException.Should()
                .BeEquivalentTo(expectedConsumerAccessServiceValidationException);

            securityAuditBrokerMock.Verify(service =>
                service.ApplyModifyAuditValuesAsync(invalidConsumerAccess),
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

        [Theory]
        [InlineData(1)]
        [InlineData(-91)]
        public async Task ShouldThrowValidationExceptionOnModifyIfUpdatedDateIsNotRecentAndLogItAsync(
            int invalidSeconds)
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = Guid.NewGuid().ToString();
            DateTimeOffset startDate = randomDateTimeOffset.AddSeconds(-90);
            DateTimeOffset endDate = randomDateTimeOffset.AddSeconds(0);
            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess(randomDateTimeOffset, randomUserId);
            ConsumerAccess invalidConsumerAccess = randomConsumerAccess;
            invalidConsumerAccess.UpdatedDate = randomDateTimeOffset.AddSeconds(invalidSeconds);

            var invalidConsumerAccessServiceException =
                new InvalidConsumerAccessServiceException(
                    message: "Invalid consumer access. Please correct the errors and try again.");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ConsumerAccess.UpdatedDate),
                values:
                [
                    $"Date is not recent." +
                    $" Expected a value between {startDate} and {endDate} but found {randomConsumerAccess.UpdatedDate}"
                ]);

            var expectedConsumerAccessServiceValidationException =
                new ConsumerAccessServiceValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: invalidConsumerAccessServiceException);

            securityAuditBrokerMock.Setup(service =>
                service.ApplyModifyAuditValuesAsync(invalidConsumerAccess))
                    .ReturnsAsync(invalidConsumerAccess);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<ConsumerAccess> modifyConsumerAccessTask =
                consumerAccessService.ModifyConsumerAccessAsync(invalidConsumerAccess);

            ConsumerAccessServiceValidationException actualConsumerAccessServiceValidationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceValidationException>(
                    testCode: modifyConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceValidationException.Should().BeEquivalentTo(expectedConsumerAccessServiceValidationException);

            securityAuditBrokerMock.Verify(service =>
                service.ApplyModifyAuditValuesAsync(invalidConsumerAccess),
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

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfConsumerAccessHasInvalidLengthProperty()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomStringWithLengthOf(256);

            ConsumerAccess invalidConsumerAccess = CreateRandomModifyConsumerAccess(
                dateTimeOffset: randomDateTimeOffset,
                userId: randomUserId);

            invalidConsumerAccess.OrgCode = GetRandomStringWithLength(16);
            invalidConsumerAccess.CreatedBy = randomUserId;
            invalidConsumerAccess.UpdatedBy = randomUserId;

            securityAuditBrokerMock.Setup(service =>
                service.ApplyModifyAuditValuesAsync(invalidConsumerAccess))
                    .ReturnsAsync(invalidConsumerAccess);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            var invalidConsumerAccessServiceException = new InvalidConsumerAccessServiceException(
                message: "Invalid consumer access. Please correct the errors and try again.");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ConsumerAccess.OrgCode),
                values: $"Text exceed max length of {invalidConsumerAccess.OrgCode.Length - 1} characters");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ConsumerAccess.CreatedBy),
                values: $"Text exceed max length of {invalidConsumerAccess.CreatedBy.Length - 1} characters");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ConsumerAccess.UpdatedBy),
                values: $"Text exceed max length of {invalidConsumerAccess.UpdatedBy.Length - 1} characters");

            var expectedConsumerAccessException = new
                ConsumerAccessServiceValidationException(
                    message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                    innerException: invalidConsumerAccessServiceException);

            // when
            ValueTask<ConsumerAccess> modifyConsumerAccessTask =
                consumerAccessService.ModifyConsumerAccessAsync(invalidConsumerAccess);

            ConsumerAccessServiceValidationException actualConsumerAccessServiceValidationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceValidationException>(
                    testCode: modifyConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceValidationException.Should().BeEquivalentTo(expectedConsumerAccessException);

            securityAuditBrokerMock.Verify(service =>
                service.ApplyModifyAuditValuesAsync(invalidConsumerAccess),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
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
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModiyIfConsumerAccessHasSameCreatedDateUpdatedDateAndLogItAsync()
        {
            // given
            DateTimeOffset randomDatTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = Guid.NewGuid().ToString();
            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess(randomDatTimeOffset, randomUserId);
            var invalidConsumerAccess = randomConsumerAccess;

            securityAuditBrokerMock.Setup(service =>
                service.ApplyModifyAuditValuesAsync(invalidConsumerAccess))
                    .ReturnsAsync(invalidConsumerAccess);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDatTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            var invalidConsumerAccessServiceException = new InvalidConsumerAccessServiceException(
                message: "Invalid consumer access. Please correct the errors and try again.");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ConsumerAccess.UpdatedDate),
                values: $"Date is the same as {nameof(ConsumerAccess.CreatedDate)}");

            var expectedConsumerAccessServiceValidationException = new ConsumerAccessServiceValidationException(
                message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                innerException: invalidConsumerAccessServiceException);

            // when
            ValueTask<ConsumerAccess> modifyConsumerAccessTask =
                consumerAccessService.ModifyConsumerAccessAsync(invalidConsumerAccess);

            ConsumerAccessServiceValidationException actualConsumerAccessVaildationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceValidationException>(
                    testCode: modifyConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessVaildationException.Should().BeEquivalentTo(expectedConsumerAccessServiceValidationException);

            securityAuditBrokerMock.Verify(service =>
                service.ApplyModifyAuditValuesAsync(invalidConsumerAccess),
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

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfStorageConsumerAccessDoesNotExistAndLogItAsync()
        {
            // given
            int randomNegativeNumber = GetRandomNegativeNumber();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = Guid.NewGuid().ToString();
            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess(randomDateTimeOffset, randomUserId);
            ConsumerAccess nonExistingConsumerAccess = randomConsumerAccess;
            nonExistingConsumerAccess.CreatedDate = randomDateTimeOffset.AddMinutes(randomNegativeNumber);
            ConsumerAccess nullConsumerAccess = null;

            securityAuditBrokerMock.Setup(service =>
                service.ApplyModifyAuditValuesAsync(nonExistingConsumerAccess))
                    .ReturnsAsync(nonExistingConsumerAccess);

            var notFoundConsumerAccessException = new NotFoundConsumerAccessServiceException(
                message: $"Consumer access not found with Id: {nonExistingConsumerAccess.Id}");

            var expectedConsumerAccessServiceValidationException = new ConsumerAccessServiceValidationException(
                message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                innerException: notFoundConsumerAccessException);

            this.storageBroker.Setup(broker =>
                broker.SelectConsumerAccessByIdAsync(nonExistingConsumerAccess.Id))
                    .ReturnsAsync(nullConsumerAccess);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<ConsumerAccess> modifyConsumerAccessTask =
                consumerAccessService.ModifyConsumerAccessAsync(nonExistingConsumerAccess);

            ConsumerAccessServiceValidationException actualConsumerAccessVaildationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceValidationException>(
                    testCode: modifyConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessVaildationException.Should().BeEquivalentTo(expectedConsumerAccessServiceValidationException);

            securityAuditBrokerMock.Verify(service =>
                service.ApplyModifyAuditValuesAsync(nonExistingConsumerAccess),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

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

        [Fact]
        public async Task
            ShouldThrowValidationExceptionIfStorageConsumerAccessCreatedDateIsNotSameAsConsumerAccessCreatedDateAndLogItAsync()
        {
            // given
            int randomNegativeNumber = GetRandomNegativeNumber();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = Guid.NewGuid().ToString();

            ConsumerAccess randomConsumerAccess =
                CreateRandomModifyConsumerAccess(randomDateTimeOffset, randomUserId.ToString());

            ConsumerAccess invalidConsumerAccess = randomConsumerAccess;
            ConsumerAccess storageConsumerAccess = invalidConsumerAccess.DeepClone();
            storageConsumerAccess.CreatedDate = randomDateTimeOffset.AddMinutes(randomNegativeNumber);
            storageConsumerAccess.UpdatedDate = randomDateTimeOffset.AddMinutes(randomNegativeNumber);

            securityAuditBrokerMock.Setup(service =>
                service.ApplyModifyAuditValuesAsync(invalidConsumerAccess))
                    .ReturnsAsync(invalidConsumerAccess);

            var invalidConsumerAccessServiceException = new InvalidConsumerAccessServiceException(
                message: "Invalid consumer access. Please correct the errors and try again.");

            invalidConsumerAccessServiceException.AddData(
                key: nameof(ConsumerAccess.CreatedDate),
                values: $"Date is not the same as {nameof(ConsumerAccess.CreatedDate)}");

            var expectedConsumerAccessServiceValidationException = new ConsumerAccessServiceValidationException(
                message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                innerException: invalidConsumerAccessServiceException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.storageBroker.Setup(broker =>
                broker.SelectConsumerAccessByIdAsync(invalidConsumerAccess.Id))
                    .ReturnsAsync(storageConsumerAccess);

            // when
            ValueTask<ConsumerAccess> modifyConsumerAccessTask =
                consumerAccessService.ModifyConsumerAccessAsync(invalidConsumerAccess);

            ConsumerAccessServiceValidationException actualConsumerAccessServiceValidationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceValidationException>(
                    testCode: modifyConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceValidationException.Should().BeEquivalentTo(expectedConsumerAccessServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidConsumerAccess),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.storageBroker.Verify(broker =>
                broker.SelectConsumerAccessByIdAsync(invalidConsumerAccess.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedConsumerAccessServiceValidationException))),
                    Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfStorageUpdatedDateSameAsUpdatedDateAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = Guid.NewGuid().ToString();
            ConsumerAccess randomConsumerAccess = CreateRandomModifyConsumerAccess(randomDateTimeOffset, randomUserId);
            ConsumerAccess invalidConsumerAccess = randomConsumerAccess;
            ConsumerAccess storageConsumerAccess = invalidConsumerAccess.DeepClone();
            invalidConsumerAccess.CreatedDate = storageConsumerAccess.CreatedDate.AddMinutes(-GetRandomNumber());
            invalidConsumerAccess.UpdatedDate = storageConsumerAccess.UpdatedDate;

            securityAuditBrokerMock.Setup(service =>
                service.ApplyModifyAuditValuesAsync(invalidConsumerAccess))
                    .ReturnsAsync(invalidConsumerAccess);

            var invalidConsumerAccessServiceValidationException = new InvalidConsumerAccessServiceException(
                message: "Invalid consumer access. Please correct the errors and try again.");

            invalidConsumerAccessServiceValidationException.AddData(
                key: nameof(ConsumerAccess.CreatedDate),
                values: $"Date is not the same as {nameof(ConsumerAccess.CreatedDate)}");

            invalidConsumerAccessServiceValidationException.AddData(
                key: nameof(ConsumerAccess.UpdatedDate),
                values: $"Date is the same as {nameof(ConsumerAccess.UpdatedDate)}");

            var expectedConsumerAccessServiceValidationException = new ConsumerAccessServiceValidationException(
                message: "ConsumerAccess validation error occurred, please fix errors and try again.",
                innerException: invalidConsumerAccessServiceValidationException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.storageBroker.Setup(broker =>
                broker.SelectConsumerAccessByIdAsync(invalidConsumerAccess.Id))
                    .ReturnsAsync(storageConsumerAccess);

            // when
            ValueTask<ConsumerAccess> modifyConsumerAccessTask =
                consumerAccessService.ModifyConsumerAccessAsync(invalidConsumerAccess);

            ConsumerAccessServiceValidationException actualConsumerAccessServiceValidationException =
                await Assert.ThrowsAsync<ConsumerAccessServiceValidationException>(
                    testCode: modifyConsumerAccessTask.AsTask);

            // then
            actualConsumerAccessServiceValidationException.Should().BeEquivalentTo(expectedConsumerAccessServiceValidationException);

            securityAuditBrokerMock.Verify(service =>
                service.ApplyModifyAuditValuesAsync(invalidConsumerAccess),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.storageBroker.Verify(broker =>
                broker.SelectConsumerAccessByIdAsync(invalidConsumerAccess.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedConsumerAccessServiceValidationException))),
                    Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBroker.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }
    }
}
