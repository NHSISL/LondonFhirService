// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Services.Foundations.Metrics
{
    public partial class MetricServiceTests
    {
        [Fact]
        public async Task ShouldPurgeMetricsOlderThanTheRetentionPeriodAsync()
        {
            // given
            DateTimeOffset currentDateTimeOffset = GetRandomRecentDateTimeOffset();
            int retentionPeriodInDays = GetRandomNumber();
            int batchSize = GetRandomNumber();
            int deletedCount = batchSize - 1;
            this.metricServiceConfigurations.RetentionPeriodInDays = retentionPeriodInDays;
            this.metricServiceConfigurations.PurgeBatchSize = batchSize;
            DateTimeOffset expectedCutOffDate = currentDateTimeOffset.AddDays(-retentionPeriodInDays);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(currentDateTimeOffset);

            this.metricBrokerMock.Setup(broker =>
                broker.DeleteMetricsOlderThanAsync(
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(deletedCount);

            // when
            int actualPurgedCount =
                await this.metricService.PurgeMetricsOlderThanRetentionPeriodAsync(
                    TestContext.Current.CancellationToken);

            // then
            actualPurgedCount.Should().Be(deletedCount);

            // The cut off is derived from the retention period, and the delete runs in the
            // database rather than over a materialised candidate list.
            this.metricBrokerMock.Verify(broker =>
                broker.DeleteMetricsOlderThanAsync(
                    expectedCutOffDate,
                    batchSize,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.metricBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    $"Purged {deletedCount} metric(s) created before {expectedCutOffDate}."),
                        Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldKeepPurgingInBatchesUntilABatchComesBackShortAsync()
        {
            // given
            DateTimeOffset currentDateTimeOffset = GetRandomRecentDateTimeOffset();
            int retentionPeriodInDays = GetRandomNumber();
            int batchSize = GetRandomNumber();
            int lastBatchCount = batchSize - 1;
            int expectedTotal = batchSize + batchSize + lastBatchCount;
            this.metricServiceConfigurations.RetentionPeriodInDays = retentionPeriodInDays;
            this.metricServiceConfigurations.PurgeBatchSize = batchSize;
            DateTimeOffset expectedCutOffDate = currentDateTimeOffset.AddDays(-retentionPeriodInDays);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(currentDateTimeOffset);

            this.metricBrokerMock.SetupSequence(broker =>
                broker.DeleteMetricsOlderThanAsync(
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(batchSize)
                        .ReturnsAsync(batchSize)
                        .ReturnsAsync(lastBatchCount);

            // when
            int actualPurgedCount =
                await this.metricService.PurgeMetricsOlderThanRetentionPeriodAsync(
                    TestContext.Current.CancellationToken);

            // then
            // A full batch means there may be more, so the loop continues; a short one means the
            // window is exhausted. The total is the sum the database actually reported.
            actualPurgedCount.Should().Be(expectedTotal);

            this.metricBrokerMock.Verify(broker =>
                broker.DeleteMetricsOlderThanAsync(
                    expectedCutOffDate,
                    batchSize,
                    It.IsAny<CancellationToken>()),
                        Times.Exactly(3));

            // The clock is read once, so every batch uses the same cut off rather than letting it
            // drift forward while the purge runs.
            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    $"Purged {expectedTotal} metric(s) created before {expectedCutOffDate}."),
                        Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldNotPurgeIfPurgingIsNotAllowedAsync()
        {
            // given
            this.metricServiceConfigurations.IsPurgingAllowed = false;

            // when
            int actualPurgedCount =
                await this.metricService.PurgeMetricsOlderThanRetentionPeriodAsync(
                    TestContext.Current.CancellationToken);

            // then
            actualPurgedCount.Should().Be(0);

            this.metricBrokerMock.Verify(broker =>
                broker.DeleteMetricsOlderThanAsync(
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                        Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldNotPurgeIfPurgingIsNotAllowedEvenWhenRetentionPeriodIsInvalidAsync()
        {
            // given
            this.metricServiceConfigurations.IsPurgingAllowed = false;
            this.metricServiceConfigurations.RetentionPeriodInDays = GetRandomNegativeNumber();

            // when
            int actualPurgedCount =
                await this.metricService.PurgeMetricsOlderThanRetentionPeriodAsync(
                    TestContext.Current.CancellationToken);

            // then
            actualPurgedCount.Should().Be(0);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldNotLogWhenNothingWasPurgedAsync()
        {
            // given
            DateTimeOffset currentDateTimeOffset = GetRandomRecentDateTimeOffset();
            this.metricServiceConfigurations.PurgeBatchSize = GetRandomNumber();

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(currentDateTimeOffset);

            this.metricBrokerMock.Setup(broker =>
                broker.DeleteMetricsOlderThanAsync(
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(0);

            // when
            int actualPurgedCount =
                await this.metricService.PurgeMetricsOlderThanRetentionPeriodAsync(
                    TestContext.Current.CancellationToken);

            // then
            actualPurgedCount.Should().Be(0);

            this.metricBrokerMock.Verify(broker =>
                broker.DeleteMetricsOlderThanAsync(
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            // A purge that removed nothing is not worth an operational log line.
            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformationAsync(It.IsAny<string>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
