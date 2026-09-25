import type { Metric } from "../../models/foundations/metrics/Metric";
import type { MetricAverages } from "../../models/foundations/metrics/MetricAverages";
import type { MetricFilter } from "../../models/foundations/metrics/MetricFilter";
import type { MetricQuery } from "../../models/foundations/metrics/MetricQuery";

export interface IMetricApiBroker {
    getRequestMetricsAsync(
        metricQuery: MetricQuery,
        metricFilter: MetricFilter,
        abortSignal?: AbortSignal): Promise<Metric[]>;

    getMetricAveragesAsync(abortSignal?: AbortSignal): Promise<MetricAverages>;

    getProviderRequestsMetricsByCorrelationIdsAsync(
        correlationIds: string[],
        abortSignal?: AbortSignal): Promise<Metric[]>;

    getMetricExportAsync(
        metricFilter: MetricFilter,
        abortSignal?: AbortSignal): Promise<Blob>;

    getMetricsByCorrelationIdAsync(
        correlationId: string,
        metricQuery: MetricQuery,
        abortSignal?: AbortSignal): Promise<Metric[]>;
}
