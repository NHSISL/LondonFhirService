// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using Moq;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Services.Foundations.Metrics
{
    public partial class MetricServiceTests
    {
        [Fact]
        public async Task ShouldReturnMetricsAsync()
        {
            // given
            IQueryable<IMetric> randomMetrics = CreateRandomMetricsQueryable();
            IQueryable<IMetric> storageMetrics = randomMetrics;
            IQueryable<IMetric> expectedMetrics = storageMetrics;

            this.metricStorageBrokerMock.Setup(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageMetrics);

            // when
            IQueryable<IMetric> actualMetrics =
                await this.metricService.RetrieveAllMetricsAsync(TestContext.Current.CancellationToken);

            // then
            actualMetrics.Should().BeEquivalentTo(expectedMetrics);

            this.metricStorageBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }

        [Fact]
        public async Task ShouldReturnMetricsEvenWhenRecordingIsDisabledAsync()
        {
            // given
            IQueryable<IMetric> randomMetrics = CreateRandomMetricsQueryable();
            IQueryable<IMetric> storageMetrics = randomMetrics;
            IQueryable<IMetric> expectedMetrics = storageMetrics;
            this.metricServiceConfigurations.IsEnabled = false;

            this.metricStorageBrokerMock.Setup(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageMetrics);

            // when
            IQueryable<IMetric> actualMetrics =
                await this.metricService.RetrieveAllMetricsAsync(TestContext.Current.CancellationToken);

            // then
            // The kill switch stops recording, not reading. Metrics already captured stay
            // queryable while recording is turned off.
            actualMetrics.Should().BeEquivalentTo(expectedMetrics);

            this.metricStorageBrokerMock.Verify(broker =>
                broker.SelectAllMetricsAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
