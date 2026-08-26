import type { ComparisonDetailView } from "../../views/comparisons/ComparisonDetailView";

export type SideBySideViewerProps = {
    comparison: ComparisonDetailView;
    syncScrollEnabled: boolean;
    showPatientDetails: boolean;
    onShowPatientDetails: (showPatientDetails: boolean) => void;
    expandedLists: Set<string>;
    setExpandedLists: (expandedLists: Set<string>) => void;
    expandedItems: Set<string>;
    setExpandedItems: (expandedItems: Set<string>) => void;
    acceptanceSaving: boolean;
    onToggleDiffAcceptance: (diffIndexes: number[], acceptable: boolean) => void;
};
