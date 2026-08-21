// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
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
            await this.metricService.AddMetricsAsync(inputMetrics);

            // then
            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.BulkInsertMetricsAsync(inputMetrics),
                    Times.Once);

            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(inputMetrics),
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
            await this.metricService.AddMetricsAsync(inputMetrics);

            // then
            inputMetrics.Should().OnlyContain(metric => metric.CreatedDate == currentDateTimeOffset);

            // One reading of the clock for the whole batch, so spans flushed together are not
            // spread across the boundary of a tick.
            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.BulkInsertMetricsAsync(inputMetrics),
                    Times.Once);

            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(inputMetrics),
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
            await this.metricService.AddMetricsAsync(randomMetrics);

            // then
            this.storageBrokerMock.Verify(broker =>
                broker.BulkInsertMetricsAsync(It.IsAny<List<Metric>>()),
                    Times.Never);

            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(It.IsAny<List<Metric>>()),
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
            await this.metricService.AddMetricsAsync(emptyMetrics);

            // then
            this.storageBrokerMock.Verify(broker =>
                broker.BulkInsertMetricsAsync(It.IsAny<List<Metric>>()),
                    Times.Never);

            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(It.IsAny<List<Metric>>()),
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
                broker.BulkInsertMetricsAsync(It.IsAny<List<Metric>>()))
                    .Callback(() => callOrder.Add("storage"));

            this.metricBrokerMock.Setup(broker =>
                broker.RecordAsync(It.IsAny<List<Metric>>()))
                    .Callback(() => callOrder.Add("telemetry"));

            // when
            await this.metricService.AddMetricsAsync(randomMetrics);

            // then
            callOrder.Should().Equal("storage", "telemetry");

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.BulkInsertMetricsAsync(randomMetrics),
                    Times.Once);

            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(randomMetrics),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
