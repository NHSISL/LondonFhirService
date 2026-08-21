// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Models.Metrics;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using Moq;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Metrics
{
    public partial class MetricClientTests
    {
        [Fact]
        public async Task ShouldAddMetricAsync()
        {
            // given
            IMetric randomMetric = CreateRandomMetric();
            IMetric serviceMetric = randomMetric;
            IMetric expectedMetric = serviceMetric;

            this.metricServiceMock.Setup(service =>
                service.AddMetricAsync(randomMetric, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(serviceMetric);

            // when
            IMetric actualMetric =
                await this.metricClient.AddMetricAsync(randomMetric, TestContext.Current.CancellationToken);

            // then
            actualMetric.Should().BeSameAs(expectedMetric);

            this.metricServiceMock.Verify(service =>
                service.AddMetricAsync(randomMetric, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldAddMetricsAsync()
        {
            // given
            List<IMetric> randomMetrics = CreateRandomMetrics();

            // when
            await this.metricClient.AddMetricsAsync(randomMetrics, TestContext.Current.CancellationToken);

            // then
            this.metricServiceMock.Verify(service =>
                service.AddMetricsAsync(randomMetrics, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldRetrieveAllMetricsAsync()
        {
            // given
            IQueryable<IMetric> randomMetrics = CreateRandomMetricsQueryable();
            IQueryable<IMetric> expectedMetrics = randomMetrics;

            this.metricServiceMock.Setup(service =>
                service.RetrieveAllMetricsAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(randomMetrics);

            // when
            IQueryable<IMetric> actualMetrics =
                await this.metricClient.RetrieveAllMetricsAsync(TestContext.Current.CancellationToken);

            // then
            actualMetrics.Should().BeSameAs(expectedMetrics);

            this.metricServiceMock.Verify(service =>
                service.RetrieveAllMetricsAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldRetrieveMetricByIdAsync()
        {
            // given
            IMetric randomMetric = CreateRandomMetric();
            Guid inputMetricId = randomMetric.Id;

            this.metricServiceMock.Setup(service =>
                service.RetrieveMetricByIdAsync(inputMetricId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(randomMetric);

            // when
            IMetric actualMetric =
                await this.metricClient.RetrieveMetricByIdAsync(inputMetricId, TestContext.Current.CancellationToken);

            // then
            actualMetric.Should().BeSameAs(randomMetric);

            this.metricServiceMock.Verify(service =>
                service.RetrieveMetricByIdAsync(inputMetricId, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldRemoveMetricByIdAsync()
        {
            // given
            IMetric randomMetric = CreateRandomMetric();
            Guid inputMetricId = randomMetric.Id;

            this.metricServiceMock.Setup(service =>
                service.RemoveMetricByIdAsync(inputMetricId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(randomMetric);

            // when
            IMetric actualMetric =
                await this.metricClient.RemoveMetricByIdAsync(inputMetricId, TestContext.Current.CancellationToken);

            // then
            actualMetric.Should().BeSameAs(randomMetric);

            this.metricServiceMock.Verify(service =>
                service.RemoveMetricByIdAsync(inputMetricId, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPurgeMetricsOlderThanRetentionPeriodAsync()
        {
            // given
            int randomPurgedCount = GetRandomNumber();

            this.metricServiceMock.Setup(service =>
                service.PurgeMetricsOlderThanRetentionPeriodAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(randomPurgedCount);

            // when
            int actualPurgedCount =
                await this.metricClient.PurgeMetricsOlderThanRetentionPeriodAsync(
                    TestContext.Current.CancellationToken);

            // then
            actualPurgedCount.Should().Be(randomPurgedCount);

            this.metricServiceMock.Verify(service =>
                service.PurgeMetricsOlderThanRetentionPeriodAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassTheCallersTokenThroughToTheServiceAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            IMetric randomMetric = CreateRandomMetric();

            this.metricServiceMock.Setup(service =>
                service.AddMetricAsync(It.IsAny<IMetric>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(randomMetric);

            // when
            await this.metricClient.AddMetricAsync(randomMetric, cancellationToken);

            // then
            // The exact token, so the client cannot quietly swallow it and call the service with
            // CancellationToken.None.
            this.metricServiceMock.Verify(service =>
                service.AddMetricAsync(randomMetric, cancellationToken),
                    Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }
    }
}
