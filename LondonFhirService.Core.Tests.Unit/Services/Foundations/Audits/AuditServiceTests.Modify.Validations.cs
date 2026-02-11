// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.Audits;
using LondonFhirService.Core.Models.Foundations.Audits.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Audits
{
    public partial class AuditServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfAuditIsNullAndLogItAsync()
        {
            // given
            Audit nullAudit = null;
            var nullAuditException = new NullAuditServiceException(message: "Audit is null.");

            var expectedAuditValidationException =
                new AuditServiceValidationException(
                    message: "Audit validation errors occurred, please try again.",
                    innerException: nullAuditException);

            securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(nullAudit))
                    .ReturnsAsync(nullAudit);

            // when
            ValueTask<Audit> modifyAuditTask =
                this.auditService.ModifyAuditAsync(nullAudit);

            AuditServiceValidationException actualAuditValidationException =
                await Assert.ThrowsAsync<AuditServiceValidationException>(
                    modifyAuditTask.AsTask);

            // then
            actualAuditValidationException.Should()
                .BeEquivalentTo(expectedAuditValidationException);

            securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(nullAudit),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditValidationException))),
                        Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.storageBrokerFactoryMock.Verify(broker =>
                broker.CreateDbContextAsync(),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateAuditAsync(It.IsAny<Audit>()),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnModifyIfAuditIsInvalidAndLogItAsync(string invalidText)
        {
            // given 
            string randomUserId = GetRandomStringWithLengthOf(50);
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();

            var invalidAudit = new Audit
            {
                AuditType = invalidText,
                Title = invalidText,
            };

            securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidAudit))
                    .ReturnsAsync(invalidAudit);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            var invalidAuditException =
                new InvalidAuditServiceException(
                    message: "Invalid audit. Please correct the errors and try again.");

            invalidAuditException.AddData(
                key: nameof(Audit.Id),
                values: "Id is required");

            invalidAuditException.AddData(
                key: nameof(Audit.AuditType),
                values: "Text is required");

            invalidAuditException.AddData(
                key: nameof(Audit.Title),
                values: "Text is required");

            invalidAuditException.AddData(
                key: nameof(Audit.CreatedBy),
                values: "Text is required");

            invalidAuditException.AddData(
                key: nameof(Audit.CreatedDate),
                values:
                    [
                        "Date is required",
                    ]);

            invalidAuditException.AddData(
                key: nameof(Audit.UpdatedDate),
                values:
                    [
                        "Date is required",
                        "Date is the same as CreatedDate",
                        $"Date is not recent"
                    ]);

            invalidAuditException.AddData(
                key: nameof(Audit.UpdatedBy),
                values:
                    [
                        "Text is required",
                        $"Expected value to be '{randomUserId}' but found " +
                        $"'{invalidAudit.UpdatedBy}'."
                    ]);

            var expectedAuditValidationException =
                new AuditServiceValidationException(
                    message: "Audit validation errors occurred, please try again.",
                    innerException: invalidAuditException);

            // when
            ValueTask<Audit> modifyAuditTask =
                auditService.ModifyAuditAsync(invalidAudit);

            AuditServiceValidationException actualAuditValidationException =
                await Assert.ThrowsAsync<AuditServiceValidationException>(
                    modifyAuditTask.AsTask);

            //then
            actualAuditValidationException.Should()
                .BeEquivalentTo(expectedAuditValidationException);

            securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidAudit),
                    Times.Once());

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditValidationException))),
                        Times.Once());

            this.storageBrokerFactoryMock.Verify(broker =>
                broker.CreateDbContextAsync(),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateAuditAsync(It.IsAny<Audit>()),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfUpdatedDateIsSameAsCreatedDateAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomStringWithLengthOf(50);
            Audit randomAudit = CreateRandomAudit(randomDateTimeOffset, randomUserId);

            Audit invalidAudit = randomAudit;

            securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidAudit))
                    .ReturnsAsync(invalidAudit);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            var invalidAuditException =
                new InvalidAuditServiceException(
                    message: "Invalid audit. Please correct the errors and try again.");

            invalidAuditException.AddData(
                key: nameof(Audit.UpdatedDate),
                values: $"Date is the same as {nameof(Audit.CreatedDate)}");

            var expectedAuditValidationException =
                new AuditServiceValidationException(
                    message: "Audit validation errors occurred, please try again.",
                    innerException: invalidAuditException);

            // when
            ValueTask<Audit> modifyAuditTask =
                auditService.ModifyAuditAsync(invalidAudit);

            AuditServiceValidationException actualAuditValidationException =
                await Assert.ThrowsAsync<AuditServiceValidationException>(
                    modifyAuditTask.AsTask);

            // then
            actualAuditValidationException.Should()
                .BeEquivalentTo(expectedAuditValidationException);

            securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidAudit),
                    Times.Once());

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditValidationException))),
                        Times.Once);

            this.storageBrokerFactoryMock.Verify(broker =>
                broker.CreateDbContextAsync(),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(invalidAudit.Id),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(MinutesBeforeOrAfter))]
        public async Task ShouldThrowValidationExceptionOnModifyIfUpdatedDateIsNotRecentAndLogItAsync(int minutes)
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomStringWithLengthOf(50);
            Audit invalidAudit = CreateRandomAudit(randomDateTimeOffset, randomUserId);

            invalidAudit.UpdatedDate = randomDateTimeOffset.AddMinutes(minutes);

            securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidAudit))
                    .ReturnsAsync(invalidAudit);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            var invalidAuditException =
                new InvalidAuditServiceException(
                    message: "Invalid audit. Please correct the errors and try again.");

            invalidAuditException.AddData(
                key: nameof(Audit.UpdatedDate),
                values: "Date is not recent");

            var expectedAuditValidationException =
                new AuditServiceValidationException(
                    message: "Audit validation errors occurred, please try again.",
                    innerException: invalidAuditException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<Audit> modifyAuditTask =
                auditService.ModifyAuditAsync(invalidAudit);

            AuditServiceValidationException actualAuditValidationException =
                await Assert.ThrowsAsync<AuditServiceValidationException>(
                    modifyAuditTask.AsTask);

            // then
            actualAuditValidationException.Should()
                .BeEquivalentTo(expectedAuditValidationException);

            securityAuditBrokerMock.Verify(service =>
                service.ApplyModifyAuditValuesAsync(invalidAudit),
                    Times.Once());

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditValidationException))),
                        Times.Once);

            this.storageBrokerFactoryMock.Verify(broker =>
                broker.CreateDbContextAsync(),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfAuditDoesNotExistAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomStringWithLengthOf(50);
            Audit invalidAudit = CreateRandomModifyAudit(randomDateTimeOffset, randomUserId);

            Audit nonExistAudit = invalidAudit;

            var notFoundAuditException =
                new NotFoundAuditServiceException($"Couldn't find audit with auditId: {nonExistAudit.Id}.");

            var expectedAuditValidationException =
                new AuditServiceValidationException(
                    message: "Audit validation errors occurred, please try again.",
                    innerException: notFoundAuditException);

            securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidAudit))
                    .ReturnsAsync(invalidAudit);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.storageBrokerFactoryMock.Setup(broker =>
                broker.CreateDbContextAsync())
                    .ReturnsAsync(this.storageBrokerMock.Object);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAuditByIdAsync(nonExistAudit.Id))
                    .ReturnsAsync((Audit)null);

            // when 
            ValueTask<Audit> modifyAuditTask =
                auditService.ModifyAuditAsync(nonExistAudit);

            AuditServiceValidationException actualAuditValidationException =
                await Assert.ThrowsAsync<AuditServiceValidationException>(
                    modifyAuditTask.AsTask);

            // then
            actualAuditValidationException.Should()
                .BeEquivalentTo(expectedAuditValidationException);

            securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidAudit),
                    Times.Once());

            this.storageBrokerFactoryMock.Verify(broker =>
                broker.CreateDbContextAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(nonExistAudit.Id),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditValidationException))),
                        Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfStorageCreatedDateNotSameAsCreatedDateAndLogItAsync()
        {
            // given
            int randomNumber = GetRandomNegativeNumber();
            int randomMinutes = randomNumber;
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomStringWithLengthOf(50);
            Audit randomAudit = CreateRandomModifyAudit(randomDateTimeOffset, randomUserId);
            Audit invalidAudit = randomAudit.DeepClone();
            Audit storageAudit = invalidAudit.DeepClone();
            storageAudit.CreatedDate = storageAudit.CreatedDate.AddMinutes(randomMinutes);
            storageAudit.UpdatedDate = storageAudit.UpdatedDate.AddMinutes(randomMinutes);

            securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidAudit))
                    .ReturnsAsync(invalidAudit);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            var invalidAuditException =
                new InvalidAuditServiceException(
                    message: "Invalid audit. Please correct the errors and try again.");

            invalidAuditException.AddData(
                key: nameof(Audit.CreatedDate),
                values: $"Date is not the same as {nameof(Audit.CreatedDate)}");

            var expectedAuditValidationException =
                new AuditServiceValidationException(
                    message: "Audit validation errors occurred, please try again.",
                    innerException: invalidAuditException);

            this.storageBrokerFactoryMock.Setup(broker =>
                broker.CreateDbContextAsync())
                    .ReturnsAsync(this.storageBrokerMock.Object as StorageBroker);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAuditByIdAsync(invalidAudit.Id))
                    .ReturnsAsync(storageAudit);

            // when
            ValueTask<Audit> modifyAuditTask =
                auditService.ModifyAuditAsync(invalidAudit);

            AuditServiceValidationException actualAuditValidationException =
                await Assert.ThrowsAsync<AuditServiceValidationException>(
                    modifyAuditTask.AsTask);

            // then
            actualAuditValidationException.Should()
                .BeEquivalentTo(expectedAuditValidationException);

            securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidAudit),
                    Times.Once());

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.storageBrokerFactoryMock.Verify(broker =>
                broker.CreateDbContextAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(invalidAudit.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedAuditValidationException))),
                       Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfCreatedUserDontMatchStorageAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomStringWithLengthOf(50);
            Audit randomAudit = CreateRandomModifyAudit(randomDateTimeOffset, randomUserId);
            Audit invalidAudit = randomAudit.DeepClone();
            Audit storageAudit = invalidAudit.DeepClone();
            invalidAudit.CreatedBy = Guid.NewGuid().ToString();
            storageAudit.UpdatedDate = storageAudit.CreatedDate;

            var invalidAuditException =
                new InvalidAuditServiceException(
                    message: "Invalid audit. Please correct the errors and try again.");

            invalidAuditException.AddData(
                key: nameof(Audit.CreatedBy),
                values: $"Text is not the same as {nameof(Audit.CreatedBy)}");

            var expectedAuditValidationException =
                new AuditServiceValidationException(
                    message: "Audit validation errors occurred, please try again.",
                    innerException: invalidAuditException);

            securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidAudit))
                    .ReturnsAsync(invalidAudit);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.storageBrokerFactoryMock.Setup(broker =>
                broker.CreateDbContextAsync())
                    .ReturnsAsync(this.storageBrokerMock.Object as StorageBroker);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAuditByIdAsync(invalidAudit.Id))
                    .ReturnsAsync(storageAudit);

            // when
            ValueTask<Audit> modifyAuditTask =
                auditService.ModifyAuditAsync(invalidAudit);

            AuditServiceValidationException actualAuditValidationException =
                await Assert.ThrowsAsync<AuditServiceValidationException>(
                    modifyAuditTask.AsTask);

            // then
            actualAuditValidationException.Should().BeEquivalentTo(expectedAuditValidationException);

            securityAuditBrokerMock.Verify(service =>
                service.ApplyModifyAuditValuesAsync(invalidAudit),
                    Times.Once());

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.storageBrokerFactoryMock.Verify(broker =>
                broker.CreateDbContextAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(invalidAudit.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedAuditValidationException))),
                       Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfStorageUpdatedDateSameAsUpdatedDateAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomStringWithLengthOf(50);
            Audit randomAudit = CreateRandomModifyAudit(randomDateTimeOffset, randomUserId);
            Audit invalidAudit = randomAudit;
            Audit storageAudit = randomAudit.DeepClone();
            invalidAudit.UpdatedDate = storageAudit.UpdatedDate;

            var invalidAuditException =
                new InvalidAuditServiceException(
                    message: "Invalid audit. Please correct the errors and try again.");

            invalidAuditException.AddData(
                key: nameof(Audit.UpdatedDate),
                values: $"Date is the same as {nameof(Audit.UpdatedDate)}");

            var expectedAuditValidationException =
                new AuditServiceValidationException(
                    message: "Audit validation errors occurred, please try again.",
                    innerException: invalidAuditException);

            securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidAudit))
                    .ReturnsAsync(invalidAudit);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.storageBrokerFactoryMock.Setup(broker =>
                broker.CreateDbContextAsync())
                    .ReturnsAsync(this.storageBrokerMock.Object as StorageBroker);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAuditByIdAsync(invalidAudit.Id))
                    .ReturnsAsync(storageAudit);

            // when
            ValueTask<Audit> modifyAuditTask =
                auditService.ModifyAuditAsync(invalidAudit);

            // then
            await Assert.ThrowsAsync<AuditServiceValidationException>(
                modifyAuditTask.AsTask);

            securityAuditBrokerMock.Verify(service =>
                service.ApplyModifyAuditValuesAsync(invalidAudit),
                    Times.Once());

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditValidationException))),
                        Times.Once);

            this.storageBrokerFactoryMock.Verify(broker =>
                broker.CreateDbContextAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(invalidAudit.Id),
                    Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
        }
    }
}