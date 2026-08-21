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
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Clients.Metrics
{
    public partial class MetricClientTests
    {
        [Theory]
        [MemberData(nameof(ServiceExceptionMappings))]
        public async Task ShouldMapServiceExceptionOnAddMetricAsync(
            Xeption serviceException,
            Xeption expectedClientException)
        {
            // given
            Metric randomMetric = CreateRandomMetric();

            this.metricServiceMock.Setup(service =>
                service.AddMetricAsync(It.IsAny<Metric>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(serviceException);

            // when
            Func<Task> addMetric = async () =>
                await this.metricClient.AddMetricAsync(randomMetric, TestContext.Current.CancellationToken);

            // then
            await AssertMappedAsync(addMetric, expectedClientException);
        }

        [Theory]
        [MemberData(nameof(ServiceExceptionMappings))]
        public async Task ShouldMapServiceExceptionOnAddMetricsAsync(
            Xeption serviceException,
            Xeption expectedClientException)
        {
            // given
            List<Metric> randomMetrics = CreateRandomMetrics();

            this.metricServiceMock.Setup(service =>
                service.AddMetricsAsync(It.IsAny<List<Metric>>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(serviceException);

            // when
            Func<Task> addMetrics = async () =>
                await this.metricClient.AddMetricsAsync(randomMetrics, TestContext.Current.CancellationToken);

            // then
            await AssertMappedAsync(addMetrics, expectedClientException);
        }

        [Theory]
        [MemberData(nameof(ServiceExceptionMappings))]
        public async Task ShouldMapServiceExceptionOnRetrieveAllMetricsAsync(
            Xeption serviceException,
            Xeption expectedClientException)
        {
            // given
            this.metricServiceMock.Setup(service =>
                service.RetrieveAllMetricsAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(serviceException);

            // when
            Func<Task> retrieveAllMetrics = async () =>
                await this.metricClient.RetrieveAllMetricsAsync(TestContext.Current.CancellationToken);

            // then
            await AssertMappedAsync(retrieveAllMetrics, expectedClientException);
        }

        [Theory]
        [MemberData(nameof(ServiceExceptionMappings))]
        public async Task ShouldMapServiceExceptionOnRetrieveMetricByIdAsync(
            Xeption serviceException,
            Xeption expectedClientException)
        {
            // given
            this.metricServiceMock.Setup(service =>
                service.RetrieveMetricByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(serviceException);

            // when
            Func<Task> retrieveMetricById = async () =>
                await this.metricClient.RetrieveMetricByIdAsync(
                    Guid.NewGuid(),
                    TestContext.Current.CancellationToken);

            // then
            await AssertMappedAsync(retrieveMetricById, expectedClientException);
        }

        [Theory]
        [MemberData(nameof(ServiceExceptionMappings))]
        public async Task ShouldMapServiceExceptionOnRemoveMetricByIdAsync(
            Xeption serviceException,
            Xeption expectedClientException)
        {
            // given
            this.metricServiceMock.Setup(service =>
                service.RemoveMetricByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(serviceException);

            // when
            Func<Task> removeMetricById = async () =>
                await this.metricClient.RemoveMetricByIdAsync(
                    Guid.NewGuid(),
                    TestContext.Current.CancellationToken);

            // then
            await AssertMappedAsync(removeMetricById, expectedClientException);
        }

        [Theory]
        [MemberData(nameof(ServiceExceptionMappings))]
        public async Task ShouldMapServiceExceptionOnPurgeAsync(
            Xeption serviceException,
            Xeption expectedClientException)
        {
            // given
            this.metricServiceMock.Setup(service =>
                service.PurgeMetricsOlderThanRetentionPeriodAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(serviceException);

            // when
            Func<Task> purgeMetrics = async () =>
                await this.metricClient.PurgeMetricsOlderThanRetentionPeriodAsync(
                    TestContext.Current.CancellationToken);

            // then
            await AssertMappedAsync(purgeMetrics, expectedClientException);
        }

        /// <summary>
        /// The client exception carries the service exception's inner exception, not the service
        /// exception itself, so callers see the original cause rather than a layer of plumbing.
        /// </summary>
        private static async Task AssertMappedAsync(Func<Task> act, Xeption expectedClientException)
        {
            Xeption actualException = (await act.Should().ThrowAsync<Xeption>()).Which;
            actualException.Should().BeOfType(expectedClientException.GetType());
            actualException.Message.Should().Be(expectedClientException.Message);
            actualException.InnerException.Should().BeSameAs(expectedClientException.InnerException);
        }
    }
}
