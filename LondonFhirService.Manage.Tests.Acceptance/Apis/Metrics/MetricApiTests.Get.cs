// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Manage.Tests.Acceptance.Models.Metrics;

namespace LondonFhirService.Manage.Tests.Acceptance.Apis.Metrics
{
    public partial class MetricApiTests
    {
        [Fact]
        public async Task ShouldGetAllMetricsAsync()
        {
            // given
            List<Metric> randomMetrics = await PostRandomMetricsAsync();
            List<Metric> expectedMetrics = randomMetrics;

            // when
            List<Metric> actualMetrics = await this.apiBroker.GetAllMetricsAsync();

            // then
            foreach (Metric expectedMetric in expectedMetrics)
            {
                Metric actualMetric = actualMetrics.Single(metric => metric.Id == expectedMetric.Id);

                actualMetric.Should().BeEquivalentTo(expectedMetric, options => options
                    .Excluding(property => property.CreatedDate));

                await this.apiBroker.DeleteMetricByIdAsync(actualMetric.Id);
            }
        }

        [Fact]
        public async Task ShouldGetMetricByIdAsync()
        {
            // given
            Metric randomMetric = await PostRandomMetricAsync();
            Metric expectedMetric = randomMetric;

            // when
            Metric actualMetric = await this.apiBroker.GetMetricByIdAsync(randomMetric.Id);

            // then
            actualMetric.Should().BeEquivalentTo(expectedMetric, options => options
                .Excluding(property => property.CreatedDate));

            await this.apiBroker.DeleteMetricByIdAsync(actualMetric.Id);
        }

        [Fact]
        public async Task ShouldGetAllMetricsWithoutTheInvisibleApiKeyAsync()
        {
            // given
            Metric randomMetric = await PostRandomMetricAsync();

            // when
            var response = await this.apiBroker.GetAllMetricsWithoutKeyAsync();

            // then
            // Reads are not hidden - only create and delete are.
            response.IsSuccessStatusCode.Should().BeTrue();

            await this.apiBroker.DeleteMetricByIdAsync(randomMetric.Id);
        }
    }
}
