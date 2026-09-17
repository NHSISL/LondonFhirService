// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue;
using LondonFhirService.Core.Models.Orchestrations.Comparisons;
using Moq;
using Xunit;

namespace LondonFhirService.Core.Tests.Unit.Services.Coordinations.Comparisons
{
    public partial class ComparisonCoordinationServiceTests
    {
        [Fact]
        public async Task ShouldNotMarkFailedWhenTheClaimWasLostBeforeTheFailureAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            FhirRecord inputPrimaryFhirRecord = CreateRandomFhirRecord();
            inputPrimaryFhirRecord.IsPrimarySource = true;

            FhirRecord inputSecondaryFhirRecord = CreateRandomFhirRecord();
            inputSecondaryFhirRecord.IsPrimarySource = false;
            inputSecondaryFhirRecord.CorrelationId = inputPrimaryFhirRecord.CorrelationId;

            var inputCompareQueueItem = new CompareQueueItem
            {
                PrimaryFhirRecord = inputPrimaryFhirRecord,
                SecondaryFhirRecord = inputSecondaryFhirRecord,
                ClaimedAt = randomDateTimeOffset
            };

            this.compareQueueOrchestrationServiceMock.SetupSequence(service =>
                service.GetUnprocessedRecordAsync())
                    .ReturnsAsync(inputCompareQueueItem)
                    .ReturnsAsync((CompareQueueItem)null);

            // The comparison itself blows up, sending the drain loop down the catch arm.
            this.comparisonOrchestrationServiceMock.Setup(service =>
                service.CompareAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                        .ThrowsAsync(new Exception(GetRandomString()));

            // The settle itself reports the lost lease; there is no separate check to stub.
            this.compareQueueOrchestrationServiceMock.Setup(service =>
                service.TryFinalizeClaimedFhirRecordAsync(
                    It.IsAny<CompareQueueItem>(),
                    It.IsAny<StatusType>()))
                        .ReturnsAsync(false);

            // when
            await this.comparisonCoordinationService.ProcessFhirRecordsAsync();

            // then
            // Failed is terminal and nothing reclaims it, so an overtaken worker must not write
            // it - that would bury a record the worker holding the row may yet complete, and
            // GetUnprocessedRecordAsync would never offer it again. The write is now guarded by
            // its own statement, so nothing lands even if the lease expires after the decision
            // to fail was taken.
            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.TryFinalizeClaimedFhirRecordAsync(
                    inputCompareQueueItem,
                    StatusType.Failed),
                        Times.Once);

            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.CompletePrimaryFhirRecordAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogWarningAsync(It.Is<string>(message =>
                    message.Contains("matched no rows"))),
                        Times.Once);
        }

        [Fact]
        public async Task ShouldAbandonTheComparisonWhenTheClaimWasLostAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            FhirRecord inputPrimaryFhirRecord = CreateRandomFhirRecord();
            inputPrimaryFhirRecord.IsPrimarySource = true;

            FhirRecord inputSecondaryFhirRecord = CreateRandomFhirRecord();
            inputSecondaryFhirRecord.IsPrimarySource = false;
            inputSecondaryFhirRecord.CorrelationId = inputPrimaryFhirRecord.CorrelationId;

            var inputCompareQueueItem = new CompareQueueItem
            {
                PrimaryFhirRecord = inputPrimaryFhirRecord,
                SecondaryFhirRecord = inputSecondaryFhirRecord,
                ClaimedAt = randomDateTimeOffset
            };

            this.compareQueueOrchestrationServiceMock.SetupSequence(service =>
                service.GetUnprocessedRecordAsync())
                    .ReturnsAsync(inputCompareQueueItem)
                    .ReturnsAsync((CompareQueueItem)null);

            this.comparisonOrchestrationServiceMock.Setup(service =>
                service.CompareAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                        .ReturnsAsync(new ComparisonResult
                        {
                            CorrelationId = inputPrimaryFhirRecord.CorrelationId
                        });

            this.identifierBrokerMock.Setup(broker =>
                broker.GetIdentifierAsync())
                    .ReturnsAsync(Guid.NewGuid());

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // The lease expired mid-flight and another worker took the record over.
            this.compareQueueOrchestrationServiceMock.Setup(service =>
                service.TryRetainClaimAsync(It.IsAny<CompareQueueItem>()))
                    .ReturnsAsync(false);

            // when
            await this.comparisonCoordinationService.ProcessFhirRecordsAsync();

            // then
            // Nothing is written. The worker that took the record over is performing the same
            // comparison, and two difference rows for one pair is worse than one produced late.
            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.PersistFhirRecordDifferencesAsync(It.IsAny<CompareQueueItem>()),
                    Times.Never);

            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.TryFinalizeClaimedFhirRecordAsync(
                    It.IsAny<CompareQueueItem>(),
                    It.IsAny<StatusType>()),
                        Times.Never);

            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.CompletePrimaryFhirRecordAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogWarningAsync(It.Is<string>(message =>
                    message.Contains("Abandoning comparison"))),
                        Times.Once);
        }

        [Fact]
        public async Task ShouldNotCompleteThePrimaryWhenTheSettleLostTheLeaseAsync()
        {
            // given
            CompareQueueItem randomCompareQueueItem = CreateRandomCompareQueueItem();
            CompareQueueItem inputCompareQueueItem = randomCompareQueueItem;
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();

            this.compareQueueOrchestrationServiceMock.SetupSequence(service =>
                service.GetUnprocessedRecordAsync())
                    .ReturnsAsync(inputCompareQueueItem)
                    .ReturnsAsync((CompareQueueItem)null);

            this.comparisonOrchestrationServiceMock.Setup(service =>
                service.CompareAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                        .ReturnsAsync(new ComparisonResult
                        {
                            CorrelationId = inputCompareQueueItem.PrimaryFhirRecord.CorrelationId
                        });

            this.identifierBrokerMock.Setup(broker =>
                broker.GetIdentifierAsync())
                    .ReturnsAsync(Guid.NewGuid());

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.compareQueueOrchestrationServiceMock.Setup(service =>
                service.TryRetainClaimAsync(It.IsAny<CompareQueueItem>()))
                    .ReturnsAsync(true);

            // The lease held long enough to persist the difference, then went.
            this.compareQueueOrchestrationServiceMock.Setup(service =>
                service.TryFinalizeClaimedFhirRecordAsync(
                    It.IsAny<CompareQueueItem>(),
                    It.IsAny<StatusType>()))
                        .ReturnsAsync(false);

            // when
            await this.comparisonCoordinationService.ProcessFhirRecordsAsync();

            // then
            // Same gate the failure path applies. A worker that no longer holds the record should
            // not keep writing on its behalf, and nothing is lost by stopping - the worker that
            // took it over completes the primary when it finishes.
            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.CompletePrimaryFhirRecordAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogWarningAsync(It.Is<string>(message =>
                    message.Contains("matched no rows"))),
                        Times.Once);
        }

        [Fact]
        public async Task ShouldProcessFhirRecordsAsync()
        {
            // given
            CompareQueueItem randomCompareQueueItem = CreateRandomCompareQueueItem();
            CompareQueueItem inputCompareQueueItem = randomCompareQueueItem;
            FhirRecord inputPrimaryFhirRecord = inputCompareQueueItem.PrimaryFhirRecord;
            FhirRecord inputSecondaryFhirRecord = inputCompareQueueItem.SecondaryFhirRecord;
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Guid randomFhirRecordDifferenceId = Guid.NewGuid();

            ComparisonResult randomComparisonResult =
                CreateRandomComparisonResult(inputPrimaryFhirRecord.CorrelationId);

            string expectedDiffJson = SerializeComparisonResult(randomComparisonResult);

            var expectedFhirRecordDifference = new FhirRecordDifference
            {
                Id = randomFhirRecordDifferenceId,
                PrimaryId = inputPrimaryFhirRecord.Id,
                SecondaryId = inputSecondaryFhirRecord.Id,
                CorrelationId = randomComparisonResult.CorrelationId,
                DiffJson = expectedDiffJson,
                DiffCount = randomComparisonResult.DiffCount,
                ComparedAt = randomDateTimeOffset
            };

            this.compareQueueOrchestrationServiceMock.SetupSequence(service =>
                service.GetUnprocessedRecordAsync())
                    .ReturnsAsync(inputCompareQueueItem)
                    .ReturnsAsync((CompareQueueItem)null);

            this.comparisonOrchestrationServiceMock.Setup(service =>
                service.CompareAsync(
                    correlationId: inputPrimaryFhirRecord.CorrelationId,
                    source1Json: inputPrimaryFhirRecord.JsonPayload,
                    source2Json: inputSecondaryFhirRecord.JsonPayload))
                        .ReturnsAsync(randomComparisonResult);

            this.identifierBrokerMock.Setup(broker =>
                broker.GetIdentifierAsync())
                    .ReturnsAsync(randomFhirRecordDifferenceId);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.compareQueueOrchestrationServiceMock.Setup(service =>
                service.TryFinalizeClaimedFhirRecordAsync(
                    It.IsAny<CompareQueueItem>(),
                    It.IsAny<StatusType>()))
                        .ReturnsAsync(true);

            this.compareQueueOrchestrationServiceMock.Setup(service =>
                service.TryRetainClaimAsync(It.IsAny<CompareQueueItem>()))
                    .ReturnsAsync(true);

            // when
            await this.comparisonCoordinationService.ProcessFhirRecordsAsync();

            // then
            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.GetUnprocessedRecordAsync(),
                    Times.Exactly(2));

            this.comparisonOrchestrationServiceMock.Verify(service =>
                service.CompareAsync(
                    correlationId: inputPrimaryFhirRecord.CorrelationId,
                    source1Json: inputPrimaryFhirRecord.JsonPayload,
                    source2Json: inputSecondaryFhirRecord.JsonPayload),
                        Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.identifierBrokerMock.Verify(broker =>
                broker.GetIdentifierAsync(),
                    Times.Once);

            // Re-asserted before anything is written, so a worker whose lease expired mid-flight
            // discards its result rather than adding a second difference row for the same pair.
            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.TryRetainClaimAsync(It.IsAny<CompareQueueItem>()),
                    Times.Once);

            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.PersistFhirRecordDifferencesAsync(
                    It.Is<CompareQueueItem>(item =>
                        item.FhirRecordDifference.Id == expectedFhirRecordDifference.Id
                        && item.FhirRecordDifference.PrimaryId == expectedFhirRecordDifference.PrimaryId
                        && item.FhirRecordDifference.SecondaryId == expectedFhirRecordDifference.SecondaryId
                        && item.FhirRecordDifference.CorrelationId == expectedFhirRecordDifference.CorrelationId
                        && item.FhirRecordDifference.DiffJson == expectedFhirRecordDifference.DiffJson
                        && item.FhirRecordDifference.DiffCount == expectedFhirRecordDifference.DiffCount
                        && item.FhirRecordDifference.ComparedAt == expectedFhirRecordDifference.ComparedAt)),
                            Times.Once);

            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.TryFinalizeClaimedFhirRecordAsync(
                    inputCompareQueueItem,
                    StatusType.Completed),
                        Times.Once);

            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.CompletePrimaryFhirRecordAsync(inputPrimaryFhirRecord.Id),
                    Times.Once);

            this.compareQueueOrchestrationServiceMock.VerifyNoOtherCalls();
            this.comparisonOrchestrationServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldMarkSecondaryAsFailedWhenPrimaryFhirRecordIsNullAsync()
        {
            // given
            CompareQueueItem randomCompareQueueItem = CreateRandomCompareQueueItem();
            CompareQueueItem inputCompareQueueItem = randomCompareQueueItem;
            inputCompareQueueItem.PrimaryFhirRecord = null;
            FhirRecord inputSecondaryFhirRecord = inputCompareQueueItem.SecondaryFhirRecord;

            string expectedWarningMessage =
                $"CompareQueueItem with CorrelationId: " +
                $"{inputSecondaryFhirRecord.CorrelationId} does not have " +
                $"a primary record. Marking as failed without comparison.";

            this.compareQueueOrchestrationServiceMock.SetupSequence(service =>
                service.GetUnprocessedRecordAsync())
                    .ReturnsAsync(inputCompareQueueItem)
                    .ReturnsAsync((CompareQueueItem)null);

            this.compareQueueOrchestrationServiceMock.Setup(service =>
                service.TryFinalizeClaimedFhirRecordAsync(
                    inputCompareQueueItem,
                    StatusType.Failed))
                        .ReturnsAsync(true);

            // when
            await this.comparisonCoordinationService.ProcessFhirRecordsAsync();

            // then
            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.GetUnprocessedRecordAsync(),
                    Times.Exactly(2));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogWarningAsync(expectedWarningMessage),
                    Times.Once);

            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.TryFinalizeClaimedFhirRecordAsync(
                    inputCompareQueueItem,
                    StatusType.Failed),
                        Times.Once);

            this.compareQueueOrchestrationServiceMock.VerifyNoOtherCalls();
            this.comparisonOrchestrationServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldNotMarkFailedForAMissingPrimaryWhenTheClaimWasLostAsync()
        {
            // given
            CompareQueueItem randomCompareQueueItem = CreateRandomCompareQueueItem();
            CompareQueueItem inputCompareQueueItem = randomCompareQueueItem;
            inputCompareQueueItem.PrimaryFhirRecord = null;
            FhirRecord inputSecondaryFhirRecord = inputCompareQueueItem.SecondaryFhirRecord;

            // Reports what the statement did rather than why. Zero rows means the record is no
            // longer Processing under this worker's token; the statement cannot tell a lost lease
            // apart from a row this worker already settled, so the message does not claim to.
            string expectedWarningMessage =
                $"Settling CorrelationId: {inputSecondaryFhirRecord.CorrelationId} as " +
                $"{StatusType.Failed} matched no rows - the record is no longer Processing under " +
                "this worker's lease, so nothing was written. Whichever worker holds it now " +
                "reports its own outcome.";

            this.compareQueueOrchestrationServiceMock.SetupSequence(service =>
                service.GetUnprocessedRecordAsync())
                    .ReturnsAsync(inputCompareQueueItem)
                    .ReturnsAsync((CompareQueueItem)null);

            this.compareQueueOrchestrationServiceMock.Setup(service =>
                service.TryFinalizeClaimedFhirRecordAsync(
                    inputCompareQueueItem,
                    StatusType.Failed))
                        .ReturnsAsync(false);

            // when
            await this.comparisonCoordinationService.ProcessFhirRecordsAsync();

            // then
            // The primary is read after the claim, so a slow read leaves room for the lease to
            // expire and the row to be taken over. Marking it Failed here would be terminal for a
            // record the new holder may be about to complete with a primary this worker simply
            // read too early - and nothing reclaims Failed. The ownership test rides in the
            // statement, so the write is attempted and simply matches nothing.
            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.TryFinalizeClaimedFhirRecordAsync(
                    inputCompareQueueItem,
                    StatusType.Failed),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogWarningAsync(expectedWarningMessage),
                    Times.Once);

            // Logged on the way in, before the settle is attempted - the record genuinely has no
            // primary whether or not this worker still holds it, so the observation stands even
            // when the write that follows matches nothing.
            this.loggingBrokerMock.Verify(broker =>
                broker.LogWarningAsync(
                    $"CompareQueueItem with CorrelationId: " +
                    $"{inputSecondaryFhirRecord.CorrelationId} does not have " +
                    $"a primary record. Marking as failed without comparison."),
                        Times.Once);

            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.GetUnprocessedRecordAsync(),
                    Times.Exactly(2));

            this.compareQueueOrchestrationServiceMock.VerifyNoOtherCalls();
            this.comparisonOrchestrationServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldNotUpdatePrimaryRecordStatusWhenAlreadyCompletedAsync()
        {
            // given
            CompareQueueItem randomCompareQueueItem = CreateRandomCompareQueueItem();
            CompareQueueItem inputCompareQueueItem = randomCompareQueueItem;
            inputCompareQueueItem.PrimaryFhirRecord.Status = StatusType.Completed;
            FhirRecord inputPrimaryFhirRecord = inputCompareQueueItem.PrimaryFhirRecord;
            FhirRecord inputSecondaryFhirRecord = inputCompareQueueItem.SecondaryFhirRecord;
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Guid randomFhirRecordDifferenceId = Guid.NewGuid();

            ComparisonResult randomComparisonResult =
                CreateRandomComparisonResult(inputPrimaryFhirRecord.CorrelationId);

            string expectedDiffJson = SerializeComparisonResult(randomComparisonResult);

            var expectedFhirRecordDifference = new FhirRecordDifference
            {
                Id = randomFhirRecordDifferenceId,
                PrimaryId = inputPrimaryFhirRecord.Id,
                SecondaryId = inputSecondaryFhirRecord.Id,
                CorrelationId = randomComparisonResult.CorrelationId,
                DiffJson = expectedDiffJson,
                DiffCount = randomComparisonResult.DiffCount,
                ComparedAt = randomDateTimeOffset
            };

            this.compareQueueOrchestrationServiceMock.SetupSequence(service =>
                service.GetUnprocessedRecordAsync())
                    .ReturnsAsync(inputCompareQueueItem)
                    .ReturnsAsync((CompareQueueItem)null);

            this.comparisonOrchestrationServiceMock.Setup(service =>
                service.CompareAsync(
                    correlationId: inputPrimaryFhirRecord.CorrelationId,
                    source1Json: inputPrimaryFhirRecord.JsonPayload,
                    source2Json: inputSecondaryFhirRecord.JsonPayload))
                        .ReturnsAsync(randomComparisonResult);

            this.identifierBrokerMock.Setup(broker =>
                broker.GetIdentifierAsync())
                    .ReturnsAsync(randomFhirRecordDifferenceId);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.compareQueueOrchestrationServiceMock.Setup(service =>
                service.TryFinalizeClaimedFhirRecordAsync(
                    It.IsAny<CompareQueueItem>(),
                    It.IsAny<StatusType>()))
                        .ReturnsAsync(true);

            this.compareQueueOrchestrationServiceMock.Setup(service =>
                service.TryRetainClaimAsync(It.IsAny<CompareQueueItem>()))
                    .ReturnsAsync(true);

            // when
            await this.comparisonCoordinationService.ProcessFhirRecordsAsync();

            // then
            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.GetUnprocessedRecordAsync(),
                    Times.Exactly(2));

            this.comparisonOrchestrationServiceMock.Verify(service =>
                service.CompareAsync(
                    correlationId: inputPrimaryFhirRecord.CorrelationId,
                    source1Json: inputPrimaryFhirRecord.JsonPayload,
                    source2Json: inputSecondaryFhirRecord.JsonPayload),
                        Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.identifierBrokerMock.Verify(broker =>
                broker.GetIdentifierAsync(),
                    Times.Once);

            // Re-asserted before anything is written, so a worker whose lease expired mid-flight
            // discards its result rather than adding a second difference row for the same pair.
            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.TryRetainClaimAsync(It.IsAny<CompareQueueItem>()),
                    Times.Once);

            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.PersistFhirRecordDifferencesAsync(
                    It.Is<CompareQueueItem>(item =>
                        item.FhirRecordDifference.Id == expectedFhirRecordDifference.Id
                        && item.FhirRecordDifference.PrimaryId == expectedFhirRecordDifference.PrimaryId
                        && item.FhirRecordDifference.SecondaryId == expectedFhirRecordDifference.SecondaryId
                        && item.FhirRecordDifference.CorrelationId == expectedFhirRecordDifference.CorrelationId
                        && item.FhirRecordDifference.DiffJson == expectedFhirRecordDifference.DiffJson
                        && item.FhirRecordDifference.DiffCount == expectedFhirRecordDifference.DiffCount
                        && item.FhirRecordDifference.ComparedAt == expectedFhirRecordDifference.ComparedAt)),
                            Times.Once);

            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.TryFinalizeClaimedFhirRecordAsync(
                    inputCompareQueueItem,
                    StatusType.Completed),
                        Times.Once);

            // Called even though the primary was already Completed, because the "already?" test
            // now lives in the database statement. The primary is shared by every secondary of a
            // correlation, so checking here and writing there let two workers race; the conditional
            // update makes the second one a no-op instead.
            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.CompletePrimaryFhirRecordAsync(inputPrimaryFhirRecord.Id),
                    Times.Once);

            this.compareQueueOrchestrationServiceMock.VerifyNoOtherCalls();
            this.comparisonOrchestrationServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
