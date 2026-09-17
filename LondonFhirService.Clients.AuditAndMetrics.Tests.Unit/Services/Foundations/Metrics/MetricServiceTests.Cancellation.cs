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
using LondonFhirService.Clients.AuditAndMetrics.Models.Metrics.Exceptions;
using LondonFhirService.Core.Abstractions.Models.Metrics.Exceptions;
using Moq;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Services.Foundations.Metrics
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
            IMetric randomMetric = CreateRandomMetric(randomDateTimeOffset);
            IMetric storageMetric = randomMetric;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertMetricAsync(It.IsAny<IMetric>(), It.IsAny<CancellationToken>()))
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
            List<IMetric> randomMetrics = CreateRandomMetrics(randomDateTimeOffset);

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
            IQueryable<IMetric> storageMetrics = CreateRandomMetricsQueryable();

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
            IMetric randomMetric = CreateRandomMetric();

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
            IMetric randomMetric = CreateRandomMetric();

            this.storageBrokerMock.Setup(broker =>
                broker.SelectMetricByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(randomMetric);

            this.storageBrokerMock.Setup(broker =>
                broker.DeleteMetricAsync(It.IsAny<IMetric>(), It.IsAny<CancellationToken>()))
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
            int batchSize = GetRandomNumber();
            this.metricServiceConfigurations.RetentionPeriodInDays = retentionPeriodInDays;
            this.metricServiceConfigurations.PurgeBatchSize = batchSize;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(currentDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.DeleteMetricsOlderThanAsync(
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(batchSize - 1);

            // when
            await this.metricService.PurgeMetricsOlderThanRetentionPeriodAsync(cancellationToken);

            // then
            this.storageBrokerMock.Verify(broker =>
                broker.DeleteMetricsOlderThanAsync(
                    currentDateTimeOffset.AddDays(-retentionPeriodInDays),
                    batchSize,
                    cancellationToken),
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
            IMetric randomMetric = CreateRandomMetric();

            // when
            ValueTask<IMetric> addMetricTask = this.metricService.AddMetricAsync(randomMetric, cancelledToken);

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(addMetricTask.AsTask);

            // then
            actualMetricDependencyException.InnerException.Should().BeOfType<CancelledMetricServiceException>();

            actualMetricDependencyException.InnerException.InnerException.Should()
                .BeAssignableTo<OperationCanceledException>();

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
                    Times.Once);

            // Nothing is written and the clock is never read - a token cancelled on the way in
            // stops the work before any broker is touched.
            this.storageBrokerMock.Verify(broker =>
                broker.InsertMetricAsync(It.IsAny<IMetric>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(It.IsAny<IMetric>(), It.IsAny<CancellationToken>()),
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
            List<IMetric> randomMetrics = CreateRandomMetrics(randomDateTimeOffset);

            // when
            ValueTask addMetricsTask = this.metricService.AddMetricsAsync(randomMetrics, cancelledToken);

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(addMetricsTask.AsTask);

            // then
            actualMetricDependencyException.InnerException.Should().BeOfType<CancelledMetricServiceException>();

            actualMetricDependencyException.InnerException.InnerException.Should()
                .BeAssignableTo<OperationCanceledException>();

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.BulkInsertMetricsAsync(It.IsAny<List<IMetric>>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(It.IsAny<List<IMetric>>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnAddMetricIfCancelledEvenWhenRecordingIsDisabledAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            CancellationToken cancelledToken = cancellationTokenSource.Token;
            IMetric randomMetric = CreateRandomMetric();
            this.metricServiceConfigurations.IsEnabled = false;

            // when
            ValueTask<IMetric> addMetricTask = this.metricService.AddMetricAsync(randomMetric, cancelledToken);

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(addMetricTask.AsTask);

            // then
            // The token is checked before the kill switch, so a caller that has already given up
            // is told so rather than receiving a silent success from a disabled service.
            actualMetricDependencyException.InnerException.Should().BeOfType<CancelledMetricServiceException>();

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnRetrieveAllIfTokenIsAlreadyCancelledAndLogItAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            CancellationToken cancelledToken = cancellationTokenSource.Token;

            // when
            ValueTask<IQueryable<IMetric>> retrieveAllMetricsTask =
                this.metricService.RetrieveAllMetricsAsync(cancelledToken);

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(retrieveAllMetricsTask.AsTask);

            // then
            actualMetricDependencyException.InnerException.Should().BeOfType<CancelledMetricServiceException>();

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnRetrieveByIdIfTokenIsAlreadyCancelledAndLogItAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            CancellationToken cancelledToken = cancellationTokenSource.Token;
            Guid someMetricId = Guid.NewGuid();

            // when
            ValueTask<IMetric> retrieveMetricByIdTask =
                this.metricService.RetrieveMetricByIdAsync(someMetricId, cancelledToken);

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(retrieveMetricByIdTask.AsTask);

            // then
            actualMetricDependencyException.InnerException.Should().BeOfType<CancelledMetricServiceException>();

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectMetricByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnRemoveByIdIfTokenIsAlreadyCancelledAndLogItAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            CancellationToken cancelledToken = cancellationTokenSource.Token;
            Guid someMetricId = Guid.NewGuid();

            // when
            ValueTask<IMetric> removeMetricByIdTask =
                this.metricService.RemoveMetricByIdAsync(someMetricId, cancelledToken);

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(removeMetricByIdTask.AsTask);

            // then
            actualMetricDependencyException.InnerException.Should().BeOfType<CancelledMetricServiceException>();

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectMetricByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteMetricAsync(It.IsAny<IMetric>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnPurgeIfTokenIsAlreadyCancelledAndLogItAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            CancellationToken cancelledToken = cancellationTokenSource.Token;

            // when
            ValueTask<int> purgeMetricsTask =
                this.metricService.PurgeMetricsOlderThanRetentionPeriodAsync(cancelledToken);

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(purgeMetricsTask.AsTask);

            // then
            actualMetricDependencyException.InnerException.Should().BeOfType<CancelledMetricServiceException>();

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
                    Times.Once);

            // Nothing is read and nothing is deleted.
            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteMetricsOlderThanAsync(
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
