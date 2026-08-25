// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Manage.Tests.Acceptance.Models.Metrics;

namespace LondonFhirService.Manage.Tests.Acceptance.Apis.Metrics
{
    /// <summary>
    /// Create and delete carry [InvisibleApi]. The middleware answers 404 rather than 401 or 403
    /// to anyone without the key header, so the endpoint does not advertise that it exists.
    ///
    /// Success here is the block. There is no PUT on this controller to cover.
    /// </summary>
    public partial class MetricApiTests
    {
        [Fact]
        public async Task ShouldBlockPostMetricWithoutTheInvisibleApiKeyAsync()
        {
            // given
            Metric randomMetric = CreateRandomMetric();

            // when
            var response = await this.apiBroker.PostMetricWithoutKeyAsync(randomMetric);

            // then
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ShouldBlockDeleteMetricWithoutTheInvisibleApiKeyAsync()
        {
            // given
            Guid randomId = Guid.NewGuid();

            // when
            var response = await this.apiBroker.DeleteMetricByIdWithoutKeyAsync(randomId);

            // then
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ShouldBlockDeleteOfAnExistingMetricWithoutTheInvisibleApiKeyAsync()
        {
            // given
            Metric randomMetric = await PostRandomMetricAsync();

            // when
            var response = await this.apiBroker.DeleteMetricByIdWithoutKeyAsync(randomMetric.Id);

            // then
            // The row exists, so a routable endpoint would have deleted it. 404 here is the
            // middleware hiding the endpoint, not the service failing to find the row.
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            Metric stillThere = await this.apiBroker.GetMetricByIdAsync(randomMetric.Id);
            stillThere.Should().NotBeNull();

            await this.apiBroker.DeleteMetricByIdAsync(randomMetric.Id);
        }
    }
}
