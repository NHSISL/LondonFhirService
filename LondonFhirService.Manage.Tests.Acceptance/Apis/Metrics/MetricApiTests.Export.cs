// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Manage.Tests.Acceptance.Models.Metrics;

namespace LondonFhirService.Manage.Tests.Acceptance.Apis.Metrics
{
    public partial class MetricApiTests
    {
        private const string ExportHeader =
            "StartedUtc,CorrelationId,Method,Name,Status,ErrorCode,DurationMs,"
                + "ProviderRequestsMs,ProxyOverheadMs,Consumer,UserId";

        private static Metric CreateRandomSpan(Guid correlationId, MetricType type, double durationMs)
        {
            Metric randomMetric = CreateRandomMetric();
            randomMetric.CorrelationId = correlationId;
            randomMetric.Type = type;
            randomMetric.DurationMs = durationMs;
            randomMetric.ErrorCode = null;

            return randomMetric;
        }

        private static string[] ReadLines(string csv) =>
            csv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        /// <summary>
        /// Runs the export against the real database, so the left join from each request to its
        /// ProviderRequests span is proven to translate to SQL rather than assumed to.
        /// </summary>
        [Fact]
        public async Task ShouldExportRequestWithItsProxyOverheadAsCsvAsync()
        {
            // given
            Guid correlationId = Guid.NewGuid();

            Metric requestMetric = await this.apiBroker.PostMetricAsync(
                CreateRandomSpan(correlationId, MetricType.Request, durationMs: 500));

            Metric providerRequestsMetric = await this.apiBroker.PostMetricAsync(
                CreateRandomSpan(correlationId, MetricType.ProviderRequests, durationMs: 380));

            // when
            string csv = await this.apiBroker.GetMetricExportAsync($"?correlationId={correlationId}");

            // then
            string[] lines = ReadLines(csv);
            lines.Should().HaveCount(2);
            lines[0].Should().Be(ExportHeader);

            string[] fields = lines[1].Split(',');
            fields[1].Should().Be(correlationId.ToString());
            fields[2].Should().Be(requestMetric.Method);
            fields[4].Should().Be(nameof(MetricStatus.Succeeded));
            fields[6].Should().Be("500");
            fields[7].Should().Be("380");
            fields[8].Should().Be("120");

            await this.apiBroker.DeleteMetricByIdAsync(providerRequestsMetric.Id);
            await this.apiBroker.DeleteMetricByIdAsync(requestMetric.Id);
        }

        [Fact]
        public async Task ShouldExportRequestThatNeverReachedItsProvidersWithNoOverheadAsync()
        {
            // given
            Guid correlationId = Guid.NewGuid();

            Metric requestMetric = await this.apiBroker.PostMetricAsync(
                CreateRandomSpan(correlationId, MetricType.Request, durationMs: 40));

            // when
            string csv = await this.apiBroker.GetMetricExportAsync($"?correlationId={correlationId}");

            // then
            string[] fields = ReadLines(csv)[1].Split(',');
            fields[6].Should().Be("40");
            fields[7].Should().BeEmpty();
            fields[8].Should().BeEmpty();

            await this.apiBroker.DeleteMetricByIdAsync(requestMetric.Id);
        }

        [Fact]
        public async Task ShouldExportOnlyRequestsCreatedWithinTheDateRangeAsync()
        {
            // given
            Guid correlationId = Guid.NewGuid();

            Metric requestMetric = await this.apiBroker.PostMetricAsync(
                CreateRandomSpan(correlationId, MetricType.Request, durationMs: 100));

            string inRange =
                $"?correlationId={correlationId}"
                    + $"&fromDate={Uri.EscapeDataString(DateTimeOffset.UtcNow.AddDays(-1).ToString("o"))}"
                    + $"&toDate={Uri.EscapeDataString(DateTimeOffset.UtcNow.AddDays(1).ToString("o"))}";

            string outOfRange =
                $"?correlationId={correlationId}"
                    + $"&fromDate={Uri.EscapeDataString(DateTimeOffset.UtcNow.AddDays(1).ToString("o"))}";

            // when
            string inRangeCsv = await this.apiBroker.GetMetricExportAsync(inRange);
            string outOfRangeCsv = await this.apiBroker.GetMetricExportAsync(outOfRange);

            // then
            ReadLines(inRangeCsv).Should().HaveCount(2);

            // Header only: an empty export is still a well formed file.
            ReadLines(outOfRangeCsv).Should().Equal(ExportHeader);

            await this.apiBroker.DeleteMetricByIdAsync(requestMetric.Id);
        }

        [Fact]
        public async Task ShouldExportOnlyRequestsMadeByTheGivenUserAsync()
        {
            // given
            Guid correlationId = Guid.NewGuid();

            // The user id is stamped by the metric service from the caller, not taken from the
            // posted body, so the filter is exercised with whatever the host stamped.
            Metric requestMetric = await this.apiBroker.PostMetricAsync(
                CreateRandomSpan(correlationId, MetricType.Request, durationMs: 100));

            string sameUser =
                $"?correlationId={correlationId}&userId={Uri.EscapeDataString(requestMetric.UserId)}";

            string otherUser =
                $"?correlationId={correlationId}&userId={Guid.NewGuid()}";

            // when
            string sameUserCsv = await this.apiBroker.GetMetricExportAsync(sameUser);
            string otherUserCsv = await this.apiBroker.GetMetricExportAsync(otherUser);

            // then
            ReadLines(sameUserCsv).Should().HaveCount(2);
            ReadLines(sameUserCsv)[1].Should().EndWith($",{requestMetric.UserId}");
            ReadLines(otherUserCsv).Should().Equal(ExportHeader);

            await this.apiBroker.DeleteMetricByIdAsync(requestMetric.Id);
        }

        [Fact]
        public async Task ShouldReturnBadRequestOnExportIfDateRangeRunsBackwardsAsync()
        {
            // given
            string backwards =
                $"?fromDate={Uri.EscapeDataString(DateTimeOffset.UtcNow.ToString("o"))}"
                    + $"&toDate={Uri.EscapeDataString(DateTimeOffset.UtcNow.AddDays(-1).ToString("o"))}";

            // when
            HttpResponseMessage response = await this.apiBroker.GetMetricExportResponseAsync(backwards);

            // then
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
