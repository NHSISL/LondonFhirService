// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

#nullable enable annotations

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.Metrics;
using LondonFhirService.Core.Models.Processings.Metrics;
using LondonFhirService.Core.Services.Foundations.Metrics;

namespace LondonFhirService.Core.Services.Processings.Metrics
{
    internal partial class MetricProcessingService : IMetricProcessingService
    {
        private readonly IMetricService metricService;
        private readonly ILoggingBroker loggingBroker;

        public MetricProcessingService(
            IMetricService metricService,
            ILoggingBroker loggingBroker)
        {
            this.metricService = metricService;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<IQueryable<MetricExport>> RetrieveRequestMetricExportsAsync(
            Guid? correlationId,
            string? userId,
            DateTimeOffset? fromDate,
            DateTimeOffset? toDate,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            ValidateOnRetrieveRequestMetricExports(correlationId, userId, fromDate, toDate);
            IQueryable<Metric> metrics = await this.metricService.RetrieveAllMetricsAsync(cancellationToken);
            IQueryable<Metric> requestMetrics = metrics.Where(metric => metric.Type == MetricType.Request);

            if (correlationId.HasValue)
            {
                Guid requestCorrelationId = correlationId.Value;

                requestMetrics = requestMetrics.Where(metric =>
                    metric.CorrelationId == requestCorrelationId);
            }

            if (userId is not null)
            {
                string requestUserId = userId;
                requestMetrics = requestMetrics.Where(metric => metric.UserId == requestUserId);
            }

            if (fromDate.HasValue)
            {
                DateTimeOffset createdOnOrAfter = fromDate.Value;
                requestMetrics = requestMetrics.Where(metric => metric.CreatedDate >= createdOnOrAfter);
            }

            if (toDate.HasValue)
            {
                DateTimeOffset createdOnOrBefore = toDate.Value;
                requestMetrics = requestMetrics.Where(metric => metric.CreatedDate <= createdOnOrBefore);
            }

            IQueryable<Metric> providerRequestsMetrics =
                metrics.Where(metric => metric.Type == MetricType.ProviderRequests);

            // A left join, so a request that never reached its providers is still exported - with
            // no provider requests figure - rather than dropped. Composed as one query, so the
            // pairing happens in the database rather than a row at a time here.
            IQueryable<MetricExport> metricExports = requestMetrics
                .GroupJoin(
                    providerRequestsMetrics,
                    requestMetric => requestMetric.CorrelationId,
                    providerRequestsMetric => providerRequestsMetric.CorrelationId,
                    (requestMetric, matches) => new { requestMetric, matches })
                .SelectMany(
                    pair => pair.matches.DefaultIfEmpty(),
                    (pair, providerRequestsMetric) => new MetricExport
                    {
                        Started = pair.requestMetric.Started,
                        CorrelationId = pair.requestMetric.CorrelationId,
                        Method = pair.requestMetric.Method,
                        Name = pair.requestMetric.Name,
                        Status = pair.requestMetric.Status,
                        ErrorCode = pair.requestMetric.ErrorCode,
                        DurationMs = pair.requestMetric.DurationMs,

                        ProviderRequestsMs = providerRequestsMetric == null
                            ? null
                            : providerRequestsMetric.DurationMs,

                        Consumer = pair.requestMetric.Consumer,
                        UserId = pair.requestMetric.UserId
                    })
                .OrderByDescending(metricExport => metricExport.Started);

            return metricExports;
        });
    }
}
