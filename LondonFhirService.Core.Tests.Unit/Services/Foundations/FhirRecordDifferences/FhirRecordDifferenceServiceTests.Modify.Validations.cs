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
        public async Task ShouldThrowValidationExceptionOnModifyIfFhirRecordDifferenceIsNullAndLogItAsync()
        {
            // given
            FhirRecordDifference nullFhirRecordDifference = null;
            var nullFhirRecordDifferenceException = new NullFhirRecordDifferenceException(message: "FhirRecordDifference is null.");

            var expectedFhirRecordDifferenceValidationException =
                new FhirRecordDifferenceValidationException(
                    message: "FhirRecordDifference validation errors occurred, please try again.",
                    innerException: nullFhirRecordDifferenceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(nullFhirRecordDifference))
                    .ReturnsAsync(nullFhirRecordDifference);

            // when
            ValueTask<FhirRecordDifference> modifyFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.ModifyFhirRecordDifferenceAsync(nullFhirRecordDifference);

            FhirRecordDifferenceValidationException actualFhirRecordDifferenceValidationException =
                await Assert.ThrowsAsync<FhirRecordDifferenceValidationException>(
                    modifyFhirRecordDifferenceTask.AsTask);

            // then
            actualFhirRecordDifferenceValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(nullFhirRecordDifference),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceValidationException))),
                        Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnModifyIfFhirRecordDifferenceIsInvalidAndLogItAsync(string invalidText)
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
                values: "Text is required");

            invalidFhirRecordDifferenceException.AddData(
                key: nameof(FhirRecordDifference.UpdatedDate),
                values:
                new[] {
                    "Date is required",
                    $"Date is the same as {nameof(FhirRecordDifference.CreatedDate)}"
                });

            invalidFhirRecordDifferenceException.AddData(
                key: nameof(FhirRecordDifference.UpdatedBy),
                values:
                    [
                        "Text is required",
                        $"Expected value to be '{randomUserId}' but found '{invalidFhirRecordDifference.UpdatedBy}'."
                    ]);

            var expectedFhirRecordDifferenceValidationException =
                new FhirRecordDifferenceValidationException(
                    message: "FhirRecordDifference validation errors occurred, please try again.",
                    innerException: invalidFhirRecordDifferenceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidFhirRecordDifference))
                    .ReturnsAsync(invalidFhirRecordDifference);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            // when
            ValueTask<FhirRecordDifference> modifyFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.ModifyFhirRecordDifferenceAsync(invalidFhirRecordDifference);

            FhirRecordDifferenceValidationException actualFhirRecordDifferenceValidationException =
                await Assert.ThrowsAsync<FhirRecordDifferenceValidationException>(
                    modifyFhirRecordDifferenceTask.AsTask);

            //then
            actualFhirRecordDifferenceValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidFhirRecordDifference),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceValidationException))),
                        Times.Once());

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfFhirRecordDifferenceHasInvalidLengthProperty()
        {
            // given
            string randomUserId = GetRandomStringWithLengthOf(256);
            User randomUser = CreateRandomUser(userId: randomUserId);

            var invalidFhirRecordDifference =
                CreateRandomModifyFhirRecordDifference(GetRandomDateTimeOffset(), userId: randomUserId);

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
                broker.ApplyModifyAuditValuesAsync(invalidFhirRecordDifference))
                    .ReturnsAsync(invalidFhirRecordDifference);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            // when
            ValueTask<FhirRecordDifference> modifyFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.ModifyFhirRecordDifferenceAsync(invalidFhirRecordDifference);

            FhirRecordDifferenceValidationException actualFhirRecordDifferenceValidationException =
                await Assert.ThrowsAsync<FhirRecordDifferenceValidationException>(
                    modifyFhirRecordDifferenceTask.AsTask);

            // then
            actualFhirRecordDifferenceValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidFhirRecordDifference),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once());

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
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

            FhirRecordDifference randomFhirRecordDifference =
                CreateRandomFhirRecordDifference(dateTimeOffset: randomDateTimeOffset, userId: randomUserId);

            FhirRecordDifference invalidFhirRecordDifference = randomFhirRecordDifference;

            var invalidFhirRecordDifferenceException =
                new InvalidFhirRecordDifferenceException(
                    message: "Invalid fhirRecordDifference. Please correct the errors and try again.");

            invalidFhirRecordDifferenceException.AddData(
                key: nameof(FhirRecordDifference.UpdatedDate),
                values: $"Date is the same as {nameof(FhirRecordDifference.CreatedDate)}");

            var expectedFhirRecordDifferenceValidationException =
                new FhirRecordDifferenceValidationException(
                    message: "FhirRecordDifference validation errors occurred, please try again.",
                    innerException: invalidFhirRecordDifferenceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidFhirRecordDifference))
                    .ReturnsAsync(invalidFhirRecordDifference);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            // when
            ValueTask<FhirRecordDifference> modifyFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.ModifyFhirRecordDifferenceAsync(invalidFhirRecordDifference);

            FhirRecordDifferenceValidationException actualFhirRecordDifferenceValidationException =
                await Assert.ThrowsAsync<FhirRecordDifferenceValidationException>(
                    modifyFhirRecordDifferenceTask.AsTask);

            // then
            actualFhirRecordDifferenceValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidFhirRecordDifference),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(invalidFhirRecordDifference.Id),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
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

            FhirRecordDifference randomFhirRecordDifference =
                CreateRandomFhirRecordDifference(dateTimeOffset: randomDateTimeOffset, userId: randomUserId);

            randomFhirRecordDifference.UpdatedDate = randomDateTimeOffset.AddMinutes(minutes);

            var invalidFhirRecordDifferenceException =
                new InvalidFhirRecordDifferenceException(
                    message: "Invalid fhirRecordDifference. Please correct the errors and try again.");

            invalidFhirRecordDifferenceException.AddData(
                key: nameof(FhirRecordDifference.UpdatedDate),
                values:
                    $"Date is not recent. Expected a value between {startDate} and {endDate} but found {invalidDate}");

            var expectedFhirRecordDifferenceValidatonException =
                new FhirRecordDifferenceValidationException(
                    message: "FhirRecordDifference validation errors occurred, please try again.",
                    innerException: invalidFhirRecordDifferenceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(randomFhirRecordDifference))
                    .ReturnsAsync(randomFhirRecordDifference);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(randomDateTimeOffset);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            // when
            ValueTask<FhirRecordDifference> modifyFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.ModifyFhirRecordDifferenceAsync(randomFhirRecordDifference);

            FhirRecordDifferenceValidationException actualFhirRecordDifferenceValidationException =
                await Assert.ThrowsAsync<FhirRecordDifferenceValidationException>(
                    modifyFhirRecordDifferenceTask.AsTask);

            // then
            actualFhirRecordDifferenceValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceValidatonException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(randomFhirRecordDifference),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceValidatonException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfFhirRecordDifferenceDoesNotExistAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();
            User randomUser = CreateRandomUser(userId: randomUserId);

            FhirRecordDifference randomFhirRecordDifference = CreateRandomModifyFhirRecordDifference(
                dateTimeOffset: randomDateTimeOffset, userId: randomUserId);

            FhirRecordDifference nonExistFhirRecordDifference = randomFhirRecordDifference;
            FhirRecordDifference nullFhirRecordDifference = null;

            var notFoundFhirRecordDifferenceException = new NotFoundFhirRecordDifferenceException(
                message: $"Couldn't find fhirRecordDifference with Id: {nonExistFhirRecordDifference.Id}.");

            var expectedFhirRecordDifferenceValidationException =
                new FhirRecordDifferenceValidationException(
                    message: "FhirRecordDifference validation errors occurred, please try again.",
                    innerException: notFoundFhirRecordDifferenceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(nonExistFhirRecordDifference))
                    .ReturnsAsync(nonExistFhirRecordDifference);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(nonExistFhirRecordDifference.Id))
                    .ReturnsAsync(nullFhirRecordDifference);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            // when 
            ValueTask<FhirRecordDifference> modifyFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.ModifyFhirRecordDifferenceAsync(nonExistFhirRecordDifference);

            FhirRecordDifferenceValidationException actualFhirRecordDifferenceValidationException =
                await Assert.ThrowsAsync<FhirRecordDifferenceValidationException>(
                    modifyFhirRecordDifferenceTask.AsTask);

            // then
            actualFhirRecordDifferenceValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(nonExistFhirRecordDifference),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(nonExistFhirRecordDifference.Id),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceValidationException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
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

            FhirRecordDifference randomFhirRecordDifference = CreateRandomModifyFhirRecordDifference(
                dateTimeOffset: randomDateTimeOffset, userId: randomUserId);

            FhirRecordDifference invalidFhirRecordDifference = randomFhirRecordDifference.DeepClone();
            FhirRecordDifference storageFhirRecordDifference = invalidFhirRecordDifference.DeepClone();
            storageFhirRecordDifference.CreatedDate = storageFhirRecordDifference.CreatedDate.AddMinutes(randomMinutes);
            storageFhirRecordDifference.UpdatedDate = storageFhirRecordDifference.UpdatedDate.AddMinutes(randomMinutes);

            var invalidFhirRecordDifferenceException =
                new InvalidFhirRecordDifferenceException(
                    message: "Invalid fhirRecordDifference. Please correct the errors and try again.");

            invalidFhirRecordDifferenceException.AddData(
                key: nameof(FhirRecordDifference.CreatedDate),
                values: $"Date is not the same as {nameof(FhirRecordDifference.CreatedDate)}");

            var expectedFhirRecordDifferenceValidationException =
                new FhirRecordDifferenceValidationException(
                    message: "FhirRecordDifference validation errors occurred, please try again.",
                    innerException: invalidFhirRecordDifferenceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidFhirRecordDifference))
                    .ReturnsAsync(invalidFhirRecordDifference);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(invalidFhirRecordDifference.Id))
                    .ReturnsAsync(storageFhirRecordDifference);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(invalidFhirRecordDifference, storageFhirRecordDifference))
                    .ReturnsAsync(invalidFhirRecordDifference);

            // when
            ValueTask<FhirRecordDifference> modifyFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.ModifyFhirRecordDifferenceAsync(invalidFhirRecordDifference);

            FhirRecordDifferenceValidationException actualFhirRecordDifferenceValidationException =
                await Assert.ThrowsAsync<FhirRecordDifferenceValidationException>(
                    modifyFhirRecordDifferenceTask.AsTask);

            // then
            actualFhirRecordDifferenceValidationException.Should()
                .BeEquivalentTo(expectedFhirRecordDifferenceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidFhirRecordDifference),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(invalidFhirRecordDifference.Id),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(invalidFhirRecordDifference, storageFhirRecordDifference),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedFhirRecordDifferenceValidationException))),
                       Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfCreatedUserDontMacthStorageAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();
            User randomUser = CreateRandomUser(userId: randomUserId);

            FhirRecordDifference randomFhirRecordDifference =
                CreateRandomModifyFhirRecordDifference(dateTimeOffset: randomDateTimeOffset, userId: randomUserId);

            FhirRecordDifference invalidFhirRecordDifference = randomFhirRecordDifference.DeepClone();
            FhirRecordDifference storageFhirRecordDifference = invalidFhirRecordDifference.DeepClone();
            invalidFhirRecordDifference.CreatedBy = Guid.NewGuid().ToString();
            storageFhirRecordDifference.UpdatedDate = storageFhirRecordDifference.CreatedDate;

            var invalidFhirRecordDifferenceException =
                new InvalidFhirRecordDifferenceException(
                    message: "Invalid fhirRecordDifference. Please correct the errors and try again.");

            invalidFhirRecordDifferenceException.AddData(
                key: nameof(FhirRecordDifference.CreatedBy),
                values: $"Text is not the same as {nameof(FhirRecordDifference.CreatedBy)}");

            var expectedFhirRecordDifferenceValidationException =
                new FhirRecordDifferenceValidationException(
                    message: "FhirRecordDifference validation errors occurred, please try again.",
                    innerException: invalidFhirRecordDifferenceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidFhirRecordDifference))
                    .ReturnsAsync(invalidFhirRecordDifference);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(invalidFhirRecordDifference.Id))
                    .ReturnsAsync(storageFhirRecordDifference);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(invalidFhirRecordDifference, storageFhirRecordDifference))
                    .ReturnsAsync(invalidFhirRecordDifference);

            // when
            ValueTask<FhirRecordDifference> modifyFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.ModifyFhirRecordDifferenceAsync(invalidFhirRecordDifference);

            FhirRecordDifferenceValidationException actualFhirRecordDifferenceValidationException =
                await Assert.ThrowsAsync<FhirRecordDifferenceValidationException>(
                    modifyFhirRecordDifferenceTask.AsTask);

            // then
            actualFhirRecordDifferenceValidationException.Should().BeEquivalentTo(expectedFhirRecordDifferenceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidFhirRecordDifference),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(invalidFhirRecordDifference.Id),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(invalidFhirRecordDifference, storageFhirRecordDifference),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedFhirRecordDifferenceValidationException))),
                       Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
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

            FhirRecordDifference randomFhirRecordDifference =
                CreateRandomModifyFhirRecordDifference(dateTimeOffset: randomDateTimeOffset, userId: randomUserId);

            FhirRecordDifference invalidFhirRecordDifference = randomFhirRecordDifference;
            FhirRecordDifference storageFhirRecordDifference = randomFhirRecordDifference.DeepClone();

            var invalidFhirRecordDifferenceException =
                new InvalidFhirRecordDifferenceException(
                    message: "Invalid fhirRecordDifference. Please correct the errors and try again.");

            invalidFhirRecordDifferenceException.AddData(
                key: nameof(FhirRecordDifference.UpdatedDate),
                values: $"Date is the same as {nameof(FhirRecordDifference.UpdatedDate)}");

            var expectedFhirRecordDifferenceValidationException =
                new FhirRecordDifferenceValidationException(
                    message: "FhirRecordDifference validation errors occurred, please try again.",
                    innerException: invalidFhirRecordDifferenceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidFhirRecordDifference))
                    .ReturnsAsync(invalidFhirRecordDifference);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(invalidFhirRecordDifference.Id))
                    .ReturnsAsync(storageFhirRecordDifference);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(invalidFhirRecordDifference, storageFhirRecordDifference))
                    .ReturnsAsync(invalidFhirRecordDifference);

            // when
            ValueTask<FhirRecordDifference> modifyFhirRecordDifferenceTask =
                this.fhirRecordDifferenceService.ModifyFhirRecordDifferenceAsync(invalidFhirRecordDifference);

            // then
            await Assert.ThrowsAsync<FhirRecordDifferenceValidationException>(
                modifyFhirRecordDifferenceTask.AsTask);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidFhirRecordDifference),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirRecordDifferenceValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(invalidFhirRecordDifference.Id),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(invalidFhirRecordDifference, storageFhirRecordDifference),
                    Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}