// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.Metrics;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Metrics
{
    public partial class MetricServiceTests
    {
        [Fact]
        public async Task ShouldAddMetricAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Metric randomMetric = CreateRandomMetric(randomDateTimeOffset);
            Metric inputMetric = randomMetric;
            Metric storageMetric = inputMetric.DeepClone();
            Metric expectedMetric = storageMetric.DeepClone();

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertMetricAsync(inputMetric, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageMetric);

            // when
            Metric actualMetric =
                await this.metricService.AddMetricAsync(inputMetric, TestContext.Current.CancellationToken);

            // then
            actualMetric.Should().BeEquivalentTo(expectedMetric);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertMetricAsync(inputMetric, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(storageMetric, It.IsAny<CancellationToken>()),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldStampCreatedDateFromDateTimeBrokerOnAddMetricAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset currentDateTimeOffset = randomDateTimeOffset.AddMinutes(GetRandomNumber());
            Metric randomMetric = CreateRandomMetric(randomDateTimeOffset);
            Metric inputMetric = randomMetric;
            Metric storageMetric = inputMetric.DeepClone();

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(currentDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertMetricAsync(inputMetric, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageMetric);

            // when
            await this.metricService.AddMetricAsync(inputMetric, TestContext.Current.CancellationToken);

            // then
            inputMetric.CreatedDate.Should().Be(currentDateTimeOffset);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            // The exact metric, so a service that persisted a different instance could not
            // pass on the timestamp predicate alone.
            this.storageBrokerMock.Verify(broker =>
                broker.InsertMetricAsync(inputMetric, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(storageMetric, It.IsAny<CancellationToken>()),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldNotAddMetricIfRecordingIsDisabledAsync()
        {
            // given
            Metric randomMetric = CreateRandomMetric();
            Metric inputMetric = randomMetric;
            Metric expectedMetric = inputMetric.DeepClone();
            this.metricServiceConfigurations.IsEnabled = false;

            // when
            Metric actualMetric =
                await this.metricService.AddMetricAsync(inputMetric, TestContext.Current.CancellationToken);

            // then
            actualMetric.Should().BeEquivalentTo(expectedMetric);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertMetricAsync(It.IsAny<Metric>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.metricBrokerMock.Verify(broker =>
                broker.RecordAsync(It.IsAny<Metric>(), It.IsAny<CancellationToken>()),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldNotValidateWhenRecordingIsDisabledOnAddMetricAsync()
        {
            // given
            Metric nullMetric = null;
            this.metricServiceConfigurations.IsEnabled = false;

            // when
            Metric actualMetric =
                await this.metricService.AddMetricAsync(nullMetric, TestContext.Current.CancellationToken);

            // then
            actualMetric.Should().BeNull();

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
                    Times.Never);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
