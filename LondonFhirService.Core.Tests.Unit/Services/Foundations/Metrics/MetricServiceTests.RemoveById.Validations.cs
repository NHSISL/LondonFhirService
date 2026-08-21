// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
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
        public async Task ShouldThrowValidationExceptionOnRemoveByIdIfIdIsInvalidAndLogItAsync()
        {
            // given
            var invalidMetricId = Guid.Empty;

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

            // when
            ValueTask<Metric> removeMetricByIdTask = this.metricService.RemoveMetricByIdAsync(invalidMetricId);

            MetricValidationException actualMetricValidationException =
                await Assert.ThrowsAsync<MetricValidationException>(removeMetricByIdTask.AsTask);

            // then
            actualMetricValidationException.Should().BeEquivalentTo(expectedMetricValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectMetricByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteMetricAsync(It.IsAny<Metric>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowNotFoundExceptionOnRemoveByIdIfMetricIsNotFoundAndLogItAsync()
        {
            // given
            Guid someMetricId = Guid.NewGuid();
            Metric noMetric = null;

            var notFoundMetricException =
                new NotFoundMetricException(
                    message: $"Couldn't find metric with metricId: {someMetricId}.");

            var expectedMetricValidationException =
                new MetricValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: notFoundMetricException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectMetricByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(noMetric);

            // when
            ValueTask<Metric> removeMetricByIdTask = this.metricService.RemoveMetricByIdAsync(someMetricId);

            MetricValidationException actualMetricValidationException =
                await Assert.ThrowsAsync<MetricValidationException>(removeMetricByIdTask.AsTask);

            // then
            actualMetricValidationException.Should().BeEquivalentTo(expectedMetricValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectMetricByIdAsync(someMetricId),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteMetricAsync(It.IsAny<Metric>()),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricValidationException))),
                        Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
