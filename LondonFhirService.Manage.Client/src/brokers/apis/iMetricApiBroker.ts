import type { Metric } from "../../models/foundations/metrics/Metric";
import type { MetricFilter } from "../../models/foundations/metrics/MetricFilter";
import type { MetricQuery } from "../../models/foundations/metrics/MetricQuery";

export interface IMetricApiBroker {
    getRequestMetricsAsync(
        metricQuery: MetricQuery,
        metricFilter: MetricFilter,
        abortSignal?: AbortSignal): Promise<Metric[]>;

    getProviderRequestsMetricsAsync(
        metricQuery: MetricQuery,
        metricFilter: MetricFilter,
        abortSignal?: AbortSignal): Promise<Metric[]>;

    getMetricsByCorrelationIdAsync(
        correlationId: string,
        metricQuery: MetricQuery,
        abortSignal?: AbortSignal): Promise<Metric[]>;
}
