// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue;
using Moq;
using Xunit;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.CompareQueue
{
    public partial class CompareQueueOrchestrationServiceTests
    {
        [Theory]
        [InlineData(StatusType.Completed)]
        [InlineData(StatusType.Failed)]
        public async Task ShouldSettleTheRecordUnderTheLeaseTokenAsync(StatusType terminalStatus)
        {
            // given
            DateTimeOffset claimedAt = GetRandomDateTimeOffset();
            DateTimeOffset settledAt = claimedAt.AddMinutes(2);

            var inputCompareQueueItem = new CompareQueueItem
            {
                SecondaryFhirRecord = CreateRandomFhirRecord(),
                ClaimedAt = claimedAt
            };

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(settledAt);

            this.fhirRecordServiceMock.Setup(service =>
                service.TryClaimFhirRecordAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<bool>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(true);

            // when
            bool actualSettled = await this.compareQueueOrchestrationService
                .TryFinalizeClaimedFhirRecordAsync(inputCompareQueueItem, terminalStatus);

            // then
            // The lease token goes INTO the statement. This replaced a read-then-write that ran
            // behind a separate ownership check, where the check could pass, the lease expire, and
            // the write then land a terminal status on a row another worker had reclaimed -
            // burying its comparison, because nothing reclaims Completed or Failed.
            actualSettled.Should().BeTrue();

            this.fhirRecordServiceMock.Verify(service =>
                service.TryClaimFhirRecordAsync(
                    inputCompareQueueItem.SecondaryFhirRecord.Id,
                    StatusType.Processing,
                    terminalStatus,
                    settledAt,
                    true,
                    claimedAt,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReportTheRecordUnsettledWhenTheLeaseWasLostAsync()
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
                    .ReturnsAsync(claimedAt.AddMinutes(2));

            this.fhirRecordServiceMock.Setup(service =>
                service.TryClaimFhirRecordAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<bool>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(false);

            // when
            bool actualSettled = await this.compareQueueOrchestrationService
                .TryFinalizeClaimedFhirRecordAsync(inputCompareQueueItem, StatusType.Failed);

            // then
            // Zero rows is the answer, not an error: the row has moved on under another worker,
            // and the caller needs to know it wrote nothing so it does not report an outcome for
            // work it no longer owns.
            actualSettled.Should().BeFalse();

            this.fhirRecordServiceMock.Verify(service =>
                service.TryClaimFhirRecordAsync(
                    inputCompareQueueItem.SecondaryFhirRecord.Id,
                    StatusType.Processing,
                    StatusType.Failed,
                    It.IsAny<DateTimeOffset>(),
                    true,
                    claimedAt,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldLeaveTheLeaseTokenAloneOnSettleAsync()
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
                    .ReturnsAsync(claimedAt.AddMinutes(2));

            this.fhirRecordServiceMock.Setup(service =>
                service.TryClaimFhirRecordAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<bool>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(true);

            // when
            await this.compareQueueOrchestrationService
                .TryFinalizeClaimedFhirRecordAsync(inputCompareQueueItem, StatusType.Completed);

            // then
            // Deliberately unlike TryRetainClaimAsync, which advances it. The row has left
            // Processing, so no later re-assertion against it can or should match, and moving the
            // token would only suggest a lease that no longer exists.
            inputCompareQueueItem.ClaimedAt.Should().Be(claimedAt);
        }
    }
}
