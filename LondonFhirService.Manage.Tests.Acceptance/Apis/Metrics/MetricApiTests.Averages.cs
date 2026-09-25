// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Manage.Tests.Acceptance.Models.Metrics;

namespace LondonFhirService.Manage.Tests.Acceptance.Apis.Metrics
{
    public partial class MetricApiTests
    {
        /// <summary>
        /// The all requests tile asks the database for its averages with $apply, so the expression
        /// has to parse, translate to SQL and come back in the shape the client reads. The suite
        /// shares one database, so this asserts that its own spans moved the figures rather than
        /// what the figures are.
        /// </summary>
        [Fact]
        public async Task ShouldAverageEveryRequestAndProviderRequestsSpanByTypeAsync()
        {
            // given
            (int requestCount, int providerRequestsCount) before = ReadSpanCounts(
                await this.apiBroker.GetMetricAveragesAsync());

            Guid correlationId = Guid.NewGuid();

            Metric requestMetric = await this.apiBroker.PostMetricAsync(
                CreateRandomSpan(correlationId, MetricType.Request, durationMs: 500));

            Metric providerRequestsMetric = await this.apiBroker.PostMetricAsync(
                CreateRandomSpan(correlationId, MetricType.ProviderRequests, durationMs: 380));

            Metric otherMetric = await this.apiBroker.PostMetricAsync(
                CreateRandomSpan(correlationId, MetricType.Provider, durationMs: 9999));

            // when
            string averagesJson = await this.apiBroker.GetMetricAveragesAsync();

            // then
            (int requestCount, int providerRequestsCount) after = ReadSpanCounts(averagesJson);
            after.requestCount.Should().Be(before.requestCount + 1);
            after.providerRequestsCount.Should().Be(before.providerRequestsCount + 1);

            using JsonDocument averages = JsonDocument.Parse(averagesJson);

            // Only the two span types the tile averages come back - the Provider span is filtered.
            averages.RootElement.EnumerateArray()
                .Select(row => row.GetProperty("type").GetInt32())
                .Should().BeSubsetOf(new[] { (int)MetricType.Request, (int)MetricType.ProviderRequests });

            averages.RootElement.EnumerateArray()
                .Should().OnlyContain(row => row.GetProperty("averageDurationMs").ValueKind == JsonValueKind.Number);

            await this.apiBroker.DeleteMetricByIdAsync(otherMetric.Id);
            await this.apiBroker.DeleteMetricByIdAsync(providerRequestsMetric.Id);
            await this.apiBroker.DeleteMetricByIdAsync(requestMetric.Id);
        }

        private static (int requestCount, int providerRequestsCount) ReadSpanCounts(string averagesJson)
        {
            using JsonDocument averages = JsonDocument.Parse(averagesJson);

            int CountOf(MetricType metricType) => averages.RootElement.EnumerateArray()
                .Where(row => row.GetProperty("type").GetInt32() == (int)metricType)
                .Select(row => row.GetProperty("spanCount").GetInt32())
                .SingleOrDefault();

            return (CountOf(MetricType.Request), CountOf(MetricType.ProviderRequests));
        }
    }
}
