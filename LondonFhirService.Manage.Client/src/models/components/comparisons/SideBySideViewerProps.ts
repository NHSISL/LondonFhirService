import type { ComparisonDetailView } from "../../views/comparisons/ComparisonDetailView";
import type { ComparisonSide } from "../../../helpers/comparisons/diffHighlighting";
import type { SideExpandedKeys } from "../../../hooks/pages/useComparisonDetailPage";

export type SideBySideViewerProps = {
    comparison: ComparisonDetailView;

    // Governs both scrolling and expanding: with it on, the two cards move and open together.
    syncEnabled: boolean;

    expandedKeys: SideExpandedKeys;
    onToggleExpanded: (side: ComparisonSide, expansionKey: string) => void;

    acceptanceSaving: boolean;
    onToggleDiffAcceptance: (diffIndexes: number[], acceptable: boolean) => void;
};
