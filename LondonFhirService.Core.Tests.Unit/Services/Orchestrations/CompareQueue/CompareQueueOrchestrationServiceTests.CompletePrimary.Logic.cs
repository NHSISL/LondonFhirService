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
                    It.IsAny<Guid>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
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
            this.fhirRecordServiceMock.Verify(service =>
                service.TryTransitionFhirRecordStatusAsync(
                    inputFhirRecordId,
                    StatusType.Completed,
                    StatusType.Completed,
                    true,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
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
                    It.IsAny<Guid>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()))
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

            this.fhirRecordServiceMock.Verify(service =>
                service.TryClaimFhirRecordAsync(
                    inputCompareQueueItem.SecondaryFhirRecord.Id,
                    StatusType.Processing,
                    StatusType.Processing,
                    retainedAt,
                    claimedAt,
                    It.IsAny<CancellationToken>()),
                        Times.Once);
        }

        [Fact]
        public async Task ShouldLeaveTheLeaseTokenAloneWhenTheClaimWasLostAsync()
        {
            // given
            DateTimeOffset claimedAt = GetRandomDateTimeOffset();

            var inputCompareQueueItem = new CompareQueueItem
            {
                SecondaryFhirRecord = CreateRandomFhirRecord(),
                ClaimedAt = claimedAt
            };

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(claimedAt.AddMinutes(1));

            this.fhirRecordServiceMock.Setup(service =>
                service.TryClaimFhirRecordAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(false);

            // when
            bool actualRetained = await this.compareQueueOrchestrationService
                .TryRetainClaimAsync(inputCompareQueueItem);

            // then
            // A failed re-assertion wrote nothing, so the row still carries whatever the worker
            // that took it over put there - advancing the token here would invent a lease.
            actualRetained.Should().BeFalse();
            inputCompareQueueItem.ClaimedAt.Should().Be(claimedAt);
        }
    }
}
