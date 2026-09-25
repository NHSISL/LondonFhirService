// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Processings.Metrics;
using LondonFhirService.Core.Models.Processings.Metrics.Exceptions;
using Moq;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.Metrics
{
    public partial class MetricProcessingServiceTests
    {
        [Theory]
        [MemberData(nameof(DependencyValidationExceptions))]
        public async Task ShouldThrowDependencyValidationExceptionOnRetrieveExportsIfDependencyValidationErrorOccursAndLogItAsync(
            Xeption dependencyValidationException)
        {
            // given
            var expectedMetricProcessingDependencyValidationException =
                new MetricProcessingDependencyValidationException(
                    message: "Metric processing dependency validation error occurred, " +
                        "please fix errors and try again.",
                    innerException: dependencyValidationException.InnerException as Xeption);

            this.metricServiceMock.Setup(service =>
                service.RetrieveAllMetricsAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(dependencyValidationException);

            // when
            ValueTask<IQueryable<MetricExport>> retrieveExportsTask =
                this.metricProcessingService.RetrieveRequestMetricExportsAsync(
                    correlationId: null,
                    userId: null,
                    fromDate: null,
                    toDate: null,
                    TestContext.Current.CancellationToken);

            MetricProcessingDependencyValidationException actualException =
                await Assert.ThrowsAsync<MetricProcessingDependencyValidationException>(
                    retrieveExportsTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedMetricProcessingDependencyValidationException);

            this.metricServiceMock.Verify(service =>
                service.RetrieveAllMetricsAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricProcessingDependencyValidationException))),
                        Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(DependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnRetrieveExportsIfDependencyErrorOccursAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            var expectedMetricProcessingDependencyException =
                new MetricProcessingDependencyException(
                    message: "Metric processing dependency error occurred, please contact support.",
                    innerException: dependencyException.InnerException as Xeption);

            this.metricServiceMock.Setup(service =>
                service.RetrieveAllMetricsAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(dependencyException);

            // when
            ValueTask<IQueryable<MetricExport>> retrieveExportsTask =
                this.metricProcessingService.RetrieveRequestMetricExportsAsync(
                    correlationId: null,
                    userId: null,
                    fromDate: null,
                    toDate: null,
                    TestContext.Current.CancellationToken);

            MetricProcessingDependencyException actualException =
                await Assert.ThrowsAsync<MetricProcessingDependencyException>(retrieveExportsTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedMetricProcessingDependencyException);

            this.metricServiceMock.Verify(service =>
                service.RetrieveAllMetricsAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricProcessingDependencyException))),
                        Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnRetrieveExportsIfServiceErrorOccursAndLogItAsync()
        {
            // given
            var serviceException = new Exception(GetRandomString());

            var failedMetricProcessingServiceException =
                new FailedMetricProcessingServiceException(
                    message: "Failed metric processing service error occurred, please contact support.",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedMetricProcessingServiceException =
                new MetricProcessingServiceException(
                    message: "Metric processing service error occurred, please contact support.",
                    innerException: failedMetricProcessingServiceException);

            this.metricServiceMock.Setup(service =>
                service.RetrieveAllMetricsAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<IQueryable<MetricExport>> retrieveExportsTask =
                this.metricProcessingService.RetrieveRequestMetricExportsAsync(
                    correlationId: null,
                    userId: null,
                    fromDate: null,
                    toDate: null,
                    TestContext.Current.CancellationToken);

            MetricProcessingServiceException actualException =
                await Assert.ThrowsAsync<MetricProcessingServiceException>(retrieveExportsTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedMetricProcessingServiceException);

            this.metricServiceMock.Verify(service =>
                service.RetrieveAllMetricsAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricProcessingServiceException))),
                        Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
