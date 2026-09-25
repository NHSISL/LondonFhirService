import type { MetricCorrelationView } from "../../../models/views/metrics/MetricCorrelationView";
import type { MetricAveragesView } from "../../../models/views/metrics/MetricAveragesView";
import type { MetricFilter } from "../../../models/foundations/metrics/MetricFilter";
import type { MetricListItemView } from "../../../models/views/metrics/MetricListItemView";
import type { MetricPageView } from "../../../models/views/metrics/MetricPageView";

export interface IMetricViewService {
    retrieveMetricPageViewAsync(
        pageNumber: number,
        metricFilter: MetricFilter,
        abortSignal?: AbortSignal): Promise<MetricPageView>;

    retrieveAllMetricAveragesViewAsync(abortSignal?: AbortSignal): Promise<MetricAveragesView>;

    buildLoadedMetricAveragesView(metrics: MetricListItemView[]): MetricAveragesView;

    createMetricFilter(): MetricFilter;

    exportMetricsAsync(metricFilter: MetricFilter, abortSignal?: AbortSignal): Promise<void>;

    isSearchableCorrelationId(correlationId: string): boolean;

    retrieveMetricCorrelationViewAsync(
        correlationId: string,
        abortSignal?: AbortSignal): Promise<MetricCorrelationView>;
}
