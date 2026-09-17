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
        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        [InlineData(7)]
        public async Task ShouldSpreadCandidateSelectionAcrossTheWindowAsync(int selectedIndex)
        {
            // given
            DateTimeOffset inputDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset bufferedDateTimeOffset = inputDateTimeOffset.AddMinutes(-5);
            var candidateFhirRecords = new List<FhirRecord>();

            for (int index = 0; index < 10; index++)
            {
                FhirRecord candidateFhirRecord =
                    CreateRandomFhirRecord(bufferedDateTimeOffset.AddMinutes(-(10 - index)));

                candidateFhirRecord.Status = StatusType.Pending;
                candidateFhirRecord.IsPrimarySource = false;
                candidateFhirRecords.Add(candidateFhirRecord);
            }

            FhirRecord expectedFhirRecord = candidateFhirRecords[selectedIndex];

            // The first byte of the identifier picks within the window, so a chosen byte pins a
            // chosen index - the point being that it is not always the head of the queue.
            var selectionBytes = new byte[16];
            selectionBytes[0] = (byte)selectedIndex;
            var selectionIdentifier = new Guid(selectionBytes);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(inputDateTimeOffset);

            this.identifierBrokerMock.Setup(broker =>
                broker.GetIdentifierAsync())
                    .ReturnsAsync(selectionIdentifier);

            this.fhirRecordServiceMock.Setup(service =>
                service.RetrieveAllFhirRecordsAsync())
                    .ReturnsAsync(candidateFhirRecords.AsQueryable());

            this.fhirRecordServiceMock.Setup(service =>
                service.TryClaimFhirRecordAsync(
                    expectedFhirRecord.Id,
                    StatusType.Pending,
                    StatusType.Processing,
                    inputDateTimeOffset,
                    false,
                    null))
                        .ReturnsAsync(true);

            this.fhirRecordServiceMock.Setup(service =>
                service.RetrieveFhirRecordByIdAsync(expectedFhirRecord.Id))
                    .ReturnsAsync(expectedFhirRecord);

            // when
            CompareQueueItem actualCompareQueueItem =
                await this.compareQueueOrchestrationService.GetUnprocessedRecordAsync();

            // then
            // Every worker runs the same ordered query, so taking the head made N workers converge
            // on the identical row every cycle and N-1 lose the claim by construction. Choosing
            // within a window of the oldest rows spreads them out and keeps the queue roughly
            // first-in-first-out.
            actualCompareQueueItem.SecondaryFhirRecord.Id.Should().Be(expectedFhirRecord.Id);

            // The clock is read once per claim attempt, and this claim wins on the first one.
            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            // A fresh identifier per attempt is what spreads the workers; here it is the byte
            // that pinned the chosen index.
            this.identifierBrokerMock.Verify(broker =>
                broker.GetIdentifierAsync(),
                    Times.Once);

            // Once for the candidate window, once for the winner's sibling primary.
            this.fhirRecordServiceMock.Verify(service =>
                service.RetrieveAllFhirRecordsAsync(),
                    Times.Exactly(2));

            this.fhirRecordServiceMock.Verify(service =>
                service.TryClaimFhirRecordAsync(
                    expectedFhirRecord.Id,
                    StatusType.Pending,
                    StatusType.Processing,
                    inputDateTimeOffset,
                    false,
                    null),
                        Times.Once);

            this.fhirRecordServiceMock.Verify(service =>
                service.RetrieveFhirRecordByIdAsync(expectedFhirRecord.Id),
                    Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

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

            // The exact value written to UpdatedDate by the claim. The caller keeps it as its
            // lease token, so a worker overtaken mid-flight can tell and abandon its result.
            expectedCompareQueueItem.ClaimedAt = inputDateTimeOffset;

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
                    inputDateTimeOffset,
                    false,
                    null))
                        .ReturnsAsync(true);

            // Read back after the claim won, which is why it reads Processing rather than the
            // Pending the candidate query saw.
            this.fhirRecordServiceMock.Setup(service =>
                service.RetrieveFhirRecordByIdAsync(inputSecondaryFhirRecord.Id))
                    .ReturnsAsync(storedSecondaryFhirRecord);

            // when
            CompareQueueItem actualCompareQueueItem =
                await this.compareQueueOrchestrationService.GetUnprocessedRecordAsync();

            // then
            actualCompareQueueItem.Should().BeEquivalentTo(expectedCompareQueueItem);

            // One read per claim attempt, and this claim wins on the first attempt.
            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            // The candidate is chosen from the window rather than taken from the head, so the
            // single attempt still costs one identifier.
            this.identifierBrokerMock.Verify(broker =>
                broker.GetIdentifierAsync(),
                    Times.Once);

            this.fhirRecordServiceMock.Verify(service =>
                service.RetrieveAllFhirRecordsAsync(),
                    Times.Exactly(2));

            // Claimed by the database rather than by a read-then-write, so two workers cannot
            // both take the same row. The claimed date is the same stamp the item carries back
            // as its lease token, and a claim is never a processed write.
            this.fhirRecordServiceMock.Verify(service =>
                service.TryClaimFhirRecordAsync(
                    inputSecondaryFhirRecord.Id,
                    StatusType.Pending,
                    StatusType.Processing,
                    inputDateTimeOffset,
                    false,
                    null),
                        Times.Once);

            // Only the winner's payload is fetched. The candidate query projects to the two
            // columns the claim needs, so a tick no longer drags ClaimCandidateWindow whole FHIR
            // bundles across the wire to choose one identifier.
            this.fhirRecordServiceMock.Verify(service =>
                service.RetrieveFhirRecordByIdAsync(inputSecondaryFhirRecord.Id),
                    Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
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
                    inputDateTimeOffset,
                    false,
                    null))
                        .ReturnsAsync(false);

            // when
            CompareQueueItem actualCompareQueueItem =
                await this.compareQueueOrchestrationService.GetUnprocessedRecordAsync();

            // then
            // Nothing is returned and, critically, the primary is never looked up - the losing
            // worker must not go on to compare a pair somebody else owns.
            actualCompareQueueItem.Should().BeNull();

            // The clock is read once per attempt rather than once per call, so a stamp taken up
            // front cannot go stale across the loop and shorten the lease it is written into.
            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Exactly(3));

            // A fresh identifier per attempt, so a worker that loses a race does not
            // deterministically collide with the same rival on the retry.
            this.identifierBrokerMock.Verify(broker =>
                broker.GetIdentifierAsync(),
                    Times.Exactly(3));

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
                    inputDateTimeOffset,
                    false,
                    null),
                        Times.Exactly(3));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogWarningAsync(It.Is<string>(message =>
                    message.Contains("Gave up claiming"))),
                        Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
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
                    inputDateTimeOffset,
                    false,
                    expectedLeaseExpiry))
                        .ReturnsAsync(true);

            this.fhirRecordServiceMock.Setup(service =>
                service.RetrieveFhirRecordByIdAsync(strandedFhirRecord.Id))
                    .ReturnsAsync(strandedFhirRecord);

            // when
            CompareQueueItem actualCompareQueueItem =
                await this.compareQueueOrchestrationService.GetUnprocessedRecordAsync();

            // then
            actualCompareQueueItem.Should().NotBeNull();
            actualCompareQueueItem.SecondaryFhirRecord.Id.Should().Be(strandedFhirRecord.Id);
            actualCompareQueueItem.PrimaryFhirRecord.Id.Should().Be(inputPrimaryFhirRecord.Id);

            // The clock is read once per claim attempt, and the reclaim wins on the first one.
            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.identifierBrokerMock.Verify(broker =>
                broker.GetIdentifierAsync(),
                    Times.Once);

            // Once for the candidate window, once for the reclaimed row's sibling primary.
            this.fhirRecordServiceMock.Verify(service =>
                service.RetrieveAllFhirRecordsAsync(),
                    Times.Exactly(2));

            // The lease bound has to reach the statement. This arm moves Processing to
            // Processing, so a status-only guard would match for every competing worker and the
            // claim would not arbitrate anything.
            this.fhirRecordServiceMock.Verify(service =>
                service.TryClaimFhirRecordAsync(
                    strandedFhirRecord.Id,
                    StatusType.Processing,
                    StatusType.Processing,
                    inputDateTimeOffset,
                    false,
                    expectedLeaseExpiry),
                        Times.Once);

            // The payload is fetched only after the reclaim is won, and only for the row this
            // worker now holds.
            this.fhirRecordServiceMock.Verify(service =>
                service.RetrieveFhirRecordByIdAsync(strandedFhirRecord.Id),
                    Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
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

            // An empty window short-circuits before the candidate is selected, so no identifier
            // is drawn and the loop does not burn its remaining attempts on an empty queue.
            this.identifierBrokerMock.Verify(broker =>
                broker.GetIdentifierAsync(),
                    Times.Never);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
