import type { MetricListItemView } from "./MetricListItemView";

// One page of root request spans, plus whether the API had more to give.
export type MetricPageView = {
    metrics: MetricListItemView[];
    hasMore: boolean;
};
