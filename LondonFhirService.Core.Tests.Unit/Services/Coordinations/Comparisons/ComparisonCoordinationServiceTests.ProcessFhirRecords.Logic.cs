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

            this.compareQueueOrchestrationServiceMock.Setup(service =>
                service.TryRetainClaimAsync(It.IsAny<CompareQueueItem>()))
                    .ReturnsAsync(false);

            // when
            await this.comparisonCoordinationService.ProcessFhirRecordsAsync();

            // then
            // Failed is terminal and nothing reclaims it, so an overtaken worker must not write
            // it - that would bury a record the worker holding the row may yet complete, and
            // GetUnprocessedRecordAsync would never offer it again.
            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.ChangeFhirRecordStatusAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<StatusType>()),
                        Times.Never);

            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.CompletePrimaryFhirRecordAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogWarningAsync(It.Is<string>(message =>
                    message.Contains("Not marking"))),
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
                service.ChangeFhirRecordStatusAsync(
                    It.IsAny<Guid>(),
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
                service.ChangeFhirRecordStatusAsync(
                    inputSecondaryFhirRecord.Id,
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
                service.ChangeFhirRecordStatusAsync(
                    inputSecondaryFhirRecord.Id,
                    StatusType.Failed),
                        Times.Once);

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
                service.ChangeFhirRecordStatusAsync(
                    inputSecondaryFhirRecord.Id,
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
