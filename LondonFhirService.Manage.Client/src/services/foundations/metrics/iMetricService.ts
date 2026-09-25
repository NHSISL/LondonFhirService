import type { Metric } from "../../../models/foundations/metrics/Metric";
import type { MetricAverages } from "../../../models/foundations/metrics/MetricAverages";
import type { MetricFilter } from "../../../models/foundations/metrics/MetricFilter";
import type { MetricQuery } from "../../../models/foundations/metrics/MetricQuery";

export interface IMetricService {
    retrieveRequestMetricsAsync(
        metricQuery: MetricQuery,
        metricFilter: MetricFilter,
        abortSignal?: AbortSignal): Promise<Metric[]>;

    retrieveMetricAveragesAsync(abortSignal?: AbortSignal): Promise<MetricAverages>;

    retrieveProviderRequestsMetricsByCorrelationIdsAsync(
        correlationIds: string[],
        abortSignal?: AbortSignal): Promise<Metric[]>;

    retrieveMetricExportAsync(
        metricFilter: MetricFilter,
        abortSignal?: AbortSignal): Promise<Blob>;

    retrieveMetricsByCorrelationIdAsync(
        correlationId: string,
        metricQuery: MetricQuery,
        abortSignal?: AbortSignal): Promise<Metric[]>;
}
