// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Metrics;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Metrics
{
    public partial class MetricServiceTests
    {
        [Fact]
        public async Task ShouldPurgeOnlyMetricsOlderThanTheRetentionPeriodAsync()
        {
            // given
            DateTimeOffset currentDateTimeOffset = GetRandomRecentDateTimeOffset();
            int retentionPeriodInDays = GetRandomNumber();
            this.metricServiceConfigurations.RetentionPeriodInDays = retentionPeriodInDays;
            DateTimeOffset cutOffDate = currentDateTimeOffset.AddDays(-retentionPeriodInDays);

            Metric expiredMetric = CreateRandomMetric();
            expiredMetric.CreatedDate = cutOffDate.AddDays(-1);

            Metric metricOnTheBoundary = CreateRandomMetric();
            metricOnTheBoundary.CreatedDate = cutOffDate;

            Metric retainedMetric = CreateRandomMetric();
            retainedMetric.CreatedDate = cutOffDate.AddDays(1);

            IQueryable<Metric> storageMetrics =
                new List<Metric> { expiredMetric, metricOnTheBoundary, retainedMetric }.AsQueryable();

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(currentDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageMetrics);

            // when
            int actualPurgedCount =
                await this.metricService.PurgeMetricsOlderThanRetentionPeriodAsync(
                    TestContext.Current.CancellationToken);

            // then
            actualPurgedCount.Should().Be(1);

            // A metric created exactly on the cut off is retained, so the retention period is
            // the number of days kept rather than the number of days minus a boundary case.
            this.storageBrokerMock.Verify(broker =>
                broker.BulkDeleteMetricsAsync(It.Is<List<Metric>>(metrics =>
                    metrics.Count == 1 && metrics[0].Id == expiredMetric.Id), It.IsAny<CancellationToken>()),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            // The operational message reports the real count and cut off, so a wrong one
            // cannot pass unnoticed. Exactly one of the three metrics is past the cut off.
            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformationAsync($"Purged 1 metric(s) created before {cutOffDate}."),
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

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.BulkDeleteMetricsAsync(It.IsAny<List<Metric>>(), It.IsAny<CancellationToken>()),
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
        public async Task ShouldNotCallBulkDeleteIfNoMetricsAreExpiredAsync()
        {
            // given
            DateTimeOffset currentDateTimeOffset = GetRandomRecentDateTimeOffset();
            int retentionPeriodInDays = GetRandomNumber();
            this.metricServiceConfigurations.RetentionPeriodInDays = retentionPeriodInDays;
            DateTimeOffset cutOffDate = currentDateTimeOffset.AddDays(-retentionPeriodInDays);

            Metric retainedMetric = CreateRandomMetric();
            retainedMetric.CreatedDate = cutOffDate.AddDays(1);
            IQueryable<Metric> storageMetrics = new List<Metric> { retainedMetric }.AsQueryable();

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(currentDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageMetrics);

            // when
            int actualPurgedCount =
                await this.metricService.PurgeMetricsOlderThanRetentionPeriodAsync(
                    TestContext.Current.CancellationToken);

            // then
            actualPurgedCount.Should().Be(0);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.BulkDeleteMetricsAsync(It.IsAny<List<Metric>>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformationAsync(It.IsAny<string>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldPurgeEveryExpiredMetricInOneCallAsync()
        {
            // given
            DateTimeOffset currentDateTimeOffset = GetRandomRecentDateTimeOffset();
            int retentionPeriodInDays = GetRandomNumber();
            this.metricServiceConfigurations.RetentionPeriodInDays = retentionPeriodInDays;
            DateTimeOffset cutOffDate = currentDateTimeOffset.AddDays(-retentionPeriodInDays);
            int expiredCount = GetRandomNumber();
            var expiredMetrics = new List<Metric>();

            for (int index = 0; index < expiredCount; index++)
            {
                Metric expiredMetric = CreateRandomMetric();
                expiredMetric.CreatedDate = cutOffDate.AddDays(-(index + 1));
                expiredMetrics.Add(expiredMetric);
            }

            IQueryable<Metric> storageMetrics = expiredMetrics.AsQueryable();

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(currentDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageMetrics);

            // when
            int actualPurgedCount =
                await this.metricService.PurgeMetricsOlderThanRetentionPeriodAsync(
                    TestContext.Current.CancellationToken);

            // then
            actualPurgedCount.Should().Be(expiredCount);

            // One bulk call rather than a delete per row, which matters on a table this large.
            this.storageBrokerMock.Verify(broker =>
                broker.BulkDeleteMetricsAsync(It.Is<List<Metric>>(metrics =>
                    metrics.Count == expiredCount), It.IsAny<CancellationToken>()),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteMetricAsync(It.IsAny<Metric>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformationAsync($"Purged {expiredCount} metric(s) created before {cutOffDate}."),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
