// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Metrics;
using LondonFhirService.Core.Models.Foundations.Metrics.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Metrics
{
    public partial class MetricServiceTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnPurgeIfSqlErrorOccursAndLogItAsync()
        {
            // given
            DateTimeOffset currentDateTimeOffset = GetRandomRecentDateTimeOffset();
            SqlException sqlException = GetSqlException();

            var failedStorageMetricException =
                new FailedStorageMetricException(
                    message: "Failed metric storage error occurred, contact support.",
                    innerException: sqlException);

            var expectedMetricDependencyException =
                new MetricDependencyException(
                    message: "Metric dependency error occurred, contact support.",
                    innerException: failedStorageMetricException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(currentDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllMetricsAsync())
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<int> purgeMetricsTask = this.metricService.PurgeMetricsOlderThanRetentionPeriodAsync();

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(purgeMetricsTask.AsTask);

            // then
            actualMetricDependencyException.Should().BeEquivalentTo(expectedMetricDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedMetricDependencyException))),
                        Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Theory]
        [MemberData(nameof(TimeoutExceptions))]
        public async Task ShouldThrowDependencyExceptionOnPurgeIfTimedOutAndLogItAsync(Exception timeoutException)
        {
            // given
            DateTimeOffset currentDateTimeOffset = GetRandomRecentDateTimeOffset();

            var timedOutMetricException =
                new TimedOutMetricException(
                    message: "Metric request timed out, please try again.",
                    innerException: timeoutException,
                    data: timeoutException.Data);

            var expectedMetricDependencyException =
                new MetricDependencyException(
                    message: "Metric dependency error occurred, contact support.",
                    innerException: timedOutMetricException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(currentDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllMetricsAsync())
                    .ThrowsAsync(timeoutException);

            // when
            ValueTask<int> purgeMetricsTask = this.metricService.PurgeMetricsOlderThanRetentionPeriodAsync();

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(purgeMetricsTask.AsTask);

            // then
            actualMetricDependencyException.Should().BeEquivalentTo(expectedMetricDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricDependencyException))),
                        Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Theory]
        [MemberData(nameof(CancellationExceptions))]
        public async Task ShouldThrowDependencyExceptionOnPurgeIfCancelledAndLogItAsync(
            Exception cancellationException)
        {
            // given
            DateTimeOffset currentDateTimeOffset = GetRandomRecentDateTimeOffset();

            var cancelledMetricException =
                new CancelledMetricException(
                    message: "Metric request was cancelled, please try again.",
                    innerException: cancellationException,
                    data: cancellationException.Data);

            var expectedMetricDependencyException =
                new MetricDependencyException(
                    message: "Metric dependency error occurred, contact support.",
                    innerException: cancelledMetricException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(currentDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllMetricsAsync())
                    .ThrowsAsync(cancellationException);

            // when
            ValueTask<int> purgeMetricsTask = this.metricService.PurgeMetricsOlderThanRetentionPeriodAsync();

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(purgeMetricsTask.AsTask);

            // then
            actualMetricDependencyException.Should().BeEquivalentTo(expectedMetricDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricDependencyException))),
                        Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnPurgeIfMetricIsLockedAndLogItAsync()
        {
            // given
            DateTimeOffset currentDateTimeOffset = GetRandomRecentDateTimeOffset();
            int retentionPeriodInDays = GetRandomNumber();
            this.metricServiceConfigurations.RetentionPeriodInDays = retentionPeriodInDays;
            DateTimeOffset cutOffDate = currentDateTimeOffset.AddDays(-retentionPeriodInDays);

            Metric expiredMetric = CreateRandomMetric();
            expiredMetric.CreatedDate = cutOffDate.AddDays(-1);
            IQueryable<Metric> storageMetrics = new List<Metric> { expiredMetric }.AsQueryable();
            var dbUpdateConcurrencyException = new DbUpdateConcurrencyException();

            var lockedMetricException =
                new LockedMetricException(
                    message: "Locked metric record exception, please try again later.",
                    innerException: dbUpdateConcurrencyException);

            var expectedMetricDependencyValidationException =
                new MetricDependencyValidationException(
                    message: "Metric dependency validation occurred, please try again.",
                    innerException: lockedMetricException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(currentDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllMetricsAsync())
                    .ReturnsAsync(storageMetrics);

            this.storageBrokerMock.Setup(broker =>
                broker.BulkDeleteMetricsAsync(It.IsAny<List<Metric>>()))
                    .ThrowsAsync(dbUpdateConcurrencyException);

            // when
            ValueTask<int> purgeMetricsTask = this.metricService.PurgeMetricsOlderThanRetentionPeriodAsync();

            MetricDependencyValidationException actualMetricDependencyValidationException =
                await Assert.ThrowsAsync<MetricDependencyValidationException>(purgeMetricsTask.AsTask);

            // then
            actualMetricDependencyValidationException.Should()
                .BeEquivalentTo(expectedMetricDependencyValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.BulkDeleteMetricsAsync(It.IsAny<List<Metric>>()),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricDependencyValidationException))),
                        Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnPurgeIfDatabaseUpdateErrorOccursAndLogItAsync()
        {
            // given
            DateTimeOffset currentDateTimeOffset = GetRandomRecentDateTimeOffset();
            int retentionPeriodInDays = GetRandomNumber();
            this.metricServiceConfigurations.RetentionPeriodInDays = retentionPeriodInDays;
            DateTimeOffset cutOffDate = currentDateTimeOffset.AddDays(-retentionPeriodInDays);

            Metric expiredMetric = CreateRandomMetric();
            expiredMetric.CreatedDate = cutOffDate.AddDays(-1);
            IQueryable<Metric> storageMetrics = new List<Metric> { expiredMetric }.AsQueryable();
            var dbUpdateException = new DbUpdateException();

            var failedStorageMetricException =
                new FailedStorageMetricException(
                    message: "Failed metric storage error occurred, contact support.",
                    innerException: dbUpdateException);

            var expectedMetricDependencyException =
                new MetricDependencyException(
                    message: "Metric dependency error occurred, contact support.",
                    innerException: failedStorageMetricException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(currentDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllMetricsAsync())
                    .ReturnsAsync(storageMetrics);

            this.storageBrokerMock.Setup(broker =>
                broker.BulkDeleteMetricsAsync(It.IsAny<List<Metric>>()))
                    .ThrowsAsync(dbUpdateException);

            // when
            ValueTask<int> purgeMetricsTask = this.metricService.PurgeMetricsOlderThanRetentionPeriodAsync();

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(purgeMetricsTask.AsTask);

            // then
            actualMetricDependencyException.Should().BeEquivalentTo(expectedMetricDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.BulkDeleteMetricsAsync(It.IsAny<List<Metric>>()),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricDependencyException))),
                        Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnPurgeIfServiceErrorOccursAndLogItAsync()
        {
            // given
            DateTimeOffset currentDateTimeOffset = GetRandomRecentDateTimeOffset();
            var serviceException = new Exception(GetRandomString());

            var failedMetricServiceException =
                new FailedMetricServiceException(
                    message: "Failed metric service occurred, please contact support.",
                    innerException: serviceException);

            var expectedMetricServiceException =
                new MetricServiceException(
                    message: "Metric service error occurred, contact support.",
                    innerException: failedMetricServiceException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(currentDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllMetricsAsync())
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<int> purgeMetricsTask = this.metricService.PurgeMetricsOlderThanRetentionPeriodAsync();

            MetricServiceException actualMetricServiceException =
                await Assert.ThrowsAsync<MetricServiceException>(purgeMetricsTask.AsTask);

            // then
            actualMetricServiceException.Should().BeEquivalentTo(expectedMetricServiceException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricServiceException))),
                        Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
