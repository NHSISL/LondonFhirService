import type { Metric } from "../../../models/foundations/metrics/Metric";
import type { MetricFilter } from "../../../models/foundations/metrics/MetricFilter";
import type { MetricQuery } from "../../../models/foundations/metrics/MetricQuery";

export interface IMetricService {
    retrieveRequestMetricsAsync(
        metricQuery: MetricQuery,
        metricFilter: MetricFilter,
        abortSignal?: AbortSignal): Promise<Metric[]>;

    retrieveProviderRequestsMetricsAsync(
        metricQuery: MetricQuery,
        metricFilter: MetricFilter,
        abortSignal?: AbortSignal): Promise<Metric[]>;

    retrieveMetricsByCorrelationIdAsync(
        correlationId: string,
        metricQuery: MetricQuery,
        abortSignal?: AbortSignal): Promise<Metric[]>;
}
