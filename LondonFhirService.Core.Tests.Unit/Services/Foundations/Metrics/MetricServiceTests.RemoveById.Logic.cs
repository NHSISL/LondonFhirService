// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

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
        public async Task ShouldRemoveMetricByIdAsync()
        {
            // given
            Metric randomMetric = CreateRandomMetric();
            Metric storageMetric = randomMetric;
            Metric deletedMetric = storageMetric;
            Metric expectedMetric = deletedMetric.DeepClone();

            this.storageBrokerMock.Setup(broker =>
                broker.SelectMetricByIdAsync(randomMetric.Id))
                    .ReturnsAsync(storageMetric);

            this.storageBrokerMock.Setup(broker =>
                broker.DeleteMetricAsync(storageMetric))
                    .ReturnsAsync(deletedMetric);

            // when
            Metric actualMetric = await this.metricService.RemoveMetricByIdAsync(randomMetric.Id);

            // then
            actualMetric.Should().BeEquivalentTo(expectedMetric);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectMetricByIdAsync(randomMetric.Id),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteMetricAsync(storageMetric),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
