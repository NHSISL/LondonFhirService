// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Metrics;
using LondonFhirService.Core.Models.Foundations.Metrics.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Metrics
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
                key: nameof(MetricServiceConfigurations.RetentionPeriodInDays),
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
            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.BulkDeleteMetricsAsync(It.IsAny<List<Metric>>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
