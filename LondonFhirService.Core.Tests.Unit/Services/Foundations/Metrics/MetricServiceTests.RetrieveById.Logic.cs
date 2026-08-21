// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

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
        public async Task ShouldRetrieveMetricByIdAsync()
        {
            // given
            Metric randomMetric = CreateRandomMetric();
            Metric storageMetric = randomMetric;
            Metric expectedMetric = storageMetric.DeepClone();

            this.storageBrokerMock.Setup(broker =>
                broker.SelectMetricByIdAsync(randomMetric.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageMetric);

            // when
            Metric actualMetric =
                await this.metricService.RetrieveMetricByIdAsync(
                    randomMetric.Id, TestContext.Current.CancellationToken);

            // then
            actualMetric.Should().BeEquivalentTo(expectedMetric);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectMetricByIdAsync(randomMetric.Id, It.IsAny<CancellationToken>()),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
