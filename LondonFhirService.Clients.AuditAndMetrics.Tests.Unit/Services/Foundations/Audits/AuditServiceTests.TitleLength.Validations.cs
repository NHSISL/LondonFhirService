// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Clients.AuditAndMetrics.Models.Audits.Exceptions;
using LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Models.Audits;
using LondonFhirService.Core.Abstractions.Models.Audits;
using Moq;
using Tynamix.ObjectFiller;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Services.Foundations.Audits
{
    /// <summary>
    /// A migration narrows Audits.Title to nvarchar(500), so an over-length title is now a write
    /// the database will refuse. Audit writes are swallowed by design - a failed entry must never
    /// fail the request it describes - so without this rule the entry would simply vanish and the
    /// only trace of it would be a log line nobody is watching. Catching it in validation turns a
    /// silent loss into a caller error.
    ///
    /// The boundary is the test worth having: 500 is the column width, not one less than it, and a
    /// rule written as >= would reject a title that fits. Only a test sitting exactly on 500 tells
    /// the two apart.
    ///
    /// These drive AddAuditAsync and ModifyAuditAsync rather than the Log* twins because Log*
    /// swallows the exception - the validation would still run, but there would be nothing to
    /// assert on.
    /// </summary>
    public partial class AuditServiceTests
    {
        [Fact]
        public async Task ShouldAddAuditWhenTitleIsExactlyAtTheMaxLengthAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();
            TestAudit audit = CreateUnstampedAudit();
            audit.Title = GetRandomTitleWithLengthOf(500);

            this.dateTimeBrokerMock.Setup(broker => broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(randomDateTimeOffset);

            this.auditUserBrokerMock.Setup(broker => broker.GetCurrentUserIdAsync())
                .ReturnsAsync(randomUserId);

            this.auditStorageBrokerMock.Setup(broker =>
                broker.InsertAuditAsync(audit, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(audit);

            // when
            IAudit actualAudit = await this.auditService.AddAuditAsync(
                audit, TestContext.Current.CancellationToken);

            // then
            // A title that fills the column exactly has to go through. Rejecting it would make the
            // rule stricter than the schema it exists to protect.
            actualAudit.Should().BeSameAs(audit);
            actualAudit.Title.Should().HaveLength(500);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.auditUserBrokerMock.Verify(broker =>
                broker.GetCurrentUserIdAsync(),
                    Times.Once);

            this.auditStorageBrokerMock.Verify(broker =>
                broker.InsertAuditAsync(audit, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
                    Times.Never);

            this.auditStorageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.auditUserBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dispatcherMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfTitleExceedsMaxLengthAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();
            TestAudit invalidAudit = CreateUnstampedAudit();
            invalidAudit.Title = GetRandomTitleWithLengthOf(501);

            var invalidAuditException =
                new InvalidAuditException(
                    message: "Invalid audit. Please correct the errors and try again.");

            invalidAuditException.AddData(
                key: nameof(IAudit.Title),
                values: "Text exceeds max length of 500 characters");

            var expectedAuditValidationException =
                new AuditValidationException(
                    message: "Audit validation errors occurred, please try again.",
                    innerException: invalidAuditException);

            this.dateTimeBrokerMock.Setup(broker => broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(randomDateTimeOffset);

            this.auditUserBrokerMock.Setup(broker => broker.GetCurrentUserIdAsync())
                .ReturnsAsync(randomUserId);

            // when
            ValueTask<IAudit> addAuditTask =
                this.auditService.AddAuditAsync(invalidAudit, TestContext.Current.CancellationToken);

            AuditValidationException actualAuditValidationException =
                await Assert.ThrowsAsync<AuditValidationException>(addAuditTask.AsTask);

            // then
            // One character over the column width, and the caller is told which property is at
            // fault rather than being handed a truncation error from the database - or, on the
            // dispatched path, nothing at all.
            actualAuditValidationException.Should().BeEquivalentTo(expectedAuditValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.auditUserBrokerMock.Verify(broker =>
                broker.GetCurrentUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditValidationException))),
                        Times.Once);

            this.auditStorageBrokerMock.Verify(broker =>
                broker.InsertAuditAsync(It.IsAny<IAudit>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.auditStorageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.auditUserBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dispatcherMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldModifyAuditWhenTitleIsExactlyAtTheMaxLengthAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            TestAudit storedAudit = CreateUnstampedAudit();
            storedAudit.CreatedBy = GetRandomString();
            storedAudit.CreatedDate = randomDateTimeOffset;

            TestAudit inputAudit = CreateUnstampedAudit();
            inputAudit.Id = storedAudit.Id;
            inputAudit.CreatedBy = storedAudit.CreatedBy;
            inputAudit.CreatedDate = storedAudit.CreatedDate;
            inputAudit.Title = GetRandomTitleWithLengthOf(500);

            this.dateTimeBrokerMock.Setup(broker => broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(randomDateTimeOffset);

            this.auditStorageBrokerMock.Setup(broker =>
                broker.SelectAuditByIdAsync(storedAudit.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storedAudit);

            this.auditStorageBrokerMock.Setup(broker =>
                broker.UpdateAuditAsync(inputAudit, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(inputAudit);

            // when
            IAudit actualAudit = await this.auditService.ModifyAuditAsync(
                inputAudit, TestContext.Current.CancellationToken);

            // then
            // The same boundary on the update path. The column does not care which statement the
            // row arrived through, so neither can the rule.
            actualAudit.Should().BeSameAs(inputAudit);
            actualAudit.Title.Should().HaveLength(500);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.auditStorageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(storedAudit.Id, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.auditStorageBrokerMock.Verify(broker =>
                broker.UpdateAuditAsync(inputAudit, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
                    Times.Never);

            this.auditStorageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.auditUserBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dispatcherMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfTitleExceedsMaxLengthAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            TestAudit invalidAudit = CreateUnstampedAudit();
            invalidAudit.Title = GetRandomTitleWithLengthOf(501);

            var invalidAuditException =
                new InvalidAuditException(
                    message: "Invalid audit. Please correct the errors and try again.");

            invalidAuditException.AddData(
                key: nameof(IAudit.Title),
                values: "Text exceeds max length of 500 characters");

            var expectedAuditValidationException =
                new AuditValidationException(
                    message: "Audit validation errors occurred, please try again.",
                    innerException: invalidAuditException);

            this.dateTimeBrokerMock.Setup(broker => broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<IAudit> modifyAuditTask =
                this.auditService.ModifyAuditAsync(invalidAudit, TestContext.Current.CancellationToken);

            AuditValidationException actualAuditValidationException =
                await Assert.ThrowsAsync<AuditValidationException>(modifyAuditTask.AsTask);

            // then
            actualAuditValidationException.Should().BeEquivalentTo(expectedAuditValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedAuditValidationException))),
                        Times.Once);

            // Rejected before the row is even read, so an over-length edit costs no round trip and
            // leaves the stored entry untouched.
            this.auditStorageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.auditStorageBrokerMock.Verify(broker =>
                broker.UpdateAuditAsync(It.IsAny<IAudit>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.auditStorageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.auditUserBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dispatcherMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ShouldAddAuditWhenTitleIsMissingAsync(string missingTitle)
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();
            TestAudit audit = CreateUnstampedAudit();
            audit.Title = missingTitle;

            this.dateTimeBrokerMock.Setup(broker => broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(randomDateTimeOffset);

            this.auditUserBrokerMock.Setup(broker => broker.GetCurrentUserIdAsync())
                .ReturnsAsync(randomUserId);

            this.auditStorageBrokerMock.Setup(broker =>
                broker.InsertAuditAsync(audit, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(audit);

            // when
            IAudit actualAudit = await this.auditService.AddAuditAsync(
                audit, TestContext.Current.CancellationToken);

            // then
            // Title is optional - the length rule coalesces a null to empty rather than tripping
            // over it. Turning a missing title into a validation error would reject entries the
            // schema is perfectly happy to store.
            actualAudit.Should().BeSameAs(audit);
            actualAudit.Title.Should().Be(missingTitle);

            this.auditStorageBrokerMock.Verify(broker =>
                broker.InsertAuditAsync(audit, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.auditUserBrokerMock.Verify(broker =>
                broker.GetCurrentUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
                    Times.Never);

            this.auditStorageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.auditUserBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dispatcherMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// The generator is asked for a word of an exact size but is not contractually bound to
        /// one, so the result is trimmed or padded to length. A boundary test that quietly ran at
        /// 499 characters would pass for the wrong reason and never fail when the rule broke.
        /// </summary>
        private static string GetRandomTitleWithLengthOf(int length)
        {
            string result =
                new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length >= length
                ? result.Substring(0, length)
                : result.PadRight(length, 'a');
        }
    }
}
