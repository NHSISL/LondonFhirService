import type { PendingComparisonView } from "../../views/comparisons/PendingComparisonView";

export type PendingComparisonListProps = {
    pendingComparisons: PendingComparisonView[];
    watching: boolean;
    error: Error | null;
};
