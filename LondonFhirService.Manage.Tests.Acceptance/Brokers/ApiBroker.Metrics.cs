// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
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

        /// <summary>
        /// The CSV export, read as text: what is asserted is the file a person would open.
        /// </summary>
        public async ValueTask<string> GetMetricExportAsync(string queryString) =>
            await this.httpClient.GetStringAsync($"{metricsRelativeUrl}/exports{queryString}");

        public async ValueTask<HttpResponseMessage> GetMetricExportResponseAsync(string queryString) =>
            await this.httpClient.GetAsync($"{metricsRelativeUrl}/exports{queryString}");

        /// <summary>
        /// The raw EDM document. Read as text rather than parsed, because what is being asserted
        /// is which properties the host advertises at all.
        /// </summary>
        public async ValueTask<string> GetODataMetadataAsync() =>
            await this.httpClient.GetStringAsync("odata/$metadata");

        /// <summary>
        /// Posts hand-written JSON rather than a typed model, so a test can send a field the
        /// typed model does not have and see what the host does with it.
        /// </summary>
        public async ValueTask<HttpResponseMessage> PostRawMetricAsync(string json)
        {
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await this.httpClient.PostAsync(metricsRelativeUrl, content);
        }
    }
}
