// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Audits;
using LondonFhirService.Core.Models.Foundations.Audits.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Audits
{
    public partial class AuditServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfAuditIsNullAndLogItAsync()
        {
            // given
            Audit nullAudit = null;

            var nullAuditException =
                new NullAuditServiceException(message: "Audit is null.");

            var expectedAuditValidationException =
                new AuditServiceValidationException(
                    message: "Audit validation errors occurred, please try again.",
                    innerException: nullAuditException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(nullAudit))
                    .ReturnsAsync(nullAudit);

            // when
            ValueTask<Audit> addAuditTask =
                this.auditService.AddAuditAsync(nullAudit);

            AuditServiceValidationException actualAuditValidationException =
                await Assert.ThrowsAsync<AuditServiceValidationException>(addAuditTask.AsTask);

            // then
            actualAuditValidationException.Should()
                .BeEquivalentTo(expectedAuditValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(nullAudit),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditValidationException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnAddIfAuditsIsInvalidAndLogItAsync(string invalidText)
        {
            // given
            DateTimeOffset randomDataTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomStringWithLengthOf(50);

            var invalidAudit = new Audit
            {
                AuditType = invalidText,
                Title = invalidText,
            };

            securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(invalidAudit))
                    .ReturnsAsync(invalidAudit);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDataTimeOffset);

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
                 key: nameof(Audit.CreatedDate),
                 values:
                 [
                    "Date is required",
                    $"Date is not recent"
                 ]);

            invalidAuditException.AddData(
                key: nameof(Audit.CreatedBy),
                values:
                [
                    "Text is required",
                    $"Expected value to be '{randomUserId}' but found '{invalidAudit.CreatedBy}'."
                ]);

            invalidAuditException.AddData(
                key: nameof(Audit.UpdatedDate),
                values: "Date is required");

            invalidAuditException.AddData(
                key: nameof(Audit.UpdatedBy),
                values: "Text is required");

            var expectedAuditValidationException =
                new AuditServiceValidationException(
                    message: "Audit validation errors occurred, please try again.",
                    innerException: invalidAuditException);

            // when
            ValueTask<Audit> addAuditTask =
                auditService.AddAuditAsync(invalidAudit);

            AuditServiceValidationException actualAuditValidationException =
                await Assert.ThrowsAsync<AuditServiceValidationException>(addAuditTask.AsTask);

            // then
            actualAuditValidationException.Should()
                .BeEquivalentTo(expectedAuditValidationException);

            securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(invalidAudit),
                    Times.Once());

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once());

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertAuditAsync(It.IsAny<Audit>()),
                    Times.Never);

            securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfCreateAndUpdateDatesIsNotSameAndLogItAsync()
        {
            // given
            int randomNumber = GetRandomNumber();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomStringWithLengthOf(50);

            Audit randomAudit =
                CreateRandomAudit(randomDateTimeOffset, randomUserId);

            Audit invalidAudit = randomAudit;
            invalidAudit.CreatedDate = GetRandomDateTimeOffset();
            invalidAudit.UpdatedDate = GetRandomDateTimeOffset();

            var invalidAuditException =
                new InvalidAuditServiceException(
                    message: "Invalid audit. Please correct the errors and try again.");

            invalidAuditException.AddData(
                key: nameof(Audit.UpdatedDate),
                values: $"Date is not the same as {nameof(Audit.CreatedDate)}");

            invalidAuditException.AddData(
                key: nameof(Audit.CreatedDate),
                values: $"Date is not recent");

            var expectedAuditValidationException =
                new AuditServiceValidationException(
                    message: "Audit validation errors occurred, please try again.",
                    innerException: invalidAuditException);

            securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(invalidAudit))
                    .ReturnsAsync(invalidAudit);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<Audit> addAuditTask =
                auditService.AddAuditAsync(invalidAudit);

            AuditServiceValidationException actualAuditValidationException =
                await Assert.ThrowsAsync<AuditServiceValidationException>(addAuditTask.AsTask);

            // then
            actualAuditValidationException.Should()
                .BeEquivalentTo(expectedAuditValidationException);

            securityAuditBrokerMock.Verify(service =>
                service.ApplyAddAuditValuesAsync(invalidAudit),
                    Times.Once());

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once());

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertAuditAsync(It.IsAny<Audit>()),
                    Times.Never);

            securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfCreateAndUpdateUsersIsNotSameAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomStringWithLengthOf(50);

            Audit randomAudit =
                CreateRandomAudit(randomDateTimeOffset, randomUserId);

            Audit invalidAudit = randomAudit;
            invalidAudit.CreatedBy = GetRandomString();
            invalidAudit.UpdatedBy = GetRandomString();

            var invalidAuditException =
                new InvalidAuditServiceException(
                    message: "Invalid audit. Please correct the errors and try again.");

            invalidAuditException.AddData(
                key: nameof(Audit.CreatedBy),
                values: $"Expected value to be '{randomUserId}' " +
                    $"but found '{invalidAudit.CreatedBy}'.");

            invalidAuditException.AddData(
                key: nameof(Audit.UpdatedBy),
                values: $"Text is not the same as {nameof(Audit.CreatedBy)}");

            var expectedAuditValidationException =
                new AuditServiceValidationException(
                    message: "Audit validation errors occurred, please try again.",
                    innerException: invalidAuditException);

            securityAuditBrokerMock.Setup(service =>
                service.ApplyAddAuditValuesAsync(invalidAudit))
                    .ReturnsAsync(invalidAudit);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<Audit> addAuditTask =
                auditService.AddAuditAsync(invalidAudit);

            AuditServiceValidationException actualAuditValidationException =
                await Assert.ThrowsAsync<AuditServiceValidationException>(addAuditTask.AsTask);

            // then
            actualAuditValidationException.Should()
                .BeEquivalentTo(expectedAuditValidationException);

            securityAuditBrokerMock.Verify(service =>
                service.ApplyAddAuditValuesAsync(invalidAudit),
                    Times.Once());

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once());

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertAuditAsync(It.IsAny<Audit>()),
                    Times.Never);

            securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
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
            string randomUserId = GetRandomStringWithLengthOf(50);
            DateTimeOffset invalidDateTime = randomDateTimeOffset.AddMinutes(minutesBeforeOrAfter);

            Audit randomAudit =
                CreateRandomAudit(invalidDateTime, randomUserId);

            Audit invalidAudit = randomAudit;

            var invalidAuditException =
                new InvalidAuditServiceException(
                    message: "Invalid audit. Please correct the errors and try again.");

            invalidAuditException.AddData(
                key: nameof(Audit.CreatedDate),
                values: "Date is not recent");

            var expectedAuditValidationException =
                new AuditServiceValidationException(
                    message: "Audit validation errors occurred, please try again.",
                    innerException: invalidAuditException);

            securityAuditBrokerMock.Setup(service =>
                service.ApplyAddAuditValuesAsync(invalidAudit))
                    .ReturnsAsync(invalidAudit);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<Audit> addAuditTask =
                auditService.AddAuditAsync(invalidAudit);

            AuditServiceValidationException actualAuditValidationException =
                await Assert.ThrowsAsync<AuditServiceValidationException>(addAuditTask.AsTask);

            // then
            actualAuditValidationException.Should()
                .BeEquivalentTo(expectedAuditValidationException);

            securityAuditBrokerMock.Verify(service =>
                service.ApplyAddAuditValuesAsync(invalidAudit),
                    Times.Once());

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once());

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertAuditAsync(It.IsAny<Audit>()),
                    Times.Never);

            securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}