import type { ComparisonListItemView } from "../../views/comparisons/ComparisonListItemView";

export type ComparisonListProps = {
    comparisons: ComparisonListItemView[];
    selectedComparisonId?: string;

    // What the empty state is allowed to claim. An empty table means something different when the
    // operator typed a correlation id than when they arrived at the page, and different again when
    // the records are sitting in the queue right above it.
    searchTerm?: string;
    pendingCount?: number;
};
