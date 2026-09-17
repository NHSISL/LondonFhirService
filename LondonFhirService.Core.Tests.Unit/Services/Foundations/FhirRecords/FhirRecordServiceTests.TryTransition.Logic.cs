// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.FhirRecords
{
    public partial class FhirRecordServiceTests
    {
        [Theory]
        [InlineData(1, true)]
        [InlineData(0, false)]
        public async Task ShouldReportWhetherTheTransitionChangedTheRowAsync(
            int changedRowCount,
            bool expectedResult)
        {
            // given
            Guid inputFhirRecordId = Guid.NewGuid();
            StatusType inputExcludedStatus = StatusType.Completed;
            StatusType inputNewStatus = StatusType.Completed;
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset updatedDate = randomDateTimeOffset;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(updatedDate);

            this.storageBrokerMock.Setup(broker =>
                broker.UpdateFhirRecordStatusAsync(
                    inputFhirRecordId,
                    inputExcludedStatus,
                    inputNewStatus,
                    true,
                    updatedDate,
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(changedRowCount);

            // when
            bool actualResult = await this.fhirRecordService.TryTransitionFhirRecordStatusAsync(
                inputFhirRecordId,
                inputExcludedStatus,
                inputNewStatus,
                isProcessed: true,
                cancellationToken: TestContext.Current.CancellationToken);

            // then
            // The rows-affected count IS the answer: one means this call performed the transition,
            // zero means the row was already in the target status and somebody else got there
            // first. The primary record is shared by every secondary of a correlation, so that
            // distinction is what lets several workers complete it without racing.
            actualResult.Should().Be(expectedResult);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateFhirRecordStatusAsync(
                    inputFhirRecordId,
                    inputExcludedStatus,
                    inputNewStatus,
                    true,
                    updatedDate,
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            // Fetched and put into the statement by hand, because ExecuteUpdateAsync goes
            // round the change tracker and nothing else would stamp the actor on an
            // IAuditable row whose UpdatedDate this call moves.
            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldStampTheAuditedActorOnTransitionAsync()
        {
            // given
            Guid inputFhirRecordId = Guid.NewGuid();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();
            string expectedUpdatedBy = randomUserId;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.storageBrokerMock.Setup(broker =>
                broker.UpdateFhirRecordStatusAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<bool>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(1);

            // when
            await this.fhirRecordService.TryTransitionFhirRecordStatusAsync(
                inputFhirRecordId,
                excludedStatus: StatusType.Completed,
                newStatus: StatusType.Completed,
                isProcessed: true,
                cancellationToken: TestContext.Current.CancellationToken);

            // then
            // FhirRecord is IAuditable, and the read-then-write path this replaced ran through
            // ApplyModifyAuditValuesAsync - so completing a primary used to stamp the actor.
            // ExecuteUpdateAsync goes round the change tracker, so the value has to be fetched
            // here and put into the statement, or the row would record a move with the previous
            // actor still on it. Taken from the same broker the audited path used, so this
            // records what that path recorded rather than inventing an identity for the worker.
            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateFhirRecordStatusAsync(
                    inputFhirRecordId,
                    StatusType.Completed,
                    StatusType.Completed,
                    true,
                    randomDateTimeOffset,
                    expectedUpdatedBy,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldStampTheAuditedActorOnClaimAsync()
        {
            // given
            Guid inputFhirRecordId = Guid.NewGuid();
            DateTimeOffset claimedDate = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();
            string expectedClaimedBy = randomUserId;

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.storageBrokerMock.Setup(broker =>
                broker.ClaimFhirRecordAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(1);

            // when
            await this.fhirRecordService.TryClaimFhirRecordAsync(
                inputFhirRecordId,
                expectedStatus: StatusType.Pending,
                claimedStatus: StatusType.Processing,
                claimedDate: claimedDate,
                cancellationToken: TestContext.Current.CancellationToken);

            // then
            // The claim moves UpdatedDate as its lease token, so without this it would move the
            // timestamp on an auditable row and leave the actor behind. Same gap as the
            // transition above and the same value, from the same broker.
            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.ClaimFhirRecordAsync(
                    inputFhirRecordId,
                    StatusType.Pending,
                    StatusType.Processing,
                    claimedDate,
                    expectedClaimedBy,
                    null,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldForwardTheProcessedFlagOnTransitionAsync()
        {
            // given
            Guid inputFhirRecordId = Guid.NewGuid();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.UpdateFhirRecordStatusAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<bool>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(1);

            // when
            await this.fhirRecordService.TryTransitionFhirRecordStatusAsync(
                inputFhirRecordId,
                excludedStatus: StatusType.Completed,
                newStatus: StatusType.Completed,
                isProcessed: false,
                cancellationToken: TestContext.Current.CancellationToken);

            // then
            // Passed through rather than derived here. Which statuses are terminal is the caller's
            // business, and the read-then-write path this replaced set the flag for terminal ones -
            // so the value has to reach the statement unchanged or completed rows stay unprocessed.
            this.storageBrokerMock.Verify(broker =>
                broker.UpdateFhirRecordStatusAsync(
                    inputFhirRecordId,
                    StatusType.Completed,
                    StatusType.Completed,
                    false,
                    randomDateTimeOffset,
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            // Fetched and put into the statement by hand, because ExecuteUpdateAsync goes
            // round the change tracker and nothing else would stamp the actor on an
            // IAuditable row whose UpdatedDate this call moves.
            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
        }
    }
}
