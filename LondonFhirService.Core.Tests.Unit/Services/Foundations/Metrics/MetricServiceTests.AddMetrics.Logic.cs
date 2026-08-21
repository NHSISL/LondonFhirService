// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Metrics;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Metrics
{
    public partial class MetricServiceTests
    {
        [Fact]
        public async Task ShouldAddMetricsAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            List<Metric> randomMetrics = CreateRandomMetrics(randomDateTimeOffset);
            List<Metric> inputMetrics = randomMetrics;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            await this.metricService.AddMetricsAsync(inputMetrics, TestContext.Current.CancellationToken);

            // then
            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.BulkInsertMetricsAsync(inputMetrics, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(inputMetrics, It.IsAny<CancellationToken>()),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldStampEveryMetricWithTheSameCreatedDateOnAddMetricsAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset currentDateTimeOffset = randomDateTimeOffset.AddMinutes(GetRandomNumber());
            List<Metric> randomMetrics = CreateRandomMetrics(randomDateTimeOffset);
            List<Metric> inputMetrics = randomMetrics;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(currentDateTimeOffset);

            // when
            await this.metricService.AddMetricsAsync(inputMetrics, TestContext.Current.CancellationToken);

            // then
            inputMetrics.Should().OnlyContain(metric => metric.CreatedDate == currentDateTimeOffset);

            // One reading of the clock for the whole batch, so spans flushed together are not
            // spread across the boundary of a tick.
            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.BulkInsertMetricsAsync(inputMetrics, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(inputMetrics, It.IsAny<CancellationToken>()),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldNotAddMetricsIfRecordingIsDisabledAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            List<Metric> randomMetrics = CreateRandomMetrics(randomDateTimeOffset);
            this.metricServiceConfigurations.IsEnabled = false;

            // when
            await this.metricService.AddMetricsAsync(randomMetrics, TestContext.Current.CancellationToken);

            // then
            this.storageBrokerMock.Verify(broker =>
                broker.BulkInsertMetricsAsync(It.IsAny<List<Metric>>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(It.IsAny<List<Metric>>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldNotCallBrokersOnAddMetricsIfMetricsIsEmptyAsync()
        {
            // given
            var emptyMetrics = new List<Metric>();

            // when
            await this.metricService.AddMetricsAsync(emptyMetrics, TestContext.Current.CancellationToken);

            // then
            this.storageBrokerMock.Verify(broker =>
                broker.BulkInsertMetricsAsync(It.IsAny<List<Metric>>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(It.IsAny<List<Metric>>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldWriteToStorageBeforeTelemetryOnAddMetricsAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            List<Metric> randomMetrics = CreateRandomMetrics(randomDateTimeOffset);
            var callOrder = new List<string>();

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.BulkInsertMetricsAsync(It.IsAny<List<Metric>>(), It.IsAny<CancellationToken>()))
                    .Callback(() => callOrder.Add("storage"));

            this.metricBrokerMock.Setup(broker =>
                broker.RecordAsync(It.IsAny<List<Metric>>(), It.IsAny<CancellationToken>()))
                    .Callback(() => callOrder.Add("telemetry"));

            // when
            await this.metricService.AddMetricsAsync(randomMetrics, TestContext.Current.CancellationToken);

            // then
            callOrder.Should().Equal("storage", "telemetry");

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.BulkInsertMetricsAsync(randomMetrics, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(randomMetrics, It.IsAny<CancellationToken>()),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
