import type { ComparisonListItemView } from "../../views/comparisons/ComparisonListItemView";

export type ComparisonListProps = {
    comparisons: ComparisonListItemView[];
    selectedComparisonId?: string;
};
