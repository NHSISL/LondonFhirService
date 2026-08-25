// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
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
        public async Task ShouldThrowCriticalDependencyExceptionOnRemoveByIdIfSqlErrorOccursAndLogItAsync()
        {
            // given
            Guid someMetricId = Guid.NewGuid();
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
                broker.SelectMetricByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(failedStorageMetricException);

            // when
            ValueTask<IMetric> removeMetricByIdTask =
                this.metricService.RemoveMetricByIdAsync(someMetricId, TestContext.Current.CancellationToken);

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(removeMetricByIdTask.AsTask);

            // then
            actualMetricDependencyException.Should().BeEquivalentTo(expectedMetricDependencyException);

            this.metricBrokerMock.Verify(broker =>
                broker.SelectMetricByIdAsync(someMetricId, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedMetricDependencyException))),
                        Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Theory]
        [MemberData(nameof(TimeoutExceptions))]
        public async Task ShouldThrowDependencyExceptionOnRemoveByIdIfTimedOutAndLogItAsync(Exception timeoutException)
        {
            // given
            Guid someMetricId = Guid.NewGuid();

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
                broker.SelectMetricByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(timeoutException);

            // when
            ValueTask<IMetric> removeMetricByIdTask =
                this.metricService.RemoveMetricByIdAsync(someMetricId, TestContext.Current.CancellationToken);

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(removeMetricByIdTask.AsTask);

            // then
            actualMetricDependencyException.Should().BeEquivalentTo(expectedMetricDependencyException);

            this.metricBrokerMock.Verify(broker =>
                broker.SelectMetricByIdAsync(someMetricId, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricDependencyException))),
                        Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Theory]
        [MemberData(nameof(CancellationExceptions))]
        public async Task ShouldThrowDependencyExceptionOnRemoveByIdIfCancelledAndLogItAsync(
            Exception cancellationException)
        {
            // given
            Guid someMetricId = Guid.NewGuid();

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
                broker.SelectMetricByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(cancellationException);

            // when
            ValueTask<IMetric> removeMetricByIdTask =
                this.metricService.RemoveMetricByIdAsync(someMetricId, TestContext.Current.CancellationToken);

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(removeMetricByIdTask.AsTask);

            // then
            actualMetricDependencyException.Should().BeEquivalentTo(expectedMetricDependencyException);

            this.metricBrokerMock.Verify(broker =>
                broker.SelectMetricByIdAsync(someMetricId, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricDependencyException))),
                        Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnRemoveByIdIfMetricIsLockedAndLogItAsync()
        {
            // given
            IMetric randomMetric = CreateRandomMetric();
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

            this.metricBrokerMock.Setup(broker =>
                broker.SelectMetricByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(randomMetric);

            this.metricBrokerMock.Setup(broker =>
                broker.DeleteMetricAsync(It.IsAny<IMetric>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(lockedMetricException);

            // when
            ValueTask<IMetric> removeMetricByIdTask =
                this.metricService.RemoveMetricByIdAsync(randomMetric.Id, TestContext.Current.CancellationToken);

            MetricDependencyValidationException actualMetricDependencyValidationException =
                await Assert.ThrowsAsync<MetricDependencyValidationException>(removeMetricByIdTask.AsTask);

            // then
            actualMetricDependencyValidationException.Should()
                .BeEquivalentTo(expectedMetricDependencyValidationException);

            this.metricBrokerMock.Verify(broker =>
                broker.SelectMetricByIdAsync(randomMetric.Id, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.metricBrokerMock.Verify(broker =>
                broker.DeleteMetricAsync(randomMetric, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricDependencyValidationException))),
                        Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnRemoveByIdIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Guid someMetricId = Guid.NewGuid();
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
                broker.SelectMetricByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<IMetric> removeMetricByIdTask =
                this.metricService.RemoveMetricByIdAsync(someMetricId, TestContext.Current.CancellationToken);

            MetricServiceException actualMetricServiceException =
                await Assert.ThrowsAsync<MetricServiceException>(removeMetricByIdTask.AsTask);

            // then
            actualMetricServiceException.Should().BeEquivalentTo(expectedMetricServiceException);

            this.metricBrokerMock.Verify(broker =>
                broker.SelectMetricByIdAsync(someMetricId, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricServiceException))),
                        Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
