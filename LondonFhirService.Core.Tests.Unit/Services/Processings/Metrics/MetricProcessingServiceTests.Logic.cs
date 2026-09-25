// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Core.Models.Foundations.Metrics;
using LondonFhirService.Core.Models.Processings.Metrics;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.Metrics
{
    public partial class MetricProcessingServiceTests
    {
        [Fact]
        public async Task ShouldPairEachRequestWithItsProviderRequestsNewestFirstAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Guid reachedProviders = Guid.NewGuid();
            Guid failedAccessCheck = Guid.NewGuid();
            Guid providerRanLong = Guid.NewGuid();

            Metric reachedProvidersRequest = CreateRandomSpan(
                reachedProviders, MetricType.Request, durationMs: 8300, randomDateTimeOffset);

            Metric failedAccessCheckRequest = CreateRandomSpan(
                failedAccessCheck, MetricType.Request, durationMs: 40, randomDateTimeOffset.AddMinutes(1));

            Metric providerRanLongRequest = CreateRandomSpan(
                providerRanLong, MetricType.Request, durationMs: 100, randomDateTimeOffset.AddMinutes(-1));

            var storageMetrics = new List<Metric>
            {
                reachedProvidersRequest,
                failedAccessCheckRequest,
                providerRanLongRequest,

                CreateRandomSpan(
                    reachedProviders, MetricType.ProviderRequests, durationMs: 8167, randomDateTimeOffset),

                // Timed by a separate stopwatch, so it can round to longer than its parent.
                CreateRandomSpan(
                    providerRanLong, MetricType.ProviderRequests, durationMs: 100.4, randomDateTimeOffset),

                // Not a Request or a ProviderRequests span, so it must not reach the export.
                CreateRandomSpan(reachedProviders, MetricType.Provider, durationMs: 5000, randomDateTimeOffset)
            };

            this.metricServiceMock.Setup(service =>
                service.RetrieveAllMetricsAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageMetrics.AsQueryable());

            // when
            IQueryable<MetricExport> actualMetricExports =
                await this.metricProcessingService.RetrieveRequestMetricExportsAsync(
                    correlationId: null,
                    userId: null,
                    status: null,
                    fromDate: null,
                    toDate: null,
                    TestContext.Current.CancellationToken);

            // then
            List<MetricExport> metricExports = actualMetricExports.ToList();

            metricExports.Select(metricExport => metricExport.CorrelationId).Should()
                .Equal(failedAccessCheck, reachedProviders, providerRanLong);

            MetricExport failedAccessCheckExport = metricExports[0];
            failedAccessCheckExport.ProviderRequestsMs.Should().BeNull();
            failedAccessCheckExport.ProxyOverheadMs.Should().BeNull();

            MetricExport reachedProvidersExport = metricExports[1];
            reachedProvidersExport.Method.Should().Be(reachedProvidersRequest.Method);
            reachedProvidersExport.Name.Should().Be(reachedProvidersRequest.Name);
            reachedProvidersExport.Status.Should().Be(reachedProvidersRequest.Status);
            reachedProvidersExport.ErrorCode.Should().Be(reachedProvidersRequest.ErrorCode);
            reachedProvidersExport.Consumer.Should().Be(reachedProvidersRequest.Consumer);
            reachedProvidersExport.UserId.Should().Be(reachedProvidersRequest.UserId);
            reachedProvidersExport.DurationMs.Should().Be(8300);
            reachedProvidersExport.ProviderRequestsMs.Should().Be(8167);
            reachedProvidersExport.ProxyOverheadMs.Should().Be(133);

            reachedProvidersExport.StartedUtc.Should()
                .Be(randomDateTimeOffset.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            metricExports[2].ProxyOverheadMs.Should().Be(0);

            this.metricServiceMock.Verify(service =>
                service.RetrieveAllMetricsAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldRoundProxyOverheadToTheRecordedPrecisionAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Guid correlationId = Guid.NewGuid();

            // The pair a real export produced 0.310299999999188 from.
            var storageMetrics = new List<Metric>
            {
                CreateRandomSpan(correlationId, MetricType.Request, durationMs: 8944.0109, randomDateTimeOffset),

                CreateRandomSpan(
                    correlationId, MetricType.ProviderRequests, durationMs: 8943.7006, randomDateTimeOffset)
            };

            this.metricServiceMock.Setup(service =>
                service.RetrieveAllMetricsAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageMetrics.AsQueryable());

            // when
            IQueryable<MetricExport> actualMetricExports =
                await this.metricProcessingService.RetrieveRequestMetricExportsAsync(
                    correlationId: null,
                    userId: null,
                    status: null,
                    fromDate: null,
                    toDate: null,
                    TestContext.Current.CancellationToken);

            // then
            actualMetricExports.Single().ProxyOverheadMs.Should().Be(0.3103);
        }

        [Fact]
        public async Task ShouldExportOnlyRequestsMatchingEveryFilterAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();
            Guid correlationId = Guid.NewGuid();

            Metric matchingRequest = CreateRandomSpan(
                correlationId, MetricType.Request, durationMs: 10, randomDateTimeOffset);

            matchingRequest.UserId = randomUserId;
            matchingRequest.Status = MetricStatus.Failed;

            Metric otherStatusRequest = CreateRandomSpan(
                correlationId, MetricType.Request, durationMs: 10, randomDateTimeOffset);

            otherStatusRequest.UserId = randomUserId;
            otherStatusRequest.Status = MetricStatus.Succeeded;

            Metric otherUserRequest = CreateRandomSpan(
                correlationId, MetricType.Request, durationMs: 10, randomDateTimeOffset);

            Metric otherCorrelationRequest = CreateRandomSpan(
                Guid.NewGuid(), MetricType.Request, durationMs: 10, randomDateTimeOffset);

            otherCorrelationRequest.UserId = randomUserId;

            Metric tooEarlyRequest = CreateRandomSpan(
                correlationId, MetricType.Request, durationMs: 10, randomDateTimeOffset.AddDays(-2));

            tooEarlyRequest.UserId = randomUserId;

            Metric tooLateRequest = CreateRandomSpan(
                correlationId, MetricType.Request, durationMs: 10, randomDateTimeOffset.AddDays(2));

            tooLateRequest.UserId = randomUserId;

            var storageMetrics = new List<Metric>
            {
                matchingRequest,
                otherStatusRequest,
                otherUserRequest,
                otherCorrelationRequest,
                tooEarlyRequest,
                tooLateRequest
            };

            this.metricServiceMock.Setup(service =>
                service.RetrieveAllMetricsAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageMetrics.AsQueryable());

            // when
            IQueryable<MetricExport> actualMetricExports =
                await this.metricProcessingService.RetrieveRequestMetricExportsAsync(
                    correlationId,
                    randomUserId,
                    MetricStatus.Failed,
                    fromDate: randomDateTimeOffset.AddDays(-1),
                    toDate: randomDateTimeOffset.AddDays(1),
                    TestContext.Current.CancellationToken);

            // then
            List<MetricExport> metricExports = actualMetricExports.ToList();
            metricExports.Should().ContainSingle();
            metricExports[0].CorrelationId.Should().Be(correlationId);
            metricExports[0].UserId.Should().Be(randomUserId);
            metricExports[0].Status.Should().Be(MetricStatus.Failed);
            metricExports[0].Started.Should().Be(randomDateTimeOffset);
        }
    }
}
