// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using Moq;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Services.Foundations.Metrics
{
    public partial class MetricServiceTests
    {
        [Fact]
        public async Task ShouldRemoveMetricByIdAsync()
        {
            // given
            IMetric randomMetric = CreateRandomMetric();
            IMetric storageMetric = randomMetric;
            IMetric deletedMetric = storageMetric;
            IMetric expectedMetric = deletedMetric.DeepClone();

            this.storageBrokerMock.Setup(broker =>
                broker.SelectMetricByIdAsync(randomMetric.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageMetric);

            this.storageBrokerMock.Setup(broker =>
                broker.DeleteMetricAsync(storageMetric, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(deletedMetric);

            // when
            IMetric actualMetric =
                await this.metricService.RemoveMetricByIdAsync(randomMetric.Id, TestContext.Current.CancellationToken);

            // then
            actualMetric.Should().BeEquivalentTo(expectedMetric);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectMetricByIdAsync(randomMetric.Id, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteMetricAsync(storageMetric, It.IsAny<CancellationToken>()),
                    Times.Once);

            VerifyNoOtherCallsOnAllBrokers();
        }
    }
}
