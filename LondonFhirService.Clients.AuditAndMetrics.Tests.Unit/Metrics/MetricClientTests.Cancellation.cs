// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Models.Metrics;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using Moq;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Metrics
{
    /// <summary>
    /// Cancellation is the one thing the client does not translate. A token already cancelled on
    /// the way in stops the call before the service is touched, and an OperationCanceledException
    /// travelling up from the service is re-thrown rather than wrapped.
    /// </summary>
    public partial class MetricClientTests
    {
        public static TheoryData<string> CancellableOperations() =>
            new TheoryData<string>
            {
                "AddMetric",
                "AddMetrics",
                "RetrieveAllMetrics",
                "RetrieveMetricById",
                "RemoveMetricById",
                "Purge"
            };

        [Theory]
        [MemberData(nameof(CancellableOperations))]
        public async Task ShouldThrowOperationCanceledIfTokenIsAlreadyCancelledAsync(string operation)
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            CancellationToken cancelledToken = cancellationTokenSource.Token;

            // when
            Func<Task> callingTheClient = () => InvokeAsync(operation, cancelledToken);

            // then
            await callingTheClient.Should().ThrowAsync<OperationCanceledException>();

            // The service is never reached, so a caller that has already given up costs nothing.
            this.metricServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldNotWrapAnOperationCanceledExceptionRaisedByTheServiceAsync()
        {
            // given
            IMetric randomMetric = CreateRandomMetric();
            var operationCanceledException = new OperationCanceledException();

            this.metricServiceMock.Setup(service =>
                service.AddMetricAsync(It.IsAny<IMetric>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(operationCanceledException);

            // when
            Func<Task> addMetric = async () =>
                await this.metricClient.AddMetricAsync(randomMetric, TestContext.Current.CancellationToken);

            // then
            // Surfaces as cancellation, not as a client exception the caller would have to unwrap.
            (await addMetric.Should().ThrowAsync<OperationCanceledException>())
                .Which.Should().BeSameAs(operationCanceledException);

            this.metricServiceMock.Verify(service =>
                service.AddMetricAsync(randomMetric, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldNotWrapATaskCanceledExceptionRaisedByTheServiceAsync()
        {
            // given
            IMetric randomMetric = CreateRandomMetric();
            var taskCanceledException = new TaskCanceledException();

            this.metricServiceMock.Setup(service =>
                service.AddMetricAsync(It.IsAny<IMetric>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(taskCanceledException);

            // when
            Func<Task> addMetric = async () =>
                await this.metricClient.AddMetricAsync(randomMetric, TestContext.Current.CancellationToken);

            // then
            // TaskCanceledException derives from OperationCanceledException, so it takes the same
            // path rather than falling through to the mapping catches.
            (await addMetric.Should().ThrowAsync<TaskCanceledException>())
                .Which.Should().BeSameAs(taskCanceledException);

            this.metricServiceMock.Verify(service =>
                service.AddMetricAsync(randomMetric, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.metricServiceMock.VerifyNoOtherCalls();
        }

        private Task InvokeAsync(string operation, CancellationToken cancellationToken) =>
            operation switch
            {
                "AddMetric" =>
                    this.metricClient.AddMetricAsync(CreateRandomMetric(), cancellationToken).AsTask(),

                "AddMetrics" =>
                    this.metricClient.AddMetricsAsync(CreateRandomMetrics(), cancellationToken).AsTask(),

                "RetrieveAllMetrics" =>
                    this.metricClient.RetrieveAllMetricsAsync(cancellationToken).AsTask(),

                "RetrieveMetricById" =>
                    this.metricClient.RetrieveMetricByIdAsync(Guid.NewGuid(), cancellationToken).AsTask(),

                "RemoveMetricById" =>
                    this.metricClient.RemoveMetricByIdAsync(Guid.NewGuid(), cancellationToken).AsTask(),

                "Purge" =>
                    this.metricClient.PurgeMetricsOlderThanRetentionPeriodAsync(cancellationToken).AsTask(),

                _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "Unknown operation.")
            };
    }
}
