// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
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
    public partial class MetricServiceTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnRetrieveAllIfSqlErrorOccursAndLogItAsync()
        {
            // given
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

            this.metricBrokerMock.Setup(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(failedStorageMetricException);

            // when
            ValueTask<IQueryable<IMetric>> retrieveAllMetricsTask =
                this.metricService.RetrieveAllMetricsAsync(TestContext.Current.CancellationToken);

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(retrieveAllMetricsTask.AsTask);

            // then
            actualMetricDependencyException.Should().BeEquivalentTo(expectedMetricDependencyException);

            this.metricBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedMetricDependencyException))),
                        Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Theory]
        [MemberData(nameof(TimeoutExceptions))]
        public async Task ShouldThrowDependencyExceptionOnRetrieveAllIfTimedOutAndLogItAsync(
            Exception timeoutException)
        {
            // given
            var timedOutMetricException =
                new TimedOutMetricServiceException(
                    message: "Metric request timed out, please try again.",
                    innerException: timeoutException,
                    data: timeoutException.Data);

            var expectedMetricDependencyException =
                new MetricDependencyException(
                    message: "Metric dependency error occurred, contact support.",
                    innerException: timedOutMetricException);

            this.metricBrokerMock.Setup(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(timeoutException);

            // when
            ValueTask<IQueryable<IMetric>> retrieveAllMetricsTask =
                this.metricService.RetrieveAllMetricsAsync(TestContext.Current.CancellationToken);

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(retrieveAllMetricsTask.AsTask);

            // then
            actualMetricDependencyException.Should().BeEquivalentTo(expectedMetricDependencyException);

            this.metricBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricDependencyException))),
                        Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Theory]
        [MemberData(nameof(CancellationExceptions))]
        public async Task ShouldThrowDependencyExceptionOnRetrieveAllIfCancelledAndLogItAsync(
            Exception cancellationException)
        {
            // given
            var cancelledMetricException =
                new CancelledMetricServiceException(
                    message: "Metric request was cancelled, please try again.",
                    innerException: cancellationException,
                    data: cancellationException.Data);

            var expectedMetricDependencyException =
                new MetricDependencyException(
                    message: "Metric dependency error occurred, contact support.",
                    innerException: cancelledMetricException);

            this.metricBrokerMock.Setup(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(cancellationException);

            // when
            ValueTask<IQueryable<IMetric>> retrieveAllMetricsTask =
                this.metricService.RetrieveAllMetricsAsync(TestContext.Current.CancellationToken);

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(retrieveAllMetricsTask.AsTask);

            // then
            actualMetricDependencyException.Should().BeEquivalentTo(expectedMetricDependencyException);

            this.metricBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricDependencyException))),
                        Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnRetrieveAllIfServiceErrorOccursAndLogItAsync()
        {
            // given
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

            this.metricBrokerMock.Setup(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<IQueryable<IMetric>> retrieveAllMetricsTask =
                this.metricService.RetrieveAllMetricsAsync(TestContext.Current.CancellationToken);

            MetricServiceException actualMetricServiceException =
                await Assert.ThrowsAsync<MetricServiceException>(retrieveAllMetricsTask.AsTask);

            // then
            actualMetricServiceException.Should().BeEquivalentTo(expectedMetricServiceException);

            this.metricBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricServiceException))),
                        Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
