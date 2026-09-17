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
        public async Task ShouldNotAdvertiseTheTransportOnlySpanIdInTheEdmAsync()
        {
            // given . when
            string metadata = await this.apiBroker.GetODataMetadataAsync();

            // then
            // RequestSpanId rides from the request that produced a metric to the worker that
            // replays it into telemetry; EF ignores it rather than giving it a column. The
            // convention builder reflects over every public property, so it has to be excluded by
            // hand - otherwise $metadata advertises a field that is always null, and a $filter or
            // $orderby against it reaches EF with no column to translate to and fails the request.
            metadata.Should().NotContain("requestSpanId");

            // The rest of the entity is still there, so this is proof of an exclusion rather than
            // of a metadata document that failed to render.
            metadata.Should().Contain("correlationId");
        }

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
