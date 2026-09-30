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

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Metrics
{
    /// <summary>
    /// A caller that has already cancelled gets its cancellation straight back, before the
    /// client is called, and nothing is logged.
    /// </summary>
    public partial class MetricServiceTests
    {
        [Fact]
        public async Task ShouldPassCancellationThroughOnAddMetricIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);
            Metric randomMetric = CreateRandomMetric();

            // when
            ValueTask<Metric> addMetricTask =
                this.metricService.AddMetricAsync(randomMetric, cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(addMetricTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.auditAndMetricBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnLogMetricIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);
            Metric randomMetric = CreateRandomMetric();

            // when
            ValueTask logMetricTask =
                this.metricService.LogMetricAsync(randomMetric, cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(logMetricTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.auditAndMetricBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnLogMetricsIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);
            List<Metric> randomMetrics = CreateRandomMetrics();

            // when
            ValueTask logMetricsTask =
                this.metricService.LogMetricsAsync(randomMetrics, cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(logMetricsTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.auditAndMetricBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnRetrieveAllMetricsIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);

            // when
            ValueTask<IQueryable<Metric>> retrieveAllMetricsTask =
                this.metricService.RetrieveAllMetricsAsync(cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(retrieveAllMetricsTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.auditAndMetricBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnRetrieveMetricByIdIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);
            Guid randomMetricId = Guid.NewGuid();

            // when
            ValueTask<Metric> retrieveMetricTask =
                this.metricService.RetrieveMetricByIdAsync(randomMetricId, cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(retrieveMetricTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.auditAndMetricBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnRemoveMetricByIdIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);
            Guid randomMetricId = Guid.NewGuid();

            // when
            ValueTask<Metric> removeMetricTask =
                this.metricService.RemoveMetricByIdAsync(randomMetricId, cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(removeMetricTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.auditAndMetricBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnPurgeMetricsIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);

            // when
            ValueTask<int> purgeMetricsTask =
                this.metricService.PurgeMetricsOlderThanRetentionPeriodAsync(cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(purgeMetricsTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.auditAndMetricBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
