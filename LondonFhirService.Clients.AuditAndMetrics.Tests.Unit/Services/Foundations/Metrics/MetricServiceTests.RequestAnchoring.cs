// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Services.Foundations.Metrics
{
    /// <summary>
    /// The request span id that anchors replayed spans under the HTTP request they belong to.
    /// The timing is the whole point: it has to be read while the request is still alive, because
    /// the replay it feeds runs later on a background worker with nothing left to ask.
    /// </summary>
    public partial class MetricServiceTests
    {
        [Fact]
        public async Task ShouldPassTheRequestSpanIdToTheMetricBrokerOnLogAsync()
        {
            // given
            string randomRequestSpanId = GetRandomString();
            string expectedRequestSpanId = randomRequestSpanId;
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            IMetric randomMetric = CreateRandomMetric(randomDateTimeOffset);
            randomMetric.RequestSpanId = null;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.requestTraceBrokerMock.Setup(broker =>
                broker.GetRequestSpanIdAsync())
                    .ReturnsAsync(expectedRequestSpanId);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertMetricAsync(It.IsAny<IMetric>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IMetric inserted, CancellationToken _) => inserted);

            // when
            await this.metricService.LogMetricAsync(
                randomMetric, TestContext.Current.CancellationToken);

            // then
            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(
                    It.Is<IMetric>(metric => metric != null && metric.RequestSpanId == expectedRequestSpanId),
                    It.IsAny<CancellationToken>()),
                        Times.Once);
        }

        [Fact]
        public async Task ShouldReadTheRequestSpanIdBeforeDeferringTheWriteAsync()
        {
            // given
            string randomRequestSpanId = GetRandomString();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            IMetric randomMetric = CreateRandomMetric(randomDateTimeOffset);
            randomMetric.RequestSpanId = null;
            Func<CancellationToken, ValueTask> deferredWork = null;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.requestTraceBrokerMock.Setup(broker =>
                broker.GetRequestSpanIdAsync())
                    .ReturnsAsync(randomRequestSpanId);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertMetricAsync(It.IsAny<IMetric>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IMetric inserted, CancellationToken _) => inserted);

            // Captured rather than run, so the state at the moment of deferral can be inspected.
            this.dispatcherMock.Setup(dispatcher =>
                dispatcher.TryDispatch(It.IsAny<Func<CancellationToken, ValueTask>>()))
                    .Returns((Func<CancellationToken, ValueTask> work) =>
                    {
                        deferredWork = work;

                        return true;
                    });

            // when
            await this.metricService.LogMetricAsync(
                randomMetric, TestContext.Current.CancellationToken);

            // then
            // Already asked, before anything was handed to the dispatcher. Reading it inside the
            // closure instead would run on a background worker with no HttpContext behind it and
            // return null for every span, silently undoing the anchoring.
            this.requestTraceBrokerMock.Verify(broker =>
                broker.GetRequestSpanIdAsync(),
                    Times.Once);

            deferredWork.Should().NotBeNull();

            // and when the deferred write finally runs, it carries the value captured earlier
            await deferredWork(CancellationToken.None);

            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(
                    It.Is<IMetric>(metric => metric != null && metric.RequestSpanId == randomRequestSpanId),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.requestTraceBrokerMock.Verify(broker =>
                broker.GetRequestSpanIdAsync(),
                    Times.Once);
        }

        [Fact]
        public async Task ShouldReadTheRequestSpanIdOnceForAWholeBatchAsync()
        {
            // given
            string randomRequestSpanId = GetRandomString();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            List<IMetric> randomMetrics = CreateRandomMetrics(randomDateTimeOffset);
            randomMetrics.ForEach(metric => metric.RequestSpanId = null);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.requestTraceBrokerMock.Setup(broker =>
                broker.GetRequestSpanIdAsync())
                    .ReturnsAsync(randomRequestSpanId);

            // when
            await this.metricService.LogMetricsAsync(
                randomMetrics, TestContext.Current.CancellationToken);

            // then
            // A flush can hold every span of a request. Asking per metric would repeat the same
            // lookup for the same answer dozens of times, which is why the ambient values are all
            // read once up front.
            this.requestTraceBrokerMock.Verify(broker =>
                broker.GetRequestSpanIdAsync(),
                    Times.Once);

            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(
                    It.Is<List<IMetric>>(batch =>
                        batch.TrueForAll(metric => metric.RequestSpanId == randomRequestSpanId)),
                    It.IsAny<CancellationToken>()),
                        Times.Once);
        }

        [Fact]
        public async Task ShouldStampTheRequestSpanIdOnTheAwaitedAddPathAsync()
        {
            // given
            string randomRequestSpanId = GetRandomString();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            IMetric randomMetric = CreateRandomMetric(randomDateTimeOffset);
            randomMetric.RequestSpanId = null;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.requestTraceBrokerMock.Setup(broker =>
                broker.GetRequestSpanIdAsync())
                    .ReturnsAsync(randomRequestSpanId);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertMetricAsync(It.IsAny<IMetric>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IMetric inserted, CancellationToken _) => inserted);

            // when
            await this.metricService.AddMetricAsync(
                randomMetric, TestContext.Current.CancellationToken);

            // then
            // AddMetricAsync is awaited rather than deferred, and had no anchoring coverage at
            // all - a regression here would only have shown up in the telemetry.
            randomMetric.RequestSpanId.Should().Be(randomRequestSpanId);
            VerifyRequestSpanIdReadOnce();
        }

        [Fact]
        public async Task ShouldStampEveryMetricOfAnAwaitedBatchFromOneReadAsync()
        {
            // given
            string randomRequestSpanId = GetRandomString();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            List<IMetric> randomMetrics = CreateRandomMetrics(randomDateTimeOffset);
            randomMetrics.ForEach(metric => metric.RequestSpanId = null);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.requestTraceBrokerMock.Setup(broker =>
                broker.GetRequestSpanIdAsync())
                    .ReturnsAsync(randomRequestSpanId);

            // when
            await this.metricService.AddMetricsAsync(
                randomMetrics, TestContext.Current.CancellationToken);

            // then
            randomMetrics.Should().OnlyContain(metric => metric.RequestSpanId == randomRequestSpanId);
            VerifyRequestSpanIdReadOnce();
        }

        [Fact]
        public async Task ShouldKeepTheRequestSpanIdACallerAlreadySettledAsync()
        {
            // given
            string callerSuppliedSpanId = GetRandomString();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            IMetric randomMetric = CreateRandomMetric(randomDateTimeOffset);
            randomMetric.RequestSpanId = callerSuppliedSpanId;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.requestTraceBrokerMock.Setup(broker =>
                broker.GetRequestSpanIdAsync())
                    .ReturnsAsync(GetRandomString());

            // when
            await this.metricService.LogMetricAsync(
                randomMetric, TestContext.Current.CancellationToken);

            // then
            // The Persist span is queued before it is recorded, so it captures its own span id
            // while its request is still alive and hands it over already settled. Overwriting it
            // here would replace a real value with the null this service reads on the worker.
            randomMetric.RequestSpanId.Should().Be(callerSuppliedSpanId);

            this.requestTraceBrokerMock.Verify(broker =>
                broker.GetRequestSpanIdAsync(),
                    Times.Never);
        }

        [Fact]
        public async Task ShouldStillRecordWhenThereIsNoRequestSpanToNameAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            IMetric randomMetric = CreateRandomMetric(randomDateTimeOffset);
            randomMetric.RequestSpanId = null;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.requestTraceBrokerMock.Setup(broker =>
                broker.GetRequestSpanIdAsync())
                    .ReturnsAsync((string)null);

            // when
            await this.metricService.LogMetricAsync(
                randomMetric, TestContext.Current.CancellationToken);

            // then
            // A background worker has no request behind it. The span is still recorded; the
            // broker falls back to a parent derived from the correlation id.
            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(
                    It.Is<IMetric>(metric => metric == null || metric.RequestSpanId == null),
                    It.IsAny<CancellationToken>()),
                        Times.Once);
        }
    }
}
