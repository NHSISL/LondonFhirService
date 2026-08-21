// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

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
        public async Task ShouldReturnMetricsAsync()
        {
            // given
            IQueryable<Metric> randomMetrics = CreateRandomMetricsQueryable();
            IQueryable<Metric> storageMetrics = randomMetrics;
            IQueryable<Metric> expectedMetrics = storageMetrics;

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllMetricsAsync())
                    .ReturnsAsync(storageMetrics);

            // when
            IQueryable<Metric> actualMetrics = await this.metricService.RetrieveAllMetricsAsync();

            // then
            actualMetrics.Should().BeEquivalentTo(expectedMetrics);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldReturnMetricsEvenWhenRecordingIsDisabledAsync()
        {
            // given
            IQueryable<Metric> randomMetrics = CreateRandomMetricsQueryable();
            IQueryable<Metric> storageMetrics = randomMetrics;
            IQueryable<Metric> expectedMetrics = storageMetrics;
            this.metricServiceConfigurations.IsEnabled = false;

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllMetricsAsync())
                    .ReturnsAsync(storageMetrics);

            // when
            IQueryable<Metric> actualMetrics = await this.metricService.RetrieveAllMetricsAsync();

            // then
            // The kill switch stops recording, not reading. Metrics already captured stay
            // queryable while recording is turned off.
            actualMetrics.Should().BeEquivalentTo(expectedMetrics);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
