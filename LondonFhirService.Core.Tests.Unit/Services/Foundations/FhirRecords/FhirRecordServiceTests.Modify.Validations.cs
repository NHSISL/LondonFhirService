// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.FhirRecords
{
    public partial class FhirRecordServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfFhirRecordIsNullAndLogItAsync()
        {
            // given
            FhirRecord nullFhirRecord = null;
            var nullFhirRecordException = new NullFhirRecordException(message: "FhirRecord is null.");

            var expectedFhirRecordValidationException =
                new FhirRecordValidationException(
                    message: "FhirRecord validation errors occurred, please try again.",
                    innerException: nullFhirRecordException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(nullFhirRecord))
                    .ReturnsAsync(nullFhirRecord);

            // when
            ValueTask<FhirRecord> modifyFhirRecordTask =
                this.fhirRecordService.ModifyFhirRecordAsync(nullFhirRecord);

            FhirRecordValidationException actualFhirRecordValidationException =
                await Assert.ThrowsAsync<FhirRecordValidationException>(
                    modifyFhirRecordTask.AsTask);

            // then
            actualFhirRecordValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(nullFhirRecord),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordValidationException))),
                        Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateFhirRecordAsync(It.IsAny<FhirRecord>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnModifyIfFhirRecordIsInvalidAndLogItAsync(string invalidText)
        {
            // given 
            string randomUserId = GetRandomString();
            User randomUser = CreateRandomUser(userId: randomUserId);

            var invalidFhirRecord = new FhirRecord
            {
                CorrelationId = invalidText,
                JsonPayload = invalidText,
                SourceName = invalidText
            };

            var invalidFhirRecordException =
                new InvalidFhirRecordException(
                    message: "Invalid fhirRecord. Please correct the errors and try again.");

            invalidFhirRecordException.AddData(
                key: nameof(FhirRecord.Id),
                values: "Id is required");
            invalidFhirRecordException.AddData(
                key: nameof(FhirRecord.CorrelationId),
                values: "Text is required");

            invalidFhirRecordException.AddData(
                key: nameof(FhirRecord.JsonPayload),
                values: "Text is required");

            invalidFhirRecordException.AddData(
                key: nameof(FhirRecord.SourceName),
                values: "Text is required");

            invalidFhirRecordException.AddData(
                key: nameof(FhirRecord.CreatedDate),
                values: "Date is required");

            invalidFhirRecordException.AddData(
                key: nameof(FhirRecord.CreatedBy),
                values: "Text is required");

            invalidFhirRecordException.AddData(
                key: nameof(FhirRecord.UpdatedDate),
                values:
                new[] {
                    "Date is required",
                    $"Date is the same as {nameof(FhirRecord.CreatedDate)}"
                });

            invalidFhirRecordException.AddData(
                key: nameof(FhirRecord.UpdatedBy),
                values:
                    [
                        "Text is required",
                        $"Expected value to be '{randomUserId}' but found '{invalidFhirRecord.UpdatedBy}'."
                    ]);

            var expectedFhirRecordValidationException =
                new FhirRecordValidationException(
                    message: "FhirRecord validation errors occurred, please try again.",
                    innerException: invalidFhirRecordException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidFhirRecord))
                    .ReturnsAsync(invalidFhirRecord);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUser.UserId);

            // when
            ValueTask<FhirRecord> modifyFhirRecordTask =
                this.fhirRecordService.ModifyFhirRecordAsync(invalidFhirRecord);

            FhirRecordValidationException actualFhirRecordValidationException =
                await Assert.ThrowsAsync<FhirRecordValidationException>(
                    modifyFhirRecordTask.AsTask);

            //then
            actualFhirRecordValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidFhirRecord),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordValidationException))),
                        Times.Once());

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateFhirRecordAsync(It.IsAny<FhirRecord>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfFhirRecordHasInvalidLengthProperty()
        {
            // given
            string randomUserId = GetRandomStringWithLengthOf(256);
            User randomUser = CreateRandomUser(userId: randomUserId);

            var invalidFhirRecord = CreateRandomModifyFhirRecord(GetRandomDateTimeOffset(), userId: randomUserId);
            invalidFhirRecord.CorrelationId = GetRandomStringWithLengthOf(256);
            invalidFhirRecord.SourceName = GetRandomStringWithLengthOf(451);
            invalidFhirRecord.CreatedBy = randomUserId;
            invalidFhirRecord.UpdatedBy = randomUserId;

            var invalidFhirRecordException =
                new InvalidFhirRecordException(
                    message: "Invalid fhirRecord. Please correct the errors and try again.");

            invalidFhirRecordException.AddData(
                key: nameof(FhirRecord.CorrelationId),
                values: $"Text exceed max length of {invalidFhirRecord.CorrelationId.Length - 1} characters");

            invalidFhirRecordException.AddData(
                key: nameof(FhirRecord.SourceName),
                values: $"Text exceed max length of {invalidFhirRecord.SourceName.Length - 1} characters");

            invalidFhirRecordException.AddData(
                key: nameof(FhirRecord.CreatedBy),
                values: $"Text exceed max length of {invalidFhirRecord.CreatedBy.Length - 1} characters");

            invalidFhirRecordException.AddData(
                key: nameof(FhirRecord.UpdatedBy),
                values: $"Text exceed max length of {invalidFhirRecord.UpdatedBy.Length - 1} characters");

            var expectedFhirRecordValidationException =
                new FhirRecordValidationException(
                    message: "FhirRecord validation errors occurred, please try again.",
                    innerException: invalidFhirRecordException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidFhirRecord))
                    .ReturnsAsync(invalidFhirRecord);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUser.UserId);

            // when
            ValueTask<FhirRecord> modifyFhirRecordTask =
                this.fhirRecordService.ModifyFhirRecordAsync(invalidFhirRecord);

            FhirRecordValidationException actualFhirRecordValidationException =
                await Assert.ThrowsAsync<FhirRecordValidationException>(
                    modifyFhirRecordTask.AsTask);

            // then
            actualFhirRecordValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidFhirRecord),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once());

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateFhirRecordAsync(It.IsAny<FhirRecord>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
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
            User randomUser = CreateRandomUser(userId: randomUserId);

            FhirRecord randomFhirRecord =
                CreateRandomFhirRecord(dateTimeOffset: randomDateTimeOffset, userId: randomUserId);

            FhirRecord invalidFhirRecord = randomFhirRecord;

            var invalidFhirRecordException =
                new InvalidFhirRecordException(
                    message: "Invalid fhirRecord. Please correct the errors and try again.");

            invalidFhirRecordException.AddData(
                key: nameof(FhirRecord.UpdatedDate),
                values: $"Date is the same as {nameof(FhirRecord.CreatedDate)}");

            var expectedFhirRecordValidationException =
                new FhirRecordValidationException(
                    message: "FhirRecord validation errors occurred, please try again.",
                    innerException: invalidFhirRecordException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidFhirRecord))
                    .ReturnsAsync(invalidFhirRecord);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUser.UserId);

            // when
            ValueTask<FhirRecord> modifyFhirRecordTask =
                this.fhirRecordService.ModifyFhirRecordAsync(invalidFhirRecord);

            FhirRecordValidationException actualFhirRecordValidationException =
                await Assert.ThrowsAsync<FhirRecordValidationException>(
                    modifyFhirRecordTask.AsTask);

            // then
            actualFhirRecordValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidFhirRecord),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordByIdAsync(invalidFhirRecord.Id),
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
            User randomUser = CreateRandomUser(userId: randomUserId);

            FhirRecord randomFhirRecord =
                CreateRandomFhirRecord(dateTimeOffset: randomDateTimeOffset, userId: randomUserId);

            randomFhirRecord.UpdatedDate = randomDateTimeOffset.AddMinutes(minutes);

            var invalidFhirRecordException =
                new InvalidFhirRecordException(
                    message: "Invalid fhirRecord. Please correct the errors and try again.");

            invalidFhirRecordException.AddData(
                key: nameof(FhirRecord.UpdatedDate),
                values:
                    $"Date is not recent. Expected a value between {startDate} and {endDate} but found {invalidDate}");

            var expectedFhirRecordValidationException =
                new FhirRecordValidationException(
                    message: "FhirRecord validation errors occurred, please try again.",
                    innerException: invalidFhirRecordException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(randomFhirRecord))
                    .ReturnsAsync(randomFhirRecord);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUser.UserId);

            // when
            ValueTask<FhirRecord> modifyFhirRecordTask =
                this.fhirRecordService.ModifyFhirRecordAsync(randomFhirRecord);

            FhirRecordValidationException actualFhirRecordValidationException =
                await Assert.ThrowsAsync<FhirRecordValidationException>(
                    modifyFhirRecordTask.AsTask);

            // then
            actualFhirRecordValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(randomFhirRecord),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordByIdAsync(It.IsAny<Guid>()),
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
        public async Task ShouldThrowValidationExceptionOnModifyIfFhirRecordDoesNotExistAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();
            User randomUser = CreateRandomUser(userId: randomUserId);

            FhirRecord randomFhirRecord = CreateRandomModifyFhirRecord(
                dateTimeOffset: randomDateTimeOffset, userId: randomUserId);

            FhirRecord nonExistFhirRecord = randomFhirRecord;
            FhirRecord nullFhirRecord = null;

            var notFoundFhirRecordException = new NotFoundFhirRecordException(
                message: $"Couldn't find fhirRecord with fhirRecordId: {nonExistFhirRecord.Id}.");

            var expectedFhirRecordValidationException =
                new FhirRecordValidationException(
                    message: "FhirRecord validation errors occurred, please try again.",
                    innerException: notFoundFhirRecordException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(nonExistFhirRecord))
                    .ReturnsAsync(nonExistFhirRecord);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordByIdAsync(nonExistFhirRecord.Id))
                    .ReturnsAsync(nullFhirRecord);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUser.UserId);

            // when 
            ValueTask<FhirRecord> modifyFhirRecordTask =
                this.fhirRecordService.ModifyFhirRecordAsync(nonExistFhirRecord);

            FhirRecordValidationException actualFhirRecordValidationException =
                await Assert.ThrowsAsync<FhirRecordValidationException>(
                    modifyFhirRecordTask.AsTask);

            // then
            actualFhirRecordValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(nonExistFhirRecord),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordByIdAsync(nonExistFhirRecord.Id),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordValidationException))),
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
            User randomUser = CreateRandomUser(userId: randomUserId);

            FhirRecord randomFhirRecord = CreateRandomModifyFhirRecord(
                dateTimeOffset: randomDateTimeOffset, userId: randomUserId);

            FhirRecord invalidFhirRecord = randomFhirRecord.DeepClone();
            FhirRecord storageFhirRecord = invalidFhirRecord.DeepClone();
            storageFhirRecord.CreatedDate = storageFhirRecord.CreatedDate.AddMinutes(randomMinutes);
            storageFhirRecord.UpdatedDate = storageFhirRecord.UpdatedDate.AddMinutes(randomMinutes);

            var invalidFhirRecordException =
                new InvalidFhirRecordException(
                    message: "Invalid fhirRecord. Please correct the errors and try again.");

            invalidFhirRecordException.AddData(
                key: nameof(FhirRecord.CreatedDate),
                values: $"Date is not the same as {nameof(FhirRecord.CreatedDate)}");

            var expectedFhirRecordValidationException =
                new FhirRecordValidationException(
                    message: "FhirRecord validation errors occurred, please try again.",
                    innerException: invalidFhirRecordException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidFhirRecord))
                    .ReturnsAsync(invalidFhirRecord);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordByIdAsync(invalidFhirRecord.Id))
                    .ReturnsAsync(storageFhirRecord);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUser.UserId);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(invalidFhirRecord, storageFhirRecord))
                    .ReturnsAsync(invalidFhirRecord);

            // when
            ValueTask<FhirRecord> modifyFhirRecordTask =
                this.fhirRecordService.ModifyFhirRecordAsync(invalidFhirRecord);

            FhirRecordValidationException actualFhirRecordValidationException =
                await Assert.ThrowsAsync<FhirRecordValidationException>(
                    modifyFhirRecordTask.AsTask);

            // then
            actualFhirRecordValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidFhirRecord),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordByIdAsync(invalidFhirRecord.Id),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(invalidFhirRecord, storageFhirRecord),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedFhirRecordValidationException))),
                       Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfCreatedUserDoesNotMatchStorageAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();
            User randomUser = CreateRandomUser(userId: randomUserId);

            FhirRecord randomFhirRecord =
                CreateRandomModifyFhirRecord(dateTimeOffset: randomDateTimeOffset, userId: randomUserId);

            FhirRecord invalidFhirRecord = randomFhirRecord.DeepClone();
            FhirRecord storageFhirRecord = invalidFhirRecord.DeepClone();
            invalidFhirRecord.CreatedBy = Guid.NewGuid().ToString();
            storageFhirRecord.UpdatedDate = storageFhirRecord.CreatedDate;

            var invalidFhirRecordException =
                new InvalidFhirRecordException(
                    message: "Invalid fhirRecord. Please correct the errors and try again.");

            invalidFhirRecordException.AddData(
                key: nameof(FhirRecord.CreatedBy),
                values: $"Text is not the same as {nameof(FhirRecord.CreatedBy)}");

            var expectedFhirRecordValidationException =
                new FhirRecordValidationException(
                    message: "FhirRecord validation errors occurred, please try again.",
                    innerException: invalidFhirRecordException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidFhirRecord))
                    .ReturnsAsync(invalidFhirRecord);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordByIdAsync(invalidFhirRecord.Id))
                    .ReturnsAsync(storageFhirRecord);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUser.UserId);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(invalidFhirRecord, storageFhirRecord))
                    .ReturnsAsync(invalidFhirRecord);

            // when
            ValueTask<FhirRecord> modifyFhirRecordTask =
                this.fhirRecordService.ModifyFhirRecordAsync(invalidFhirRecord);

            FhirRecordValidationException actualFhirRecordValidationException =
                await Assert.ThrowsAsync<FhirRecordValidationException>(
                    modifyFhirRecordTask.AsTask);

            // then
            actualFhirRecordValidationException.Should().BeEquivalentTo(expectedFhirRecordValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidFhirRecord),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordByIdAsync(invalidFhirRecord.Id),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(invalidFhirRecord, storageFhirRecord),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedFhirRecordValidationException))),
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
            User randomUser = CreateRandomUser(userId: randomUserId);

            FhirRecord randomFhirRecord =
                CreateRandomModifyFhirRecord(dateTimeOffset: randomDateTimeOffset, userId: randomUserId);

            FhirRecord invalidFhirRecord = randomFhirRecord;
            FhirRecord storageFhirRecord = randomFhirRecord.DeepClone();

            var invalidFhirRecordException =
                new InvalidFhirRecordException(
                    message: "Invalid fhirRecord. Please correct the errors and try again.");

            invalidFhirRecordException.AddData(
                key: nameof(FhirRecord.UpdatedDate),
                values: $"Date is the same as {nameof(FhirRecord.UpdatedDate)}");

            var expectedFhirRecordValidationException =
                new FhirRecordValidationException(
                    message: "FhirRecord validation errors occurred, please try again.",
                    innerException: invalidFhirRecordException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidFhirRecord))
                    .ReturnsAsync(invalidFhirRecord);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordByIdAsync(invalidFhirRecord.Id))
                    .ReturnsAsync(storageFhirRecord);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUser.UserId);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(invalidFhirRecord, storageFhirRecord))
                    .ReturnsAsync(invalidFhirRecord);

            // when
            ValueTask<FhirRecord> modifyFhirRecordTask =
                this.fhirRecordService.ModifyFhirRecordAsync(invalidFhirRecord);

            // then
            await Assert.ThrowsAsync<FhirRecordValidationException>(
                modifyFhirRecordTask.AsTask);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidFhirRecord),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordByIdAsync(invalidFhirRecord.Id),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(invalidFhirRecord, storageFhirRecord),
                    Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}