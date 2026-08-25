import type { MetricCorrelationView } from "../../../models/views/metrics/MetricCorrelationView";
import type { MetricAveragesView } from "../../../models/views/metrics/MetricAveragesView";
import type { MetricFilter } from "../../../models/foundations/metrics/MetricFilter";
import type { MetricPageView } from "../../../models/views/metrics/MetricPageView";

export interface IMetricViewService {
    retrieveMetricPageViewAsync(
        pageNumber: number,
        metricFilter: MetricFilter,
        abortSignal?: AbortSignal): Promise<MetricPageView>;

    retrieveMetricAveragesViewAsync(
        metricFilter: MetricFilter,
        abortSignal?: AbortSignal): Promise<MetricAveragesView>;

    createMetricFilter(): MetricFilter;

    isSearchableCorrelationId(correlationId: string): boolean;

    retrieveMetricCorrelationViewAsync(
        correlationId: string,
        abortSignal?: AbortSignal): Promise<MetricCorrelationView>;
}
