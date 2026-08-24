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
            CancellationToken cancellationToken = TestContext.Current.CancellationToken;
            IMetric randomMetric = CreateRandomMetric();
            IMetric serviceMetric = randomMetric;
            IMetric expectedMetric = serviceMetric;

            this.metricServiceMock.Setup(service =>
                service.AddMetricAsync(randomMetric, cancellationToken))
                    .ReturnsAsync(serviceMetric);

            // when
            IMetric actualMetric =
                await this.metricClient.AddMetricAsync(randomMetric, cancellationToken);

            // then
            actualMetric.Should().BeSameAs(expectedMetric);

            this.metricServiceMock.Verify(service =>
                service.AddMetricAsync(randomMetric, cancellationToken),
                    Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldAddMetricsAsync()
        {
            // given
            CancellationToken cancellationToken = TestContext.Current.CancellationToken;
            List<IMetric> randomMetrics = CreateRandomMetrics();

            // when
            await this.metricClient.AddMetricsAsync(randomMetrics, cancellationToken);

            // then
            this.metricServiceMock.Verify(service =>
                service.AddMetricsAsync(randomMetrics, cancellationToken),
                    Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldRetrieveAllMetricsAsync()
        {
            // given
            CancellationToken cancellationToken = TestContext.Current.CancellationToken;
            IQueryable<IMetric> randomMetrics = CreateRandomMetricsQueryable();
            IQueryable<IMetric> expectedMetrics = randomMetrics;

            this.metricServiceMock.Setup(service =>
                service.RetrieveAllMetricsAsync(cancellationToken))
                    .ReturnsAsync(randomMetrics);

            // when
            IQueryable<IMetric> actualMetrics =
                await this.metricClient.RetrieveAllMetricsAsync(cancellationToken);

            // then
            actualMetrics.Should().BeSameAs(expectedMetrics);

            this.metricServiceMock.Verify(service =>
                service.RetrieveAllMetricsAsync(cancellationToken),
                    Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldRetrieveMetricByIdAsync()
        {
            // given
            CancellationToken cancellationToken = TestContext.Current.CancellationToken;
            IMetric randomMetric = CreateRandomMetric();
            Guid inputMetricId = randomMetric.Id;

            this.metricServiceMock.Setup(service =>
                service.RetrieveMetricByIdAsync(inputMetricId, cancellationToken))
                    .ReturnsAsync(randomMetric);

            // when
            IMetric actualMetric =
                await this.metricClient.RetrieveMetricByIdAsync(inputMetricId, cancellationToken);

            // then
            actualMetric.Should().BeSameAs(randomMetric);

            this.metricServiceMock.Verify(service =>
                service.RetrieveMetricByIdAsync(inputMetricId, cancellationToken),
                    Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldRemoveMetricByIdAsync()
        {
            // given
            CancellationToken cancellationToken = TestContext.Current.CancellationToken;
            IMetric randomMetric = CreateRandomMetric();
            Guid inputMetricId = randomMetric.Id;

            this.metricServiceMock.Setup(service =>
                service.RemoveMetricByIdAsync(inputMetricId, cancellationToken))
                    .ReturnsAsync(randomMetric);

            // when
            IMetric actualMetric =
                await this.metricClient.RemoveMetricByIdAsync(inputMetricId, cancellationToken);

            // then
            actualMetric.Should().BeSameAs(randomMetric);

            this.metricServiceMock.Verify(service =>
                service.RemoveMetricByIdAsync(inputMetricId, cancellationToken),
                    Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPurgeMetricsOlderThanRetentionPeriodAsync()
        {
            // given
            CancellationToken cancellationToken = TestContext.Current.CancellationToken;
            int randomPurgedCount = GetRandomNumber();

            this.metricServiceMock.Setup(service =>
                service.PurgeMetricsOlderThanRetentionPeriodAsync(cancellationToken))
                    .ReturnsAsync(randomPurgedCount);

            // when
            int actualPurgedCount =
                await this.metricClient.PurgeMetricsOlderThanRetentionPeriodAsync(
                    cancellationToken);

            // then
            actualPurgedCount.Should().Be(randomPurgedCount);

            this.metricServiceMock.Verify(service =>
                service.PurgeMetricsOlderThanRetentionPeriodAsync(cancellationToken),
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
                service.AddMetricAsync(randomMetric, cancellationToken))
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
