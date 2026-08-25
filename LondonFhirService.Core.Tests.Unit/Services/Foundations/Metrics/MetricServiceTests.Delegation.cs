// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
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
        public async Task ShouldForwardTheSpanOnLogMetricAsync()
        {
            // given
            Metric randomMetric = CreateRandomMetric();

            // when
            await this.metricService.LogMetricAsync(
                randomMetric, TestContext.Current.CancellationToken);

            // then
            // The same instance, unmodified. Started and Completed were taken when the work
            // happened, so anything stamped here would record the submit time instead.
            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.LogMetricAsync(randomMetric, It.IsAny<CancellationToken>()),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldForwardTheBatchOnLogMetricsAsync()
        {
            // given
            List<Metric> randomMetrics = CreateRandomMetrics();

            // when
            await this.metricService.LogMetricsAsync(
                randomMetrics, TestContext.Current.CancellationToken);

            // then
            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.LogMetricsAsync(randomMetrics, It.IsAny<CancellationToken>()),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldReturnThePurgedCountOnPurgeMetricsOlderThanRetentionPeriodAsync()
        {
            // given
            int expectedPurgedCount = GetRandomNumber();

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.PurgeMetricsOlderThanRetentionPeriodAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedPurgedCount);

            // when
            int actualPurgedCount = await this.metricService
                .PurgeMetricsOlderThanRetentionPeriodAsync(TestContext.Current.CancellationToken);

            // then
            // The count is what the purge worker logs, so swallowing it would leave a sweep that
            // reports nothing about what it did.
            actualPurgedCount.Should().Be(expectedPurgedCount);

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.PurgeMetricsOlderThanRetentionPeriodAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldPassTheCancellationTokenThroughAsync()
        {
            // given
            Metric randomMetric = CreateRandomMetric();
            using var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            // when
            await this.metricService.LogMetricAsync(randomMetric, cancellationToken);

            // then
            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.LogMetricAsync(randomMetric, cancellationToken),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
