// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Manage.Tests.Acceptance.Models.Metrics;

namespace LondonFhirService.Manage.Tests.Acceptance.Apis.Metrics
{
    public partial class MetricApiTests
    {
        [Fact]
        public async Task ShouldNotAcceptOrReturnTheTransportOnlySpanIdOverJsonAsync()
        {
            // given
            Metric randomMetric = CreateRandomMetric();
            string injectedSpanId = Guid.NewGuid().ToString("N")[..16];

            string metricJson = JsonSerializer.Serialize(randomMetric);

            // Hand-written rather than typed, because the acceptance model has no such property -
            // the point is what the HOST does with a field a caller invents.
            string metricJsonWithSpanId =
                metricJson.Insert(1, $"\"requestSpanId\":\"{injectedSpanId}\",");

            // when
            HttpResponseMessage response =
                await this.apiBroker.PostRawMetricAsync(metricJsonWithSpanId);

            string responseBody = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            // then
            // The field is transport only: it rides in-process from the request that produced a
            // span to the worker that replays it, and EF gives it no column. Left on the public
            // entity, the convention binder would accept whatever a caller sent and echo it back
            // on the created resource, while a later GET returned null - a value the API appears
            // to take and then silently drops.
            response.IsSuccessStatusCode.Should().BeTrue();
            responseBody.Should().NotContain("requestSpanId");
            responseBody.Should().NotContain(injectedSpanId);

            Metric createdMetric = JsonSerializer.Deserialize<Metric>(
                responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Metric persistedMetric = await this.apiBroker.GetMetricByIdAsync(createdMetric.Id);
            persistedMetric.Should().NotBeNull();

            await this.apiBroker.DeleteMetricByIdAsync(createdMetric.Id);
        }

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
