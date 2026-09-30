// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NHSOneLondon.AuditAndMetrics.Abstractions.Models.Metrics;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Metrics
{
    /// <summary>
    /// Cancellation is never wrapped. A caller that cancels an export gets back the cancellation
    /// exception itself, and nothing is logged as an error.
    /// </summary>
    public partial class MetricOrchestrationServiceTests
    {
        [Theory]
        [MemberData(nameof(CancellationExceptions))]
        public async Task ShouldPassCancellationThroughOnExportAsync(Exception cancellationException)
        {
            // given
            this.metricProcessingServiceMock.Setup(service =>
                service.RetrieveRequestMetricExportsAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<string>(),
                    It.IsAny<MetricStatus?>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(cancellationException);

            // when
            ValueTask<Stream> exportTask =
                this.metricOrchestrationService.ExportRequestMetricsToCsvAsync(
                    correlationId: null,
                    userId: null,
                    status: null,
                    fromDate: null,
                    toDate: null,
                    TestContext.Current.CancellationToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(exportTask.AsTask);

            // then
            actualOperationCanceledException.Should().BeSameAs(cancellationException);

            this.metricProcessingServiceMock.Verify(service =>
                service.RetrieveRequestMetricExportsAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<string>(),
                    It.IsAny<MetricStatus?>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.metricProcessingServiceMock.VerifyNoOtherCalls();
            this.csvHelperBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnExportIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);

            // when
            ValueTask<Stream> exportTask =
                this.metricOrchestrationService.ExportRequestMetricsToCsvAsync(
                    correlationId: null,
                    userId: null,
                    status: null,
                    fromDate: null,
                    toDate: null,
                    cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(exportTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.metricProcessingServiceMock.VerifyNoOtherCalls();
            this.csvHelperBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
