// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LondonFhirService.Manage.Tests.Acceptance.Models.Metrics;

namespace LondonFhirService.Manage.Tests.Acceptance.Brokers
{
    public partial class ApiBroker
    {
        private const string metricsRelativeUrl = "api/metrics";

        public async ValueTask<Metric> PostMetricAsync(Metric metric) =>
            await this.apiFactoryClient.PostContentAsync(metricsRelativeUrl, metric);

        public async ValueTask<List<Metric>> GetAllMetricsAsync() =>
            await this.apiFactoryClient.GetContentAsync<List<Metric>>($"{metricsRelativeUrl}/");

        public async ValueTask<Metric> GetMetricByIdAsync(Guid metricId) =>
            await this.apiFactoryClient.GetContentAsync<Metric>($"{metricsRelativeUrl}/{metricId}");

        public async ValueTask<Metric> DeleteMetricByIdAsync(Guid metricId) =>
            await this.apiFactoryClient.DeleteContentAsync<Metric>($"{metricsRelativeUrl}/{metricId}");

        // Keyless: what a caller without the invisible-api header actually gets. There is no PUT
        // on this controller - a metric is an append-only span.
        public async ValueTask<HttpResponseMessage> PostMetricWithoutKeyAsync(Metric metric) =>
            await this.keylessHttpClient.PostAsJsonAsync(metricsRelativeUrl, metric);

        public async ValueTask<HttpResponseMessage> DeleteMetricByIdWithoutKeyAsync(Guid metricId) =>
            await this.keylessHttpClient.DeleteAsync($"{metricsRelativeUrl}/{metricId}");

        public async ValueTask<HttpResponseMessage> GetAllMetricsWithoutKeyAsync() =>
            await this.keylessHttpClient.GetAsync($"{metricsRelativeUrl}/");
    }
}
