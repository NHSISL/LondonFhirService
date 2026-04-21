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
        public async Task ShouldProcessFhirRecordsAsync()
        {
            // given
            CompareQueueItem randomCompareQueueItem = CreateRandomCompareQueueItem();
            CompareQueueItem inputCompareQueueItem = randomCompareQueueItem;
            FhirRecord inputPrimaryFhirRecord = inputCompareQueueItem.PrimaryFhirRecord;
            FhirRecord inputSecondaryFhirRecord = inputCompareQueueItem.SecondaryFhirRecord;
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();

            ComparisonResult randomComparisonResult =
                CreateRandomComparisonResult(inputPrimaryFhirRecord.CorrelationId);

            string expectedDiffJson = SerializeComparisonResult(randomComparisonResult);

            var expectedFhirRecordDifference = new FhirRecordDifference
            {
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
                    inputPrimaryFhirRecord.CorrelationId,
                    inputPrimaryFhirRecord.JsonPayload,
                    inputSecondaryFhirRecord.JsonPayload))
                        .ReturnsAsync(randomComparisonResult);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            await this.comparisonCoordinationService.ProcessFhirRecords();

            // then
            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.GetUnprocessedRecordAsync(),
                    Times.Exactly(2));

            this.comparisonOrchestrationServiceMock.Verify(service =>
                service.CompareAsync(
                    inputPrimaryFhirRecord.CorrelationId,
                    inputPrimaryFhirRecord.JsonPayload,
                    inputSecondaryFhirRecord.JsonPayload),
                        Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.PersistFhirRecordDifferencesAsync(
                    It.Is<CompareQueueItem>(item =>
                        item.FhirRecordDifference.PrimaryId == expectedFhirRecordDifference.PrimaryId
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
                service.ChangeFhirRecordStatusAsync(
                    inputPrimaryFhirRecord.Id,
                    StatusType.Completed),
                        Times.Once);

            this.compareQueueOrchestrationServiceMock.VerifyNoOtherCalls();
            this.comparisonOrchestrationServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
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
                $"a primary record. Marking as completed without comparison.";

            this.compareQueueOrchestrationServiceMock.SetupSequence(service =>
                service.GetUnprocessedRecordAsync())
                    .ReturnsAsync(inputCompareQueueItem)
                    .ReturnsAsync((CompareQueueItem)null);

            // when
            await this.comparisonCoordinationService.ProcessFhirRecords();

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

            ComparisonResult randomComparisonResult =
                CreateRandomComparisonResult(inputPrimaryFhirRecord.CorrelationId);

            this.compareQueueOrchestrationServiceMock.SetupSequence(service =>
                service.GetUnprocessedRecordAsync())
                    .ReturnsAsync(inputCompareQueueItem)
                    .ReturnsAsync((CompareQueueItem)null);

            this.comparisonOrchestrationServiceMock.Setup(service =>
                service.CompareAsync(
                    inputPrimaryFhirRecord.CorrelationId,
                    inputPrimaryFhirRecord.JsonPayload,
                    inputSecondaryFhirRecord.JsonPayload))
                        .ReturnsAsync(randomComparisonResult);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            await this.comparisonCoordinationService.ProcessFhirRecords();

            // then
            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.GetUnprocessedRecordAsync(),
                    Times.Exactly(2));

            this.comparisonOrchestrationServiceMock.Verify(service =>
                service.CompareAsync(
                    inputPrimaryFhirRecord.CorrelationId,
                    inputPrimaryFhirRecord.JsonPayload,
                    inputSecondaryFhirRecord.JsonPayload),
                        Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.PersistFhirRecordDifferencesAsync(It.IsAny<CompareQueueItem>()),
                    Times.Once);

            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.ChangeFhirRecordStatusAsync(
                    inputSecondaryFhirRecord.Id,
                    StatusType.Completed),
                        Times.Once);

            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.ChangeFhirRecordStatusAsync(
                    inputPrimaryFhirRecord.Id,
                    It.IsAny<StatusType>()),
                        Times.Never);

            this.compareQueueOrchestrationServiceMock.VerifyNoOtherCalls();
            this.comparisonOrchestrationServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
