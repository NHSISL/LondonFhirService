// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Models.Metrics;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Clients.AuditAndMetrics.Models.Metrics.Exceptions;
using LondonFhirService.Core.Abstractions.Models.Metrics.Exceptions;
using Moq;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Services.Foundations.Metrics
{
    public partial class MetricServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfMetricIsNullAndLogItAsync()
        {
            // given
            IMetric nullMetric = null;

            var nullMetricException =
                new NullMetricException(message: "Metric is null.");

            var expectedMetricValidationException =
                new MetricValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: nullMetricException);

            // when
            ValueTask<IMetric> addMetricTask =
                this.metricService.AddMetricAsync(nullMetric, TestContext.Current.CancellationToken);

            MetricValidationException actualMetricValidationException =
                await Assert.ThrowsAsync<MetricValidationException>(addMetricTask.AsTask);

            // then
            actualMetricValidationException.Should().BeEquivalentTo(expectedMetricValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertMetricAsync(It.IsAny<IMetric>(), It.IsAny<CancellationToken>()),
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

            var invalidMetric = new TestMetric
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
                key: nameof(IMetric.Id),
                values: "Id is required");

            invalidMetricException.AddData(
                key: nameof(IMetric.CorrelationId),
                values: "Id is required");

            invalidMetricException.AddData(
                key: nameof(IMetric.Method),
                values: "Text is required");

            invalidMetricException.AddData(
                key: nameof(IMetric.Name),
                values: "Text is required");

            invalidMetricException.AddData(
                key: nameof(IMetric.Started),
                values: "Date is required");

            invalidMetricException.AddData(
                key: nameof(IMetric.Completed),
                values: "Date is required");

            var expectedMetricValidationException =
                new MetricValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: invalidMetricException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<IMetric> addMetricTask =
                this.metricService.AddMetricAsync(invalidMetric, TestContext.Current.CancellationToken);

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
                broker.InsertMetricAsync(It.IsAny<IMetric>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfCompletedIsBeforeStartedAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            IMetric randomMetric = CreateRandomMetric(randomDateTimeOffset);
            IMetric invalidMetric = randomMetric;
            invalidMetric.Completed = invalidMetric.Started.AddMinutes(GetRandomNegativeNumber());

            var invalidMetricException =
                new InvalidMetricException(
                    message: "Invalid metric. Please correct the errors and try again.");

            invalidMetricException.AddData(
                key: nameof(IMetric.Completed),
                values: $"Date is earlier than {nameof(IMetric.Started)}");

            var expectedMetricValidationException =
                new MetricValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: invalidMetricException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<IMetric> addMetricTask =
                this.metricService.AddMetricAsync(invalidMetric, TestContext.Current.CancellationToken);

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
                broker.InsertMetricAsync(It.IsAny<IMetric>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfParentIdIsTheMetricItselfAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            IMetric randomMetric = CreateRandomMetric(randomDateTimeOffset);
            IMetric invalidMetric = randomMetric;
            invalidMetric.ParentId = invalidMetric.Id;

            var invalidMetricException =
                new InvalidMetricException(
                    message: "Invalid metric. Please correct the errors and try again.");

            invalidMetricException.AddData(
                key: nameof(IMetric.ParentId),
                values: $"Id is the same as {nameof(IMetric.Id)}");

            var expectedMetricValidationException =
                new MetricValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: invalidMetricException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<IMetric> addMetricTask =
                this.metricService.AddMetricAsync(invalidMetric, TestContext.Current.CancellationToken);

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
                broker.InsertMetricAsync(It.IsAny<IMetric>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfDurationIsNegativeAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            IMetric randomMetric = CreateRandomMetric(randomDateTimeOffset);
            IMetric invalidMetric = randomMetric;
            invalidMetric.DurationMs = GetRandomNegativeNumber();
            invalidMetric.PayloadBytes = GetRandomNegativeNumber();

            var invalidMetricException =
                new InvalidMetricException(
                    message: "Invalid metric. Please correct the errors and try again.");

            invalidMetricException.AddData(
                key: nameof(IMetric.DurationMs),
                values: "Value is not expected to be negative");

            invalidMetricException.AddData(
                key: nameof(IMetric.PayloadBytes),
                values: "Value is not expected to be negative");

            var expectedMetricValidationException =
                new MetricValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: invalidMetricException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<IMetric> addMetricTask =
                this.metricService.AddMetricAsync(invalidMetric, TestContext.Current.CancellationToken);

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
                broker.InsertMetricAsync(It.IsAny<IMetric>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfTextExceedsMaxLengthAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            IMetric randomMetric = CreateRandomMetric(randomDateTimeOffset);
            IMetric invalidMetric = randomMetric;
            invalidMetric.Method = GetRandomStringWithLengthOf(256);
            invalidMetric.Name = GetRandomStringWithLengthOf(256);
            invalidMetric.Target = GetRandomStringWithLengthOf(256);
            invalidMetric.Consumer = GetRandomStringWithLengthOf(256);
            invalidMetric.ErrorCode = GetRandomStringWithLengthOf(101);
            invalidMetric.Description = GetRandomStringWithLengthOf(1001);

            var invalidMetricException =
                new InvalidMetricException(
                    message: "Invalid metric. Please correct the errors and try again.");

            invalidMetricException.AddData(
                key: nameof(IMetric.Method),
                values: "Text exceeds max length of 255 characters");

            invalidMetricException.AddData(
                key: nameof(IMetric.Name),
                values: "Text exceeds max length of 255 characters");

            invalidMetricException.AddData(
                key: nameof(IMetric.Target),
                values: "Text exceeds max length of 255 characters");

            invalidMetricException.AddData(
                key: nameof(IMetric.ErrorCode),
                values: "Text exceeds max length of 100 characters");

            invalidMetricException.AddData(
                key: nameof(IMetric.Consumer),
                values: "Text exceeds max length of 255 characters");

            invalidMetricException.AddData(
                key: nameof(IMetric.Description),
                values: "Text exceeds max length of 1000 characters");

            var expectedMetricValidationException =
                new MetricValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: invalidMetricException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<IMetric> addMetricTask =
                this.metricService.AddMetricAsync(invalidMetric, TestContext.Current.CancellationToken);

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
                broker.InsertMetricAsync(It.IsAny<IMetric>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfTypeOrStatusIsUndefinedAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            IMetric randomMetric = CreateRandomMetric(randomDateTimeOffset);
            IMetric invalidMetric = randomMetric;
            invalidMetric.Type = (MetricType)GetRandomNegativeNumber();
            invalidMetric.Status = (MetricStatus)GetRandomNegativeNumber();

            var invalidMetricException =
                new InvalidMetricException(
                    message: "Invalid metric. Please correct the errors and try again.");

            invalidMetricException.AddData(
                key: nameof(IMetric.Type),
                values: "Type is invalid");

            invalidMetricException.AddData(
                key: nameof(IMetric.Status),
                values: "Status is invalid");

            var expectedMetricValidationException =
                new MetricValidationException(
                    message: "Metric validation errors occurred, please try again.",
                    innerException: invalidMetricException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<IMetric> addMetricTask =
                this.metricService.AddMetricAsync(invalidMetric, TestContext.Current.CancellationToken);

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
                broker.InsertMetricAsync(It.IsAny<IMetric>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
