// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Metrics;
using LondonFhirService.Core.Models.Foundations.Metrics.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Metrics
{
    public partial class MetricServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddMetricsIfMetricsIsNullAndLogItAsync()
        {
            // given
            List<Metric> nullMetrics = null;

            var nullMetricException =
                new NullMetricException(message: "Metrics is null.");

            var expectedMetricValidationException =
                new MetricValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: nullMetricException);

            // when
            ValueTask addMetricsTask = this.metricService.AddMetricsAsync(nullMetrics);

            MetricValidationException actualMetricValidationException =
                await Assert.ThrowsAsync<MetricValidationException>(addMetricsTask.AsTask);

            // then
            actualMetricValidationException.Should().BeEquivalentTo(expectedMetricValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.BulkInsertMetricsAsync(It.IsAny<List<Metric>>()),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddMetricsIfAnyMetricIsNullAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Metric randomMetric = CreateRandomMetric(randomDateTimeOffset);
            var metricsWithNull = new List<Metric> { randomMetric, null };

            var nullMetricException =
                new NullMetricException(message: "Metric is null.");

            var expectedMetricValidationException =
                new MetricValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: nullMetricException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask addMetricsTask = this.metricService.AddMetricsAsync(metricsWithNull);

            MetricValidationException actualMetricValidationException =
                await Assert.ThrowsAsync<MetricValidationException>(addMetricsTask.AsTask);

            // then
            actualMetricValidationException.Should().BeEquivalentTo(expectedMetricValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricValidationException))),
                        Times.Once);

            // Nothing is written when any one span in the batch is unusable, so a partial
            // flush can never leave a half recorded request behind.
            this.storageBrokerMock.Verify(broker =>
                broker.BulkInsertMetricsAsync(It.IsAny<List<Metric>>()),
                    Times.Never);

            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(It.IsAny<List<Metric>>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddMetricsIfAnyMetricIsInvalidAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Metric validMetric = CreateRandomMetric(randomDateTimeOffset);
            Metric invalidMetric = CreateRandomMetric(randomDateTimeOffset);
            invalidMetric.Id = Guid.Empty;
            var metrics = new List<Metric> { validMetric, invalidMetric };

            var invalidMetricException =
                new InvalidMetricException(
                    message: "Invalid metric. Please correct the errors and try again.");

            invalidMetricException.AddData(
                key: nameof(Metric.Id),
                values: "Id is required");

            var expectedMetricValidationException =
                new MetricValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: invalidMetricException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask addMetricsTask = this.metricService.AddMetricsAsync(metrics);

            MetricValidationException actualMetricValidationException =
                await Assert.ThrowsAsync<MetricValidationException>(addMetricsTask.AsTask);

            // then
            actualMetricValidationException.Should().BeEquivalentTo(expectedMetricValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.BulkInsertMetricsAsync(It.IsAny<List<Metric>>()),
                    Times.Never);

            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(It.IsAny<List<Metric>>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
