// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Metrics;
using LondonFhirService.Core.Models.Foundations.Metrics.Exceptions;
using Microsoft.Data.SqlClient;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Metrics
{
    public partial class MetricServiceTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnRetrieveByIdIfSqlErrorOccursAndLogItAsync()
        {
            // given
            Guid someMetricId = Guid.NewGuid();
            SqlException sqlException = GetSqlException();

            var failedStorageMetricException =
                new FailedStorageMetricException(
                    message: "Failed metric storage error occurred, contact support.",
                    innerException: sqlException);

            var expectedMetricDependencyException =
                new MetricDependencyException(
                    message: "Metric dependency error occurred, contact support.",
                    innerException: failedStorageMetricException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectMetricByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<Metric> retrieveMetricByIdTask = this.metricService.RetrieveMetricByIdAsync(someMetricId);

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(retrieveMetricByIdTask.AsTask);

            // then
            actualMetricDependencyException.Should().BeEquivalentTo(expectedMetricDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectMetricByIdAsync(someMetricId),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedMetricDependencyException))),
                        Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Theory]
        [MemberData(nameof(TimeoutExceptions))]
        public async Task ShouldThrowDependencyExceptionOnRetrieveByIdIfTimedOutAndLogItAsync(
            Exception timeoutException)
        {
            // given
            Guid someMetricId = Guid.NewGuid();

            var timedOutMetricException =
                new TimedOutMetricException(
                    message: "Metric request timed out, please try again.",
                    innerException: timeoutException,
                    data: timeoutException.Data);

            var expectedMetricDependencyException =
                new MetricDependencyException(
                    message: "Metric dependency error occurred, contact support.",
                    innerException: timedOutMetricException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectMetricByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(timeoutException);

            // when
            ValueTask<Metric> retrieveMetricByIdTask = this.metricService.RetrieveMetricByIdAsync(someMetricId);

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(retrieveMetricByIdTask.AsTask);

            // then
            actualMetricDependencyException.Should().BeEquivalentTo(expectedMetricDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectMetricByIdAsync(someMetricId),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricDependencyException))),
                        Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Theory]
        [MemberData(nameof(CancellationExceptions))]
        public async Task ShouldThrowDependencyExceptionOnRetrieveByIdIfCancelledAndLogItAsync(
            Exception cancellationException)
        {
            // given
            Guid someMetricId = Guid.NewGuid();

            var cancelledMetricException =
                new CancelledMetricException(
                    message: "Metric request was cancelled, please try again.",
                    innerException: cancellationException,
                    data: cancellationException.Data);

            var expectedMetricDependencyException =
                new MetricDependencyException(
                    message: "Metric dependency error occurred, contact support.",
                    innerException: cancelledMetricException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectMetricByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(cancellationException);

            // when
            ValueTask<Metric> retrieveMetricByIdTask = this.metricService.RetrieveMetricByIdAsync(someMetricId);

            MetricDependencyException actualMetricDependencyException =
                await Assert.ThrowsAsync<MetricDependencyException>(retrieveMetricByIdTask.AsTask);

            // then
            actualMetricDependencyException.Should().BeEquivalentTo(expectedMetricDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectMetricByIdAsync(someMetricId),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricDependencyException))),
                        Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnRetrieveByIdIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Guid someMetricId = Guid.NewGuid();
            var serviceException = new Exception(GetRandomString());

            var failedMetricServiceException =
                new FailedMetricServiceException(
                    message: "Failed metric service occurred, please contact support.",
                    innerException: serviceException);

            var expectedMetricServiceException =
                new MetricServiceException(
                    message: "Metric service error occurred, contact support.",
                    innerException: failedMetricServiceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectMetricByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<Metric> retrieveMetricByIdTask = this.metricService.RetrieveMetricByIdAsync(someMetricId);

            MetricServiceException actualMetricServiceException =
                await Assert.ThrowsAsync<MetricServiceException>(retrieveMetricByIdTask.AsTask);

            // then
            actualMetricServiceException.Should().BeEquivalentTo(expectedMetricServiceException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectMetricByIdAsync(someMetricId),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricServiceException))),
                        Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
