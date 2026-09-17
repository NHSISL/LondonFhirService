// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Clients.AuditAndMetrics.Models.Metrics.Exceptions;
using LondonFhirService.Core.Abstractions.Models.Metrics.Exceptions;
using Moq;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Services.Foundations.Metrics
{
    public partial class MetricServiceTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnAddMetricsIfSqlErrorOccursAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            List<IMetric> randomMetrics = CreateRandomMetrics(randomDateTimeOffset);
            Exception storageException = GetStorageException();

            var failedStorageMetricException =
                new FailedStorageMetricException(
                    message: "Failed metric storage error occurred, contact support.",
                    innerException: storageException,
                    data: storageException.Data);

            var expectedMetricDependencyException =
                new MetricDependencyException(
                    message: "Metric dependency error occurred, contact support.",
                    innerException: failedStorageMetricException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.BulkInsertMetricsAsync(It.IsAny<List<IMetric>>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(failedStorageMetricException);

            // when
            ValueTask addMetricsTask =
                this.metricService.AddMetricsAsync(randomMetrics, TestContext.Current.CancellationToken);

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(addMetricsTask.AsTask);

            // then
            actualMetricDependencyException.Should().BeEquivalentTo(expectedMetricDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.BulkInsertMetricsAsync(randomMetrics, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedMetricDependencyException))),
                        Times.Once);

            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(It.IsAny<List<IMetric>>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Theory]
        [MemberData(nameof(TimeoutExceptions))]
        public async Task ShouldThrowDependencyExceptionOnAddMetricsIfTimedOutAndLogItAsync(Exception timeoutException)
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            List<IMetric> randomMetrics = CreateRandomMetrics(randomDateTimeOffset);

            var timedOutMetricException =
                new TimedOutMetricServiceException(
                    message: "Metric request timed out, please try again.",
                    innerException: timeoutException,
                    data: timeoutException.Data);

            var expectedMetricDependencyException =
                new MetricDependencyException(
                    message: "Metric dependency error occurred, contact support.",
                    innerException: timedOutMetricException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.BulkInsertMetricsAsync(It.IsAny<List<IMetric>>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(timeoutException);

            // when
            ValueTask addMetricsTask =
                this.metricService.AddMetricsAsync(randomMetrics, TestContext.Current.CancellationToken);

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(addMetricsTask.AsTask);

            // then
            actualMetricDependencyException.Should().BeEquivalentTo(expectedMetricDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.BulkInsertMetricsAsync(randomMetrics, It.IsAny<CancellationToken>()),
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
        public async Task ShouldThrowDependencyExceptionOnAddMetricsIfCancelledAndLogItAsync(
            Exception cancellationException)
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            List<IMetric> randomMetrics = CreateRandomMetrics(randomDateTimeOffset);

            var cancelledMetricException =
                new CancelledMetricServiceException(
                    message: "Metric request was cancelled, please try again.",
                    innerException: cancellationException,
                    data: cancellationException.Data);

            var expectedMetricDependencyException =
                new MetricDependencyException(
                    message: "Metric dependency error occurred, contact support.",
                    innerException: cancelledMetricException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.BulkInsertMetricsAsync(It.IsAny<List<IMetric>>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(cancellationException);

            // when
            ValueTask addMetricsTask =
                this.metricService.AddMetricsAsync(randomMetrics, TestContext.Current.CancellationToken);

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(addMetricsTask.AsTask);

            // then
            actualMetricDependencyException.Should().BeEquivalentTo(expectedMetricDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.BulkInsertMetricsAsync(randomMetrics, It.IsAny<CancellationToken>()),
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
        public async Task ShouldThrowDependencyValidationExceptionOnAddMetricsIfMetricAlreadyExistsAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            List<IMetric> randomMetrics = CreateRandomMetrics(randomDateTimeOffset);
            Exception storageException = GetStorageException();

            var alreadyExistsMetricException =
                new AlreadyExistsMetricException(
                    message: "Metric with the same Id already exists.",
                    innerException: storageException,
                    data: storageException.Data);

            var expectedMetricDependencyValidationException =
                new MetricDependencyValidationException(
                    message: "Metric dependency validation occurred, please try again.",
                    innerException: alreadyExistsMetricException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.BulkInsertMetricsAsync(It.IsAny<List<IMetric>>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(alreadyExistsMetricException);

            // when
            ValueTask addMetricsTask =
                this.metricService.AddMetricsAsync(randomMetrics, TestContext.Current.CancellationToken);

            MetricDependencyValidationException actualMetricDependencyValidationException =
                await Assert.ThrowsAsync<MetricDependencyValidationException>(addMetricsTask.AsTask);

            // then
            actualMetricDependencyValidationException.Should()
                .BeEquivalentTo(expectedMetricDependencyValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.BulkInsertMetricsAsync(randomMetrics, It.IsAny<CancellationToken>()),
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
        public async Task ShouldThrowDependencyValidationExceptionOnAddMetricsIfMetricIsLockedAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            List<IMetric> randomMetrics = CreateRandomMetrics(randomDateTimeOffset);
            Exception storageException = GetStorageException();

            var lockedMetricException =
                new LockedMetricException(
                    message: "Locked metric record exception, please try again later.",
                    innerException: storageException,
                    data: storageException.Data);

            var expectedMetricDependencyValidationException =
                new MetricDependencyValidationException(
                    message: "Metric dependency validation occurred, please try again.",
                    innerException: lockedMetricException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.BulkInsertMetricsAsync(It.IsAny<List<IMetric>>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(lockedMetricException);

            // when
            ValueTask addMetricsTask =
                this.metricService.AddMetricsAsync(randomMetrics, TestContext.Current.CancellationToken);

            MetricDependencyValidationException actualMetricDependencyValidationException =
                await Assert.ThrowsAsync<MetricDependencyValidationException>(addMetricsTask.AsTask);

            // then
            actualMetricDependencyValidationException.Should()
                .BeEquivalentTo(expectedMetricDependencyValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.BulkInsertMetricsAsync(randomMetrics, It.IsAny<CancellationToken>()),
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
        public async Task ShouldThrowServiceExceptionOnAddMetricsIfServiceErrorOccursAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            List<IMetric> randomMetrics = CreateRandomMetrics(randomDateTimeOffset);
            var serviceException = new Exception(GetRandomString());

            var failedMetricServiceException =
                new FailedMetricServiceException(
                    message: "Failed metric service occurred, please contact support.",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedMetricServiceException =
                new MetricServiceException(
                    message: "Metric service error occurred, contact support.",
                    innerException: failedMetricServiceException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.BulkInsertMetricsAsync(It.IsAny<List<IMetric>>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask addMetricsTask =
                this.metricService.AddMetricsAsync(randomMetrics, TestContext.Current.CancellationToken);

            MetricServiceException actualMetricServiceException =
                await Assert.ThrowsAsync<MetricServiceException>(addMetricsTask.AsTask);

            // then
            actualMetricServiceException.Should().BeEquivalentTo(expectedMetricServiceException);

            this.storageBrokerMock.Verify(broker =>
                broker.BulkInsertMetricsAsync(randomMetrics, It.IsAny<CancellationToken>()),
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
