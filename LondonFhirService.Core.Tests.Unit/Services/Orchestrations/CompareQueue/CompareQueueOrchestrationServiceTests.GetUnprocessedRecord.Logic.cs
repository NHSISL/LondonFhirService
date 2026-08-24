// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue;
using Moq;
using Xunit;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.CompareQueue
{
    public partial class CompareQueueOrchestrationServiceTests
    {
        [Fact]
        public async Task ShouldGetUnprocessedRecordAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset inputDateTimeOffset = randomDateTimeOffset;
            DateTimeOffset bufferedDateTimeOffset = inputDateTimeOffset.AddMinutes(-5);

            FhirRecord randomSecondaryFhirRecord =
                CreateRandomFhirRecord(bufferedDateTimeOffset.AddMinutes(-1));

            randomSecondaryFhirRecord.Status = StatusType.Pending;
            randomSecondaryFhirRecord.IsPrimarySource = false;
            FhirRecord inputSecondaryFhirRecord = randomSecondaryFhirRecord;

            FhirRecord storedSecondaryFhirRecord = inputSecondaryFhirRecord.DeepClone();
            storedSecondaryFhirRecord.Status = StatusType.Processing;

            FhirRecord randomPrimaryFhirRecord = CreateRandomFhirRecord(inputDateTimeOffset);
            randomPrimaryFhirRecord.IsPrimarySource = true;
            randomPrimaryFhirRecord.CorrelationId = storedSecondaryFhirRecord.CorrelationId;
            FhirRecord inputPrimaryFhirRecord = randomPrimaryFhirRecord;

            IQueryable<FhirRecord> secondaryFhirRecords =
                new List<FhirRecord> { inputSecondaryFhirRecord }.AsQueryable();

            IQueryable<FhirRecord> primaryFhirRecords =
                new List<FhirRecord> { inputPrimaryFhirRecord }.AsQueryable();

            var expectedCompareQueueItem = new CompareQueueItem();
            expectedCompareQueueItem.PrimaryFhirRecord = inputPrimaryFhirRecord;
            expectedCompareQueueItem.SecondaryFhirRecord = storedSecondaryFhirRecord;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(inputDateTimeOffset);

            this.fhirRecordServiceMock.SetupSequence(service =>
                service.RetrieveAllFhirRecordsAsync())
                    .ReturnsAsync(secondaryFhirRecords)
                    .ReturnsAsync(primaryFhirRecords);

            // A Pending claim needs no lease bound - the status change alone is a real guard.
            this.fhirRecordServiceMock.Setup(service =>
                service.TryClaimFhirRecordAsync(
                    inputSecondaryFhirRecord.Id,
                    StatusType.Pending,
                    StatusType.Processing,
                    null))
                        .ReturnsAsync(true);

            // when
            CompareQueueItem actualCompareQueueItem =
                await this.compareQueueOrchestrationService.GetUnprocessedRecordAsync();

            // then
            actualCompareQueueItem.Should().BeEquivalentTo(expectedCompareQueueItem);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.fhirRecordServiceMock.Verify(service =>
                service.RetrieveAllFhirRecordsAsync(),
                    Times.Exactly(2));

            // Claimed by the database rather than by a read-then-write, so two workers cannot
            // both take the same row.
            this.fhirRecordServiceMock.Verify(service =>
                service.TryClaimFhirRecordAsync(
                    inputSecondaryFhirRecord.Id,
                    StatusType.Pending,
                    StatusType.Processing,
                    null),
                        Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldRetryOtherCandidatesWhenTheClaimIsLostToAnotherWorkerAsync()
        {
            // given
            DateTimeOffset inputDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset bufferedDateTimeOffset = inputDateTimeOffset.AddMinutes(-5);

            FhirRecord inputSecondaryFhirRecord =
                CreateRandomFhirRecord(bufferedDateTimeOffset.AddMinutes(-1));

            inputSecondaryFhirRecord.Status = StatusType.Pending;
            inputSecondaryFhirRecord.IsPrimarySource = false;

            IQueryable<FhirRecord> secondaryFhirRecords =
                new List<FhirRecord> { inputSecondaryFhirRecord }.AsQueryable();

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(inputDateTimeOffset);

            this.fhirRecordServiceMock.Setup(service =>
                service.RetrieveAllFhirRecordsAsync())
                    .ReturnsAsync(secondaryFhirRecords);

            // Another worker wins every attempt, so the guarded update changes no rows.
            this.fhirRecordServiceMock.Setup(service =>
                service.TryClaimFhirRecordAsync(
                    inputSecondaryFhirRecord.Id,
                    StatusType.Pending,
                    StatusType.Processing,
                    null))
                        .ReturnsAsync(false);

            // when
            CompareQueueItem actualCompareQueueItem =
                await this.compareQueueOrchestrationService.GetUnprocessedRecordAsync();

            // then
            // Nothing is returned and, critically, the primary is never looked up - the losing
            // worker must not go on to compare a pair somebody else owns.
            actualCompareQueueItem.Should().BeNull();

            // Retried rather than giving up on the first loss: a lost claim means another worker
            // took that row, not that the queue is empty.
            this.fhirRecordServiceMock.Verify(service =>
                service.RetrieveAllFhirRecordsAsync(),
                    Times.Exactly(3));

            this.fhirRecordServiceMock.Verify(service =>
                service.TryClaimFhirRecordAsync(
                    inputSecondaryFhirRecord.Id,
                    StatusType.Pending,
                    StatusType.Processing,
                    null),
                        Times.Exactly(3));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogWarningAsync(It.Is<string>(message =>
                    message.Contains("Gave up claiming"))),
                        Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReclaimARecordStrandedInProcessingPastItsLeaseAsync()
        {
            // given
            // Processing used to be a write-only state: a process recycle between the claim and
            // the terminal status left the row invisible forever.
            DateTimeOffset inputDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset leaseExpiredDateTimeOffset = inputDateTimeOffset.AddMinutes(-31);

            FhirRecord strandedFhirRecord = CreateRandomFhirRecord(leaseExpiredDateTimeOffset);
            strandedFhirRecord.Status = StatusType.Processing;
            strandedFhirRecord.IsPrimarySource = false;

            FhirRecord inputPrimaryFhirRecord = CreateRandomFhirRecord(inputDateTimeOffset);
            inputPrimaryFhirRecord.IsPrimarySource = true;
            inputPrimaryFhirRecord.CorrelationId = strandedFhirRecord.CorrelationId;

            IQueryable<FhirRecord> secondaryFhirRecords =
                new List<FhirRecord> { strandedFhirRecord }.AsQueryable();

            IQueryable<FhirRecord> primaryFhirRecords =
                new List<FhirRecord> { inputPrimaryFhirRecord }.AsQueryable();

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(inputDateTimeOffset);

            this.fhirRecordServiceMock.SetupSequence(service =>
                service.RetrieveAllFhirRecordsAsync())
                    .ReturnsAsync(secondaryFhirRecords)
                    .ReturnsAsync(primaryFhirRecords);

            DateTimeOffset expectedLeaseExpiry = inputDateTimeOffset.AddMinutes(-30);

            this.fhirRecordServiceMock.Setup(service =>
                service.TryClaimFhirRecordAsync(
                    strandedFhirRecord.Id,
                    StatusType.Processing,
                    StatusType.Processing,
                    expectedLeaseExpiry))
                        .ReturnsAsync(true);

            // when
            CompareQueueItem actualCompareQueueItem =
                await this.compareQueueOrchestrationService.GetUnprocessedRecordAsync();

            // then
            actualCompareQueueItem.Should().NotBeNull();
            actualCompareQueueItem.SecondaryFhirRecord.Id.Should().Be(strandedFhirRecord.Id);
            actualCompareQueueItem.PrimaryFhirRecord.Id.Should().Be(inputPrimaryFhirRecord.Id);

            // The lease bound has to reach the statement. This arm moves Processing to
            // Processing, so a status-only guard would match for every competing worker and the
            // claim would not arbitrate anything.
            this.fhirRecordServiceMock.Verify(service =>
                service.TryClaimFhirRecordAsync(
                    strandedFhirRecord.Id,
                    StatusType.Processing,
                    StatusType.Processing,
                    expectedLeaseExpiry),
                        Times.Once);
        }

        [Fact]
        public async Task ShouldReturnNullWhenNoUnprocessedRecordsExistAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset inputDateTimeOffset = randomDateTimeOffset;

            IQueryable<FhirRecord> emptyFhirRecords =
                new List<FhirRecord>().AsQueryable();

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(inputDateTimeOffset);

            this.fhirRecordServiceMock.Setup(service =>
                service.RetrieveAllFhirRecordsAsync())
                    .ReturnsAsync(emptyFhirRecords);

            // when
            CompareQueueItem actualCompareQueueItem =
                await this.compareQueueOrchestrationService.GetUnprocessedRecordAsync();

            // then
            actualCompareQueueItem.Should().BeNull();

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.fhirRecordServiceMock.Verify(service =>
                service.RetrieveAllFhirRecordsAsync(),
                    Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
