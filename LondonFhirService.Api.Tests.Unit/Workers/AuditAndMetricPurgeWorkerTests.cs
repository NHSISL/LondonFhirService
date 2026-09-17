// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Api.Workers;
using LondonFhirService.Core.Services.Foundations.Audits;
using LondonFhirService.Core.Services.Foundations.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace LondonFhirService.Api.Tests.Unit.Workers
{
    /// <summary>
    /// The retention sweeps existed with no caller, so both tables only ever grew - and the
    /// metrics one takes a row per span rather than per request. These pin that both are
    /// actually invoked, that a failed sweep does not take the worker down with it, and that a
    /// failure in one sweep does not cost the other its turn.
    /// </summary>
    public class AuditAndMetricPurgeWorkerTests
    {
        private readonly Mock<IServiceScopeFactory> serviceScopeFactoryMock;
        private readonly Mock<IServiceScope> serviceScopeMock;
        private readonly Mock<IServiceProvider> serviceProviderMock;
        private readonly Mock<IAuditService> auditServiceMock;
        private readonly Mock<IMetricService> metricServiceMock;
        private readonly Mock<ILogger<AuditAndMetricPurgeWorker>> loggerMock;
        private readonly TestableAuditAndMetricPurgeWorker worker;

        public AuditAndMetricPurgeWorkerTests()
        {
            this.serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            this.serviceScopeMock = new Mock<IServiceScope>();
            this.serviceProviderMock = new Mock<IServiceProvider>();
            this.auditServiceMock = new Mock<IAuditService>();
            this.metricServiceMock = new Mock<IMetricService>();
            this.loggerMock = new Mock<ILogger<AuditAndMetricPurgeWorker>>();

            this.serviceScopeFactoryMock.Setup(factory => factory.CreateScope())
                .Returns(this.serviceScopeMock.Object);

            this.serviceScopeMock.Setup(scope => scope.ServiceProvider)
                .Returns(this.serviceProviderMock.Object);

            this.serviceProviderMock.Setup(provider => provider.GetService(typeof(IAuditService)))
                .Returns(this.auditServiceMock.Object);

            this.serviceProviderMock.Setup(provider => provider.GetService(typeof(IMetricService)))
                .Returns(this.metricServiceMock.Object);

            IOptions<AuditAndMetricPurgeWorkerSettings> settings = Options.Create(
                new AuditAndMetricPurgeWorkerSettings { SweepIntervalHours = 0, InitialDelayMinutes = 0 });

            this.worker = new TestableAuditAndMetricPurgeWorker(
                this.serviceScopeFactoryMock.Object,
                this.loggerMock.Object,
                settings);
        }

        [Fact]
        public async Task ShouldRunBothRetentionSweepsAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();

            this.auditServiceMock.Setup(service =>
                service.PurgeAuditsOlderThanRetentionPeriodAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(11);

            this.metricServiceMock.Setup(service =>
                service.PurgeMetricsOlderThanRetentionPeriodAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(17)
                    .Callback(() => cancellationTokenSource.Cancel());

            // when
            await this.worker.RunAsync(cancellationTokenSource.Token);

            // then
            this.auditServiceMock.Verify(service =>
                service.PurgeAuditsOlderThanRetentionPeriodAsync(It.IsAny<CancellationToken>()),
                    Times.AtLeastOnce);

            this.metricServiceMock.Verify(service =>
                service.PurgeMetricsOlderThanRetentionPeriodAsync(It.IsAny<CancellationToken>()),
                    Times.AtLeastOnce);
        }

        [Fact]
        public async Task ShouldStillSweepMetricsWhenTheAuditSweepFailsAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();

            this.auditServiceMock.Setup(service =>
                service.PurgeAuditsOlderThanRetentionPeriodAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception("Audit storage unavailable."));

            this.metricServiceMock.Setup(service =>
                service.PurgeMetricsOlderThanRetentionPeriodAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(17)
                    .Callback(() => cancellationTokenSource.Cancel());

            // when
            await this.worker.RunAsync(cancellationTokenSource.Token);

            // then
            // Separate try blocks, so a fault in one table's sweep cannot quietly stop the other
            // from ageing out.
            this.metricServiceMock.Verify(service =>
                service.PurgeMetricsOlderThanRetentionPeriodAsync(It.IsAny<CancellationToken>()),
                    Times.AtLeastOnce);
        }

        [Fact]
        public async Task ShouldStillSweepAuditsWhenTheMetricSweepFailsAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            int auditSweeps = 0;

            this.auditServiceMock.Setup(service =>
                service.PurgeAuditsOlderThanRetentionPeriodAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(11)
                    .Callback(() =>
                    {
                        auditSweeps++;

                        if (auditSweeps >= 2)
                        {
                            cancellationTokenSource.Cancel();
                        }
                    });

            this.metricServiceMock.Setup(service =>
                service.PurgeMetricsOlderThanRetentionPeriodAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception("Metric storage unavailable."));

            // when
            await this.worker.RunAsync(cancellationTokenSource.Token);

            // then
            // The audit sweep comes round again on the next pass despite the metric sweep
            // failing every time.
            auditSweeps.Should().BeGreaterThanOrEqualTo(2);
        }

        [Fact]
        public async Task ShouldKeepSweepingAfterAFailedSweepAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            int attempts = 0;

            this.auditServiceMock.Setup(service =>
                service.PurgeAuditsOlderThanRetentionPeriodAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(0);

            this.metricServiceMock.Setup(service =>
                service.PurgeMetricsOlderThanRetentionPeriodAsync(It.IsAny<CancellationToken>()))
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
            this.auditServiceMock.Verify(service =>
                service.PurgeAuditsOlderThanRetentionPeriodAsync(It.IsAny<CancellationToken>()),
                    Times.Never);

            this.metricServiceMock.Verify(service =>
                service.PurgeMetricsOlderThanRetentionPeriodAsync(It.IsAny<CancellationToken>()),
                    Times.Never);
        }

        private class TestableAuditAndMetricPurgeWorker : AuditAndMetricPurgeWorker
        {
            public TestableAuditAndMetricPurgeWorker(
                IServiceScopeFactory serviceScopeFactory,
                ILogger<AuditAndMetricPurgeWorker> logger,
                IOptions<AuditAndMetricPurgeWorkerSettings> settings)
                : base(serviceScopeFactory, logger, settings)
            { }

            public Task RunAsync(CancellationToken stoppingToken) =>
                ExecuteAsync(stoppingToken);
        }
    }
}
