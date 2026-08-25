// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Clients.AuditAndMetrics.Models.Metrics.Exceptions;
using LondonFhirService.Core.Abstractions.Models.Metrics.Exceptions;
using Moq;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Services.Foundations.Metrics
{
    public partial class MetricServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnRetrieveByIdIfIdIsInvalidAndLogItAsync()
        {
            // given
            var invalidMetricId = Guid.Empty;

            var invalidMetricException =
                new InvalidMetricException(
                    message: "Invalid metric. Please correct the errors and try again.");

            invalidMetricException.AddData(
                key: nameof(IMetric.Id),
                values: "Id is required");

            var expectedMetricValidationException =
                new MetricValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: invalidMetricException);

            // when
            ValueTask<IMetric> retrieveMetricByIdTask =
                this.metricService.RetrieveMetricByIdAsync(invalidMetricId, TestContext.Current.CancellationToken);

            MetricValidationException actualMetricValidationException =
                await Assert.ThrowsAsync<MetricValidationException>(retrieveMetricByIdTask.AsTask);

            // then
            actualMetricValidationException.Should().BeEquivalentTo(expectedMetricValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricValidationException))),
                        Times.Once);

            this.metricBrokerMock.Verify(broker =>
                broker.SelectMetricByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowNotFoundExceptionOnRetrieveByIdIfMetricIsNotFoundAndLogItAsync()
        {
            // given
            Guid someMetricId = Guid.NewGuid();
            IMetric noMetric = null;

            var notFoundMetricException =
                new NotFoundMetricException(
                    message: $"Couldn't find metric with metricId: {someMetricId}.");

            var expectedMetricValidationException =
                new MetricValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: notFoundMetricException);

            this.metricBrokerMock.Setup(broker =>
                broker.SelectMetricByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(noMetric);

            // when
            ValueTask<IMetric> retrieveMetricByIdTask =
                this.metricService.RetrieveMetricByIdAsync(someMetricId, TestContext.Current.CancellationToken);

            MetricValidationException actualMetricValidationException =
                await Assert.ThrowsAsync<MetricValidationException>(retrieveMetricByIdTask.AsTask);

            // then
            actualMetricValidationException.Should().BeEquivalentTo(expectedMetricValidationException);

            this.metricBrokerMock.Verify(broker =>
                broker.SelectMetricByIdAsync(someMetricId, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricValidationException))),
                        Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
