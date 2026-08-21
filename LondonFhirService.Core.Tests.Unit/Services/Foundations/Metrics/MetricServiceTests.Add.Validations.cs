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
        public async Task ShouldThrowValidationExceptionOnAddIfMetricIsNullAndLogItAsync()
        {
            // given
            Metric nullMetric = null;

            var nullMetricException =
                new NullMetricException(message: "Metric is null.");

            var expectedMetricValidationException =
                new MetricValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: nullMetricException);

            // when
            ValueTask<Metric> addMetricTask = this.metricService.AddMetricAsync(nullMetric);

            MetricValidationException actualMetricValidationException =
                await Assert.ThrowsAsync<MetricValidationException>(addMetricTask.AsTask);

            // then
            actualMetricValidationException.Should().BeEquivalentTo(expectedMetricValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertMetricAsync(It.IsAny<Metric>()),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnAddIfMetricIsInvalidAndLogItAsync(string invalidText)
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();

            var invalidMetric = new Metric
            {
                Id = Guid.Empty,
                CorrelationId = Guid.Empty,
                Method = invalidText,
                Name = invalidText,
                Started = default,
                Completed = default
            };

            var invalidMetricException =
                new InvalidMetricException(
                    message: "Invalid metric. Please correct the errors and try again.");

            invalidMetricException.AddData(
                key: nameof(Metric.Id),
                values: "Id is required");

            invalidMetricException.AddData(
                key: nameof(Metric.CorrelationId),
                values: "Id is required");

            invalidMetricException.AddData(
                key: nameof(Metric.Method),
                values: "Text is required");

            invalidMetricException.AddData(
                key: nameof(Metric.Name),
                values: "Text is required");

            invalidMetricException.AddData(
                key: nameof(Metric.Started),
                values: "Date is required");

            invalidMetricException.AddData(
                key: nameof(Metric.Completed),
                values: "Date is required");

            var expectedMetricValidationException =
                new MetricValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: invalidMetricException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<Metric> addMetricTask = this.metricService.AddMetricAsync(invalidMetric);

            MetricValidationException actualMetricValidationException =
                await Assert.ThrowsAsync<MetricValidationException>(addMetricTask.AsTask);

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
                broker.InsertMetricAsync(It.IsAny<Metric>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfCompletedIsBeforeStartedAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Metric randomMetric = CreateRandomMetric(randomDateTimeOffset);
            Metric invalidMetric = randomMetric;
            invalidMetric.Completed = invalidMetric.Started.AddMinutes(GetRandomNegativeNumber());

            var invalidMetricException =
                new InvalidMetricException(
                    message: "Invalid metric. Please correct the errors and try again.");

            invalidMetricException.AddData(
                key: nameof(Metric.Completed),
                values: $"Date is earlier than {nameof(Metric.Started)}");

            var expectedMetricValidationException =
                new MetricValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: invalidMetricException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<Metric> addMetricTask = this.metricService.AddMetricAsync(invalidMetric);

            MetricValidationException actualMetricValidationException =
                await Assert.ThrowsAsync<MetricValidationException>(addMetricTask.AsTask);

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
                broker.InsertMetricAsync(It.IsAny<Metric>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfParentIdIsTheMetricItselfAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Metric randomMetric = CreateRandomMetric(randomDateTimeOffset);
            Metric invalidMetric = randomMetric;
            invalidMetric.ParentId = invalidMetric.Id;

            var invalidMetricException =
                new InvalidMetricException(
                    message: "Invalid metric. Please correct the errors and try again.");

            invalidMetricException.AddData(
                key: nameof(Metric.ParentId),
                values: $"Id is the same as {nameof(Metric.Id)}");

            var expectedMetricValidationException =
                new MetricValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: invalidMetricException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<Metric> addMetricTask = this.metricService.AddMetricAsync(invalidMetric);

            MetricValidationException actualMetricValidationException =
                await Assert.ThrowsAsync<MetricValidationException>(addMetricTask.AsTask);

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
                broker.InsertMetricAsync(It.IsAny<Metric>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfDurationIsNegativeAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Metric randomMetric = CreateRandomMetric(randomDateTimeOffset);
            Metric invalidMetric = randomMetric;
            invalidMetric.DurationMs = GetRandomNegativeNumber();
            invalidMetric.PayloadBytes = GetRandomNegativeNumber();

            var invalidMetricException =
                new InvalidMetricException(
                    message: "Invalid metric. Please correct the errors and try again.");

            invalidMetricException.AddData(
                key: nameof(Metric.DurationMs),
                values: "Value is not expected to be negative");

            invalidMetricException.AddData(
                key: nameof(Metric.PayloadBytes),
                values: "Value is not expected to be negative");

            var expectedMetricValidationException =
                new MetricValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: invalidMetricException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<Metric> addMetricTask = this.metricService.AddMetricAsync(invalidMetric);

            MetricValidationException actualMetricValidationException =
                await Assert.ThrowsAsync<MetricValidationException>(addMetricTask.AsTask);

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
                broker.InsertMetricAsync(It.IsAny<Metric>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfTextExceedsMaxLengthAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Metric randomMetric = CreateRandomMetric(randomDateTimeOffset);
            Metric invalidMetric = randomMetric;
            invalidMetric.Method = GetRandomStringWithLengthOf(256);
            invalidMetric.Name = GetRandomStringWithLengthOf(256);
            invalidMetric.Target = GetRandomStringWithLengthOf(256);
            invalidMetric.Consumer = GetRandomStringWithLengthOf(256);
            invalidMetric.ErrorCode = GetRandomStringWithLengthOf(101);

            var invalidMetricException =
                new InvalidMetricException(
                    message: "Invalid metric. Please correct the errors and try again.");

            invalidMetricException.AddData(
                key: nameof(Metric.Method),
                values: "Text exceed max length of 255 characters");

            invalidMetricException.AddData(
                key: nameof(Metric.Name),
                values: "Text exceed max length of 255 characters");

            invalidMetricException.AddData(
                key: nameof(Metric.Target),
                values: "Text exceed max length of 255 characters");

            invalidMetricException.AddData(
                key: nameof(Metric.ErrorCode),
                values: "Text exceed max length of 100 characters");

            invalidMetricException.AddData(
                key: nameof(Metric.Consumer),
                values: "Text exceed max length of 255 characters");

            var expectedMetricValidationException =
                new MetricValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: invalidMetricException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<Metric> addMetricTask = this.metricService.AddMetricAsync(invalidMetric);

            MetricValidationException actualMetricValidationException =
                await Assert.ThrowsAsync<MetricValidationException>(addMetricTask.AsTask);

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
                broker.InsertMetricAsync(It.IsAny<Metric>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfTypeOrStatusIsUndefinedAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Metric randomMetric = CreateRandomMetric(randomDateTimeOffset);
            Metric invalidMetric = randomMetric;
            invalidMetric.Type = (MetricType)GetRandomNegativeNumber();
            invalidMetric.Status = (MetricStatus)GetRandomNegativeNumber();

            var invalidMetricException =
                new InvalidMetricException(
                    message: "Invalid metric. Please correct the errors and try again.");

            invalidMetricException.AddData(
                key: nameof(Metric.Type),
                values: "Type is invalid");

            invalidMetricException.AddData(
                key: nameof(Metric.Status),
                values: "Status is invalid");

            var expectedMetricValidationException =
                new MetricValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: invalidMetricException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<Metric> addMetricTask = this.metricService.AddMetricAsync(invalidMetric);

            MetricValidationException actualMetricValidationException =
                await Assert.ThrowsAsync<MetricValidationException>(addMetricTask.AsTask);

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
                broker.InsertMetricAsync(It.IsAny<Metric>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
