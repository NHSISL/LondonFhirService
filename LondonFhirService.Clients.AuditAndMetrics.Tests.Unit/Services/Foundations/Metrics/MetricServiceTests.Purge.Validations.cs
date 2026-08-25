// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Clients.AuditAndMetrics.Models.Configurations;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Clients.AuditAndMetrics.Models.Metrics.Exceptions;
using LondonFhirService.Core.Abstractions.Models.Metrics.Exceptions;
using Moq;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Services.Foundations.Metrics
{
    public partial class MetricServiceTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public async Task ShouldThrowValidationExceptionOnPurgeIfRetentionPeriodIsNotPositiveAndLogItAsync(
            int invalidRetentionPeriodInDays)
        {
            // given
            this.metricServiceConfigurations.RetentionPeriodInDays = invalidRetentionPeriodInDays;

            var invalidMetricException =
                new InvalidMetricException(
                    message: "Invalid metric. Please correct the errors and try again.");

            invalidMetricException.AddData(
                key: nameof(AuditAndMetricsConfigurations.RetentionPeriodInDays),
                values: "Value is expected to be greater than zero");

            var expectedMetricValidationException =
                new MetricValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: invalidMetricException);

            // when
            ValueTask<int> purgeMetricsTask =
                this.metricService.PurgeMetricsOlderThanRetentionPeriodAsync(TestContext.Current.CancellationToken);

            MetricValidationException actualMetricValidationException =
                await Assert.ThrowsAsync<MetricValidationException>(purgeMetricsTask.AsTask);

            // then
            actualMetricValidationException.Should().BeEquivalentTo(expectedMetricValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricValidationException))),
                        Times.Once);

            // Nothing is read and nothing is deleted. A retention period of zero or less would
            // put the cut off at or after the present moment and take the whole table with it.
            this.metricBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()),
                    Times.Never);

            this.metricBrokerMock.Verify(broker =>
                broker.DeleteMetricsOlderThanAsync(
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public async Task ShouldThrowValidationExceptionOnPurgeIfBatchSizeIsNotPositiveAndLogItAsync(
            int invalidPurgeBatchSize)
        {
            // given
            this.metricServiceConfigurations.PurgeBatchSize = invalidPurgeBatchSize;

            var invalidMetricException =
                new InvalidMetricException(
                    message: "Invalid metric. Please correct the errors and try again.");

            invalidMetricException.AddData(
                key: nameof(AuditAndMetricsConfigurations.PurgeBatchSize),
                values: "Value is expected to be greater than zero");

            var expectedMetricValidationException =
                new MetricValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: invalidMetricException);

            // when
            ValueTask<int> purgeMetricsTask =
                this.metricService.PurgeMetricsOlderThanRetentionPeriodAsync(
                    TestContext.Current.CancellationToken);

            MetricValidationException actualMetricValidationException =
                await Assert.ThrowsAsync<MetricValidationException>(purgeMetricsTask.AsTask);

            // then
            actualMetricValidationException.Should().BeEquivalentTo(expectedMetricValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricValidationException))),
                        Times.Once);

            // A batch size of zero or less would loop without ever deleting anything, so the
            // purge refuses rather than spinning.
            this.metricBrokerMock.Verify(broker =>
                broker.DeleteMetricsOlderThanAsync(
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                        Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
