// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.CompareQueue
{
    public partial class CompareQueueOrchestrationServiceTests
    {
        [Fact]
        public async Task ShouldCompleteThePrimaryRecordAndMarkItProcessedAsync()
        {
            // given
            Guid inputFhirRecordId = Guid.NewGuid();

            this.fhirRecordServiceMock.Setup(service =>
                service.TryTransitionFhirRecordStatusAsync(
                    inputFhirRecordId,
                    StatusType.Completed,
                    StatusType.Completed,
                    true,
                    CancellationToken.None))
                        .ReturnsAsync(true);

            // when
            await this.compareQueueOrchestrationService
                .CompletePrimaryFhirRecordAsync(inputFhirRecordId);

            // then
            // Conditional on the row not already being Completed - the primary is shared by every
            // secondary of a correlation, so several workers reach this for the same row and a
            // read-then-write there is a check by one and a write by another.
            //
            // isProcessed: true matters just as much. The read-then-write path this replaced set
            // it for terminal statuses; dropping it left every completed primary marked
            // unprocessed, which anything reading IsProcessed reads as still pending.
            //
            // CancellationToken.None because the orchestration takes no token of its own and
            // leaves the foundation parameter at its default, which is the value Moq records.
            this.fhirRecordServiceMock.Verify(service =>
                service.TryTransitionFhirRecordStatusAsync(
                    inputFhirRecordId,
                    StatusType.Completed,
                    StatusType.Completed,
                    true,
                    CancellationToken.None),
                        Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldRecordThatCompletingThePrimaryChangedNoRowsAsync()
        {
            // given
            Guid inputFhirRecordId = Guid.NewGuid();

            this.fhirRecordServiceMock.Setup(service =>
                service.TryTransitionFhirRecordStatusAsync(
                    inputFhirRecordId,
                    StatusType.Completed,
                    StatusType.Completed,
                    true,
                    CancellationToken.None))
                        .ReturnsAsync(false);

            // when
            await this.compareQueueOrchestrationService
                .CompletePrimaryFhirRecordAsync(inputFhirRecordId);

            // then
            // Recorded rather than thrown or warned. Zero rows is the ordinary outcome when a
            // sibling secondary completed the shared primary first, so this must not read as a
            // fault - but it is also what a missing row looks like, and the count cannot tell
            // them apart, so the one case that would otherwise vanish leaves a line.
            this.loggingBrokerMock.Verify(broker =>
                broker.LogDebugAsync(It.Is<string>(message =>
                    message.Contains(inputFhirRecordId.ToString())
                    && message.Contains("changed no rows"))),
                        Times.Once);

            this.fhirRecordServiceMock.Verify(service =>
                service.TryTransitionFhirRecordStatusAsync(
                    inputFhirRecordId,
                    StatusType.Completed,
                    StatusType.Completed,
                    true,
                    CancellationToken.None),
                        Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldAdvanceTheLeaseTokenWhenTheClaimIsRetainedAsync()
        {
            // given
            DateTimeOffset claimedAt = GetRandomDateTimeOffset();
            DateTimeOffset retainedAt = claimedAt.AddMinutes(1);

            var inputCompareQueueItem = new CompareQueueItem
            {
                SecondaryFhirRecord = CreateRandomFhirRecord(),
                ClaimedAt = claimedAt
            };

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(retainedAt);

            this.fhirRecordServiceMock.Setup(service =>
                service.TryClaimFhirRecordAsync(
                    inputCompareQueueItem.SecondaryFhirRecord.Id,
                    StatusType.Processing,
                    StatusType.Processing,
                    retainedAt,
                    false,
                    claimedAt,
                    CancellationToken.None))
                        .ReturnsAsync(true);

            // when
            bool actualRetained = await this.compareQueueOrchestrationService
                .TryRetainClaimAsync(inputCompareQueueItem);

            // then
            // Re-asserting rewrites UpdatedDate, so the token the item carries has to move with
            // it. Left stale, a second re-assertion would match nothing and the worker would
            // conclude it had been overtaken while it still held the row.
            actualRetained.Should().BeTrue();
            inputCompareQueueItem.ClaimedAt.Should().Be(retainedAt);

            // isProcessed stays false: this is a lease renewal on a row still in flight, and
            // marking it processed here would report the comparison done while it is still
            // running. notUpdatedAfter is the PRE-call token - the value the item carried when
            // the statement ran, not the one it carries now.
            this.fhirRecordServiceMock.Verify(service =>
                service.TryClaimFhirRecordAsync(
                    inputCompareQueueItem.SecondaryFhirRecord.Id,
                    StatusType.Processing,
                    StatusType.Processing,
                    retainedAt,
                    false,
                    claimedAt,
                    CancellationToken.None),
                        Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldLeaveTheLeaseTokenAloneWhenTheClaimWasLostAsync()
        {
            // given
            DateTimeOffset claimedAt = GetRandomDateTimeOffset();
            DateTimeOffset attemptedAt = claimedAt.AddMinutes(1);

            var inputCompareQueueItem = new CompareQueueItem
            {
                SecondaryFhirRecord = CreateRandomFhirRecord(),
                ClaimedAt = claimedAt
            };

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(attemptedAt);

            this.fhirRecordServiceMock.Setup(service =>
                service.TryClaimFhirRecordAsync(
                    inputCompareQueueItem.SecondaryFhirRecord.Id,
                    StatusType.Processing,
                    StatusType.Processing,
                    attemptedAt,
                    false,
                    claimedAt,
                    CancellationToken.None))
                        .ReturnsAsync(false);

            // when
            bool actualRetained = await this.compareQueueOrchestrationService
                .TryRetainClaimAsync(inputCompareQueueItem);

            // then
            // A failed re-assertion wrote nothing, so the row still carries whatever the worker
            // that took it over put there - advancing the token here would invent a lease.
            actualRetained.Should().BeFalse();
            inputCompareQueueItem.ClaimedAt.Should().Be(claimedAt);

            // The attempt itself is still worth pinning down: the statement has to have gone out
            // carrying the UNCHANGED token, because that is what lets the database decide the
            // claim was lost rather than this test merely observing a false it was handed.
            this.fhirRecordServiceMock.Verify(service =>
                service.TryClaimFhirRecordAsync(
                    inputCompareQueueItem.SecondaryFhirRecord.Id,
                    StatusType.Processing,
                    StatusType.Processing,
                    attemptedAt,
                    false,
                    claimedAt,
                    CancellationToken.None),
                        Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
