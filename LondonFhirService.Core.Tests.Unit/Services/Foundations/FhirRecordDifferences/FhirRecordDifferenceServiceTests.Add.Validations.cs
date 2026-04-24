// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.FhirRecordDifferences
{
    public partial class FhirRecordDifferenceServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfFhirRecordDifferenceIsNullAndLogItAsync()
        {
            // given
            FhirRecordDifference nullFhirRecordDifference = null;

            var nullFhirRecordDifferenceException =
                new NullFhirRecordDifferenceException(message: "FhirRecordDifference is null.");

            var expectedFhirRecordDifferenceValidationException =
                new FhirRecordDifferenceValidationException(
                    message: "FhirRecordDifference validation errors occurred, please try again.",
                    innerException: nullFhirRecordDifferenceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(nullFhirRecordDifference))
                    .ReturnsAsync(nullFhirRecordDifference);

            // when
            ValueTask<FhirRecordDifference> addFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.AddFhirRecordDifferenceAsync(nullFhirRecordDifference);

            FhirRecordDifferenceValidationException actualFhirRecordDifferenceValidationException =
                await Assert.ThrowsAsync<FhirRecordDifferenceValidationException>(() =>
                    addFhirRecordDifferenceTask.AsTask());

            // then
            actualFhirRecordDifferenceValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(nullFhirRecordDifference),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceValidationException))),
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
        public async Task ShouldThrowValidationExceptionOnAddIfFhirRecordDifferenceIsInvalidAndLogItAsync(string invalidText)
        {
            // given
            string randomUserId = GetRandomString();
            User randomUser = CreateRandomUser(userId: randomUserId);

            var invalidFhirRecordDifference = new FhirRecordDifference
            {
                PrimaryId = Guid.Empty,
                SecondaryId = Guid.Empty,
                CorrelationId = invalidText,
                DiffJson = invalidText,
            };

            var invalidFhirRecordDifferenceException =
                new InvalidFhirRecordDifferenceException(
                    message: "Invalid fhirRecordDifference. Please correct the errors and try again.");

            invalidFhirRecordDifferenceException.AddData(
                key: nameof(FhirRecordDifference.Id),
                values: "Id is required");

            invalidFhirRecordDifferenceException.AddData(
                key: nameof(FhirRecordDifference.PrimaryId),
                values: "Id is required");

            invalidFhirRecordDifferenceException.AddData(
                key: nameof(FhirRecordDifference.SecondaryId),
                values: "Id is required");

            invalidFhirRecordDifferenceException.AddData(
                key: nameof(FhirRecordDifference.CorrelationId),
                values: "Text is required");

            invalidFhirRecordDifferenceException.AddData(
                key: nameof(FhirRecordDifference.DiffJson),
                values: "Text is required");

            invalidFhirRecordDifferenceException.AddData(
                key: nameof(FhirRecordDifference.CreatedDate),
                values: "Date is required");

            invalidFhirRecordDifferenceException.AddData(
                key: nameof(FhirRecordDifference.CreatedBy),
                values:
                    [
                        "Text is required",
                        $"Expected value to be '{randomUserId}' but found '{invalidFhirRecordDifference.CreatedBy}'."
                    ]);

            invalidFhirRecordDifferenceException.AddData(
                key: nameof(FhirRecordDifference.UpdatedDate),
                values: "Date is required");

            invalidFhirRecordDifferenceException.AddData(
                key: nameof(FhirRecordDifference.UpdatedBy),
                values: "Text is required");

            var expectedFhirRecordDifferenceValidationException =
                new FhirRecordDifferenceValidationException(
                    message: "FhirRecordDifference validation errors occurred, please try again.",
                    innerException: invalidFhirRecordDifferenceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(invalidFhirRecordDifference))
                    .ReturnsAsync(invalidFhirRecordDifference);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUser.UserId);

            // when
            ValueTask<FhirRecordDifference> addFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.AddFhirRecordDifferenceAsync(invalidFhirRecordDifference);

            FhirRecordDifferenceValidationException actualFhirRecordDifferenceValidationException =
                await Assert.ThrowsAsync<FhirRecordDifferenceValidationException>(() =>
                    addFhirRecordDifferenceTask.AsTask());

            // then
            actualFhirRecordDifferenceValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(invalidFhirRecordDifference),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once());

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfFhirRecordDifferenceHasInvalidLengthProperty()
        {
            // given
            string randomUserId = GetRandomStringWithLengthOf(256);
            User randomUser = CreateRandomUser(userId: randomUserId);

            var invalidFhirRecordDifference = CreateRandomFhirRecordDifference(GetRandomDateTimeOffset(), userId: randomUserId);
            invalidFhirRecordDifference.CorrelationId = GetRandomStringWithLengthOf(256);
            invalidFhirRecordDifference.CreatedBy = randomUserId;
            invalidFhirRecordDifference.UpdatedBy = randomUserId;

            var invalidFhirRecordDifferenceException =
                new InvalidFhirRecordDifferenceException(
                    message: "Invalid fhirRecordDifference. Please correct the errors and try again.");

            invalidFhirRecordDifferenceException.AddData(
                key: nameof(FhirRecordDifference.CorrelationId),
                values: $"Text exceed max length of {invalidFhirRecordDifference.CorrelationId.Length - 1} characters");

            invalidFhirRecordDifferenceException.AddData(
                key: nameof(FhirRecordDifference.CreatedBy),
                values: $"Text exceed max length of {invalidFhirRecordDifference.CreatedBy.Length - 1} characters");

            invalidFhirRecordDifferenceException.AddData(
                key: nameof(FhirRecordDifference.UpdatedBy),
                values: $"Text exceed max length of {invalidFhirRecordDifference.UpdatedBy.Length - 1} characters");

            var expectedFhirRecordDifferenceValidationException =
                new FhirRecordDifferenceValidationException(
                    message: "FhirRecordDifference validation errors occurred, please try again.",
                    innerException: invalidFhirRecordDifferenceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(invalidFhirRecordDifference))
                    .ReturnsAsync(invalidFhirRecordDifference);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUser.UserId);

            // when
            ValueTask<FhirRecordDifference> addFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.AddFhirRecordDifferenceAsync(invalidFhirRecordDifference);

            FhirRecordDifferenceValidationException actualFhirRecordDifferenceValidationException =
                await Assert.ThrowsAsync<FhirRecordDifferenceValidationException>(
                    addFhirRecordDifferenceTask.AsTask);

            // then
            actualFhirRecordDifferenceValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(invalidFhirRecordDifference),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once());

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
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
            User randomUser = CreateRandomUser(userId: randomUserId);
            FhirRecordDifference randomFhirRecordDifference = CreateRandomFhirRecordDifference(randomDateTimeOffset, userId: randomUserId);
            FhirRecordDifference invalidFhirRecordDifference = randomFhirRecordDifference;

            invalidFhirRecordDifference.UpdatedDate =
                invalidFhirRecordDifference.CreatedDate.AddDays(randomNumber);

            var invalidFhirRecordDifferenceException =
                new InvalidFhirRecordDifferenceException(
                    message: "Invalid fhirRecordDifference. Please correct the errors and try again.");

            invalidFhirRecordDifferenceException.AddData(
                key: nameof(FhirRecordDifference.UpdatedDate),
                values: $"Date is not the same as {nameof(FhirRecordDifference.CreatedDate)}");

            var expectedFhirRecordDifferenceValidationException =
                new FhirRecordDifferenceValidationException(
                    message: "FhirRecordDifference validation errors occurred, please try again.",
                    innerException: invalidFhirRecordDifferenceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(invalidFhirRecordDifference))
                    .ReturnsAsync(invalidFhirRecordDifference);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUser.UserId);

            // when
            ValueTask<FhirRecordDifference> addFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.AddFhirRecordDifferenceAsync(invalidFhirRecordDifference);

            FhirRecordDifferenceValidationException actualFhirRecordDifferenceValidationException =
                await Assert.ThrowsAsync<FhirRecordDifferenceValidationException>(() =>
                    addFhirRecordDifferenceTask.AsTask());

            // then
            actualFhirRecordDifferenceValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(invalidFhirRecordDifference),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once());

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
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
            User randomUser = CreateRandomUser(userId: randomUserId);
            FhirRecordDifference randomFhirRecordDifference = CreateRandomFhirRecordDifference(randomDateTimeOffset, userId: randomUserId);
            FhirRecordDifference invalidFhirRecordDifference = randomFhirRecordDifference.DeepClone();
            invalidFhirRecordDifference.UpdatedBy = Guid.NewGuid().ToString();

            var invalidFhirRecordDifferenceException =
                new InvalidFhirRecordDifferenceException(
                    message: "Invalid fhirRecordDifference. Please correct the errors and try again.");

            invalidFhirRecordDifferenceException.AddData(
                key: nameof(FhirRecordDifference.UpdatedBy),
                values: $"Text is not the same as {nameof(FhirRecordDifference.CreatedBy)}");

            var expectedFhirRecordDifferenceValidationException =
                new FhirRecordDifferenceValidationException(
                    message: "FhirRecordDifference validation errors occurred, please try again.",
                    innerException: invalidFhirRecordDifferenceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(invalidFhirRecordDifference))
                    .ReturnsAsync(invalidFhirRecordDifference);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUser.UserId);

            // when
            ValueTask<FhirRecordDifference> addFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.AddFhirRecordDifferenceAsync(invalidFhirRecordDifference);

            FhirRecordDifferenceValidationException actualFhirRecordDifferenceValidationException =
                await Assert.ThrowsAsync<FhirRecordDifferenceValidationException>(() =>
                    addFhirRecordDifferenceTask.AsTask());

            // then
            actualFhirRecordDifferenceValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(invalidFhirRecordDifference),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once());

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
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
            User randomUser = CreateRandomUser(userId: randomUserId);
            FhirRecordDifference randomFhirRecordDifference = CreateRandomFhirRecordDifference(invalidDateTime, userId: randomUserId);
            FhirRecordDifference invalidFhirRecordDifference = randomFhirRecordDifference;

            var invalidFhirRecordDifferenceException =
                new InvalidFhirRecordDifferenceException(
                    message: "Invalid fhirRecordDifference. Please correct the errors and try again.");

            invalidFhirRecordDifferenceException.AddData(
                key: nameof(FhirRecordDifference.CreatedDate),
                values:
                    $"Date is not recent. Expected a value between {startDate} and {endDate} but found {invalidDate}");

            var expectedFhirRecordDifferenceValidationException =
                new FhirRecordDifferenceValidationException(
                    message: "FhirRecordDifference validation errors occurred, please try again.",
                    innerException: invalidFhirRecordDifferenceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(invalidFhirRecordDifference))
                    .ReturnsAsync(invalidFhirRecordDifference);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUser.UserId);

            // when
            ValueTask<FhirRecordDifference> addFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.AddFhirRecordDifferenceAsync(invalidFhirRecordDifference);

            FhirRecordDifferenceValidationException actualFhirRecordDifferenceValidationException =
                await Assert.ThrowsAsync<FhirRecordDifferenceValidationException>(() =>
                    addFhirRecordDifferenceTask.AsTask());

            // then
            actualFhirRecordDifferenceValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(invalidFhirRecordDifference),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once());

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}