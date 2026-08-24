// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Api.Workers;
using LondonFhirService.Core.Brokers.AuditAndMetrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace LondonFhirService.Api.Tests.Unit.Workers
{
    /// <summary>
    /// The retention sweep existed with no caller, so the metrics table only ever grew - and it
    /// takes a row per span rather than per request. These pin that it is actually invoked, and
    /// that a failed sweep does not take the worker down with it.
    /// </summary>
    public class MetricPurgeWorkerTests
    {
        private readonly Mock<IServiceScopeFactory> serviceScopeFactoryMock;
        private readonly Mock<IServiceScope> serviceScopeMock;
        private readonly Mock<IServiceProvider> serviceProviderMock;
        private readonly Mock<IAuditAndMetricBroker> auditAndMetricBrokerMock;
        private readonly Mock<ILogger<MetricPurgeWorker>> loggerMock;
        private readonly TestableMetricPurgeWorker worker;

        public MetricPurgeWorkerTests()
        {
            this.serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            this.serviceScopeMock = new Mock<IServiceScope>();
            this.serviceProviderMock = new Mock<IServiceProvider>();
            this.auditAndMetricBrokerMock = new Mock<IAuditAndMetricBroker>();
            this.loggerMock = new Mock<ILogger<MetricPurgeWorker>>();

            this.serviceScopeFactoryMock.Setup(factory => factory.CreateScope())
                .Returns(this.serviceScopeMock.Object);

            this.serviceScopeMock.Setup(scope => scope.ServiceProvider)
                .Returns(this.serviceProviderMock.Object);

            this.serviceProviderMock.Setup(provider => provider.GetService(typeof(IAuditAndMetricBroker)))
                .Returns(this.auditAndMetricBrokerMock.Object);

            IOptions<MetricPurgeWorkerSettings> settings = Options.Create(
                new MetricPurgeWorkerSettings { SweepIntervalHours = 0, InitialDelayMinutes = 0 });

            this.worker = new TestableMetricPurgeWorker(
                this.serviceScopeFactoryMock.Object,
                this.loggerMock.Object,
                settings);
        }

        [Fact]
        public async Task ShouldRunTheRetentionSweepAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            int purgedCount = 17;

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.PurgeMetricsOlderThanRetentionPeriodAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(purgedCount)
                    .Callback(() => cancellationTokenSource.Cancel());

            // when
            await this.worker.RunAsync(cancellationTokenSource.Token);

            // then
            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.PurgeMetricsOlderThanRetentionPeriodAsync(It.IsAny<CancellationToken>()),
                    Times.AtLeastOnce);
        }

        [Fact]
        public async Task ShouldKeepSweepingAfterAFailedSweepAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            int attempts = 0;

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.PurgeMetricsOlderThanRetentionPeriodAsync(It.IsAny<CancellationToken>()))
                    .Callback(() =>
                    {
                        attempts++;

                        if (attempts >= 2)
                        {
                            cancellationTokenSource.Cancel();
                        }
                    })
                    .ThrowsAsync(new Exception("Storage unavailable."));

            // when
            Func<Task> runWorker = async () => await this.worker.RunAsync(cancellationTokenSource.Token);

            // then
            // A failed sweep must not stop the worker - the next one picks up everything this one
            // would have deleted plus whatever has expired since.
            await runWorker.Should().NotThrowAsync();
            attempts.Should().BeGreaterThanOrEqualTo(2);
        }

        [Fact]
        public async Task ShouldStopWithoutSweepingWhenTheHostIsAlreadyShuttingDownAsync()
        {
            // given
            var alreadyCancelled = new CancellationToken(canceled: true);

            // when
            await this.worker.RunAsync(alreadyCancelled);

            // then
            // Starting a bulk delete against a host that is going away would leave a transaction
            // to be rolled back for nothing.
            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.PurgeMetricsOlderThanRetentionPeriodAsync(It.IsAny<CancellationToken>()),
                    Times.Never);
        }

        private class TestableMetricPurgeWorker : MetricPurgeWorker
        {
            public TestableMetricPurgeWorker(
                IServiceScopeFactory serviceScopeFactory,
                ILogger<MetricPurgeWorker> logger,
                IOptions<MetricPurgeWorkerSettings> settings)
                : base(serviceScopeFactory, logger, settings)
            { }

            public Task RunAsync(CancellationToken stoppingToken) =>
                ExecuteAsync(stoppingToken);
        }
    }
}
