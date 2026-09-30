// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Processings.Metrics;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.Metrics
{
    /// <summary>
    /// Cancellation is never wrapped. A caller that cancels gets back the cancellation exception
    /// itself, and nothing is logged as an error.
    /// </summary>
    public partial class MetricProcessingServiceTests
    {
        [Theory]
        [MemberData(nameof(CancellationExceptions))]
        public async Task ShouldPassCancellationThroughOnRetrieveExportsAsync(Exception cancellationException)
        {
            // given
            this.metricServiceMock.Setup(service =>
                service.RetrieveAllMetricsAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(cancellationException);

            // when
            ValueTask<IQueryable<MetricExport>> retrieveExportsTask =
                this.metricProcessingService.RetrieveRequestMetricExportsAsync(
                    correlationId: null,
                    userId: null,
                    status: null,
                    fromDate: null,
                    toDate: null,
                    TestContext.Current.CancellationToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(retrieveExportsTask.AsTask);

            // then
            actualOperationCanceledException.Should().BeSameAs(cancellationException);

            this.metricServiceMock.Verify(service =>
                service.RetrieveAllMetricsAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnRetrieveExportsIfAlreadyCancelledAsync()
        {
            // given
            CancellationToken cancelledToken = new CancellationToken(canceled: true);

            // when
            ValueTask<IQueryable<MetricExport>> retrieveExportsTask =
                this.metricProcessingService.RetrieveRequestMetricExportsAsync(
                    correlationId: null,
                    userId: null,
                    status: null,
                    fromDate: null,
                    toDate: null,
                    cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(retrieveExportsTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.metricServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
