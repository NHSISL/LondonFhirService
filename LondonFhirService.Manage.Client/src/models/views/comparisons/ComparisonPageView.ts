import type { ComparisonListItemView } from "./ComparisonListItemView";

// One page of comparison rows, plus whether the API had more to give. Comparisons accumulate one
// row per correlation that was compared, so the list pages server side rather than loading
// everything.
export type ComparisonPageView = {
    comparisons: ComparisonListItemView[];
    hasMore: boolean;
};
