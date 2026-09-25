// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Orchestrations.Metrics.Exceptions;
using LondonFhirService.Core.Models.Processings.Metrics;
using Moq;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Metrics
{
    public partial class MetricOrchestrationServiceTests
    {
        [Theory]
        [MemberData(nameof(DependencyValidationExceptions))]
        public async Task ShouldThrowDependencyValidationExceptionOnExportIfDependencyValidationErrorOccursAndLogItAsync(
            Xeption dependencyValidationException)
        {
            // given
            var expectedMetricOrchestrationDependencyValidationException =
                new MetricOrchestrationDependencyValidationException(
                    message: "Metric orchestration dependency validation error occurred, " +
                        "please fix errors and try again.",
                    innerException: dependencyValidationException.InnerException as Xeption);

            this.metricProcessingServiceMock.Setup(service =>
                service.RetrieveRequestMetricExportsAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(dependencyValidationException);

            // when
            ValueTask<Stream> exportTask =
                this.metricOrchestrationService.ExportRequestMetricsToCsvAsync(
                    correlationId: null,
                    userId: null,
                    fromDate: null,
                    toDate: null,
                    TestContext.Current.CancellationToken);

            MetricOrchestrationDependencyValidationException actualException =
                await Assert.ThrowsAsync<MetricOrchestrationDependencyValidationException>(
                    exportTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedMetricOrchestrationDependencyValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricOrchestrationDependencyValidationException))),
                        Times.Once);

            VerifyExportNeverWritten();
        }

        [Theory]
        [MemberData(nameof(DependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnExportIfDependencyErrorOccursAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            var expectedMetricOrchestrationDependencyException =
                new MetricOrchestrationDependencyException(
                    message: "Metric orchestration dependency error occurred, please contact support.",
                    innerException: dependencyException.InnerException as Xeption);

            this.metricProcessingServiceMock.Setup(service =>
                service.RetrieveRequestMetricExportsAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(dependencyException);

            // when
            ValueTask<Stream> exportTask =
                this.metricOrchestrationService.ExportRequestMetricsToCsvAsync(
                    correlationId: null,
                    userId: null,
                    fromDate: null,
                    toDate: null,
                    TestContext.Current.CancellationToken);

            MetricOrchestrationDependencyException actualException =
                await Assert.ThrowsAsync<MetricOrchestrationDependencyException>(exportTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedMetricOrchestrationDependencyException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricOrchestrationDependencyException))),
                        Times.Once);

            VerifyExportNeverWritten();
        }

        /// <summary>
        /// The CSV writer failing is this service's own failure rather than a dependency's: the
        /// rows were retrieved, and turning them into a file is the work this layer adds.
        /// </summary>
        [Fact]
        public async Task ShouldThrowServiceExceptionOnExportIfCsvWriteFailsAndLogItAsync()
        {
            // given
            var serviceException = new Exception(GetRandomString());

            var failedMetricOrchestrationServiceException =
                new FailedMetricOrchestrationServiceException(
                    message: "Failed metric orchestration service error occurred, please contact support.",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedMetricOrchestrationServiceException =
                new MetricOrchestrationServiceException(
                    message: "Metric orchestration service error occurred, please contact support.",
                    innerException: failedMetricOrchestrationServiceException);

            this.metricProcessingServiceMock.Setup(service =>
                service.RetrieveRequestMetricExportsAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(CreateRandomMetricExports());

            this.csvHelperBrokerMock.Setup(broker =>
                broker.MapObjectToCsvAsync(
                    It.IsAny<IAsyncEnumerable<MetricExport>>(),
                    It.IsAny<Stream>(),
                    It.IsAny<bool>(),
                    It.IsAny<Dictionary<string, int>>(),
                    It.IsAny<bool?>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(serviceException);

            // when
            ValueTask<Stream> exportTask =
                this.metricOrchestrationService.ExportRequestMetricsToCsvAsync(
                    correlationId: null,
                    userId: null,
                    fromDate: null,
                    toDate: null,
                    TestContext.Current.CancellationToken);

            MetricOrchestrationServiceException actualException =
                await Assert.ThrowsAsync<MetricOrchestrationServiceException>(exportTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedMetricOrchestrationServiceException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricOrchestrationServiceException))),
                        Times.Once);

            this.metricProcessingServiceMock.Verify(service =>
                service.RetrieveRequestMetricExportsAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.csvHelperBrokerMock.Verify(broker =>
                broker.MapObjectToCsvAsync(
                    It.IsAny<IAsyncEnumerable<MetricExport>>(),
                    It.IsAny<Stream>(),
                    It.IsAny<bool>(),
                    It.IsAny<Dictionary<string, int>>(),
                    It.IsAny<bool?>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.metricProcessingServiceMock.VerifyNoOtherCalls();
            this.csvHelperBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        private void VerifyExportNeverWritten()
        {
            this.metricProcessingServiceMock.Verify(service =>
                service.RetrieveRequestMetricExportsAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.csvHelperBrokerMock.Verify(broker =>
                broker.MapObjectToCsvAsync(
                    It.IsAny<IAsyncEnumerable<MetricExport>>(),
                    It.IsAny<Stream>(),
                    It.IsAny<bool>(),
                    It.IsAny<Dictionary<string, int>>(),
                    It.IsAny<bool?>(),
                    It.IsAny<CancellationToken>()),
                        Times.Never);

            this.metricProcessingServiceMock.VerifyNoOtherCalls();
            this.csvHelperBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
