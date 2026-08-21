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
using LondonFhirService.Core.Models.Foundations.Metrics.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Metrics
{
    /// <summary>
    /// The token reaches the brokers rather than stopping at the service boundary, and a token
    /// already cancelled on the way in stops the work before any broker is touched.
    /// </summary>
    public partial class MetricServiceTests
    {
        [Fact]
        public async Task ShouldPassCancellationTokenToBrokersOnAddMetricAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Metric randomMetric = CreateRandomMetric(randomDateTimeOffset);
            Metric storageMetric = randomMetric;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertMetricAsync(It.IsAny<Metric>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageMetric);

            // when
            await this.metricService.AddMetricAsync(randomMetric, cancellationToken);

            // then
            this.storageBrokerMock.Verify(broker =>
                broker.InsertMetricAsync(randomMetric, cancellationToken),
                    Times.Once);

            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(storageMetric, cancellationToken),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldPassCancellationTokenToBrokersOnAddMetricsAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            List<Metric> randomMetrics = CreateRandomMetrics(randomDateTimeOffset);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            await this.metricService.AddMetricsAsync(randomMetrics, cancellationToken);

            // then
            this.storageBrokerMock.Verify(broker =>
                broker.BulkInsertMetricsAsync(randomMetrics, cancellationToken),
                    Times.Once);

            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(randomMetrics, cancellationToken),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldPassCancellationTokenToStorageBrokerOnRetrieveAllMetricsAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            IQueryable<Metric> storageMetrics = CreateRandomMetricsQueryable();

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageMetrics);

            // when
            await this.metricService.RetrieveAllMetricsAsync(cancellationToken);

            // then
            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(cancellationToken),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldPassCancellationTokenToStorageBrokerOnRetrieveMetricByIdAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            Metric randomMetric = CreateRandomMetric();

            this.storageBrokerMock.Setup(broker =>
                broker.SelectMetricByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(randomMetric);

            // when
            await this.metricService.RetrieveMetricByIdAsync(randomMetric.Id, cancellationToken);

            // then
            this.storageBrokerMock.Verify(broker =>
                broker.SelectMetricByIdAsync(randomMetric.Id, cancellationToken),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldPassCancellationTokenToStorageBrokerOnRemoveMetricByIdAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            Metric randomMetric = CreateRandomMetric();

            this.storageBrokerMock.Setup(broker =>
                broker.SelectMetricByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(randomMetric);

            this.storageBrokerMock.Setup(broker =>
                broker.DeleteMetricAsync(It.IsAny<Metric>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(randomMetric);

            // when
            await this.metricService.RemoveMetricByIdAsync(randomMetric.Id, cancellationToken);

            // then
            this.storageBrokerMock.Verify(broker =>
                broker.SelectMetricByIdAsync(randomMetric.Id, cancellationToken),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteMetricAsync(randomMetric, cancellationToken),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldPassCancellationTokenToStorageBrokerOnPurgeAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            DateTimeOffset currentDateTimeOffset = GetRandomRecentDateTimeOffset();
            int retentionPeriodInDays = GetRandomNumber();
            this.metricServiceConfigurations.RetentionPeriodInDays = retentionPeriodInDays;
            DateTimeOffset cutOffDate = currentDateTimeOffset.AddDays(-retentionPeriodInDays);

            Metric expiredMetric = CreateRandomMetric();
            expiredMetric.CreatedDate = cutOffDate.AddDays(-1);
            IQueryable<Metric> storageMetrics = new List<Metric> { expiredMetric }.AsQueryable();

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(currentDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageMetrics);

            // when
            await this.metricService.PurgeMetricsOlderThanRetentionPeriodAsync(cancellationToken);

            // then
            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(cancellationToken),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.BulkDeleteMetricsAsync(It.IsAny<List<Metric>>(), cancellationToken),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformationAsync(It.IsAny<string>()),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnAddMetricIfTokenIsAlreadyCancelledAndLogItAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            CancellationToken cancelledToken = cancellationTokenSource.Token;
            Metric randomMetric = CreateRandomMetric();

            // when
            ValueTask<Metric> addMetricTask = this.metricService.AddMetricAsync(randomMetric, cancelledToken);

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(addMetricTask.AsTask);

            // then
            actualMetricDependencyException.InnerException.Should().BeOfType<CancelledMetricException>();

            actualMetricDependencyException.InnerException.InnerException.Should()
                .BeAssignableTo<OperationCanceledException>();

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
                    Times.Once);

            // Nothing is written and the clock is never read - a token cancelled on the way in
            // stops the work before any broker is touched.
            this.storageBrokerMock.Verify(broker =>
                broker.InsertMetricAsync(It.IsAny<Metric>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(It.IsAny<Metric>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnAddMetricsIfTokenIsAlreadyCancelledAndLogItAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            CancellationToken cancelledToken = cancellationTokenSource.Token;
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            List<Metric> randomMetrics = CreateRandomMetrics(randomDateTimeOffset);

            // when
            ValueTask addMetricsTask = this.metricService.AddMetricsAsync(randomMetrics, cancelledToken);

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(addMetricsTask.AsTask);

            // then
            actualMetricDependencyException.InnerException.Should().BeOfType<CancelledMetricException>();

            actualMetricDependencyException.InnerException.InnerException.Should()
                .BeAssignableTo<OperationCanceledException>();

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.BulkInsertMetricsAsync(It.IsAny<List<Metric>>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(It.IsAny<List<Metric>>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldNotThrowOnAddMetricIfTokenIsCancelledButRecordingIsDisabledAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            CancellationToken cancelledToken = cancellationTokenSource.Token;
            Metric randomMetric = CreateRandomMetric();
            this.metricServiceConfigurations.IsEnabled = false;

            // when
            Metric actualMetric = await this.metricService.AddMetricAsync(randomMetric, cancelledToken);

            // then
            // The kill switch is checked first, so a disabled service stays a pure no-op rather
            // than raising a cancellation the caller would have to handle.
            actualMetric.Should().BeSameAs(randomMetric);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
