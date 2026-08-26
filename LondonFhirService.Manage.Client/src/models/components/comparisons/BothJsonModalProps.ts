import type { ComparisonSourceView } from "../../views/comparisons/ComparisonSourceView";

export type BothJsonModalProps = {
    show: boolean;
    onHide: () => void;
    primarySource: ComparisonSourceView | null;
    secondarySource: ComparisonSourceView | null;
    syncScrollEnabled: boolean;
    onToggleSyncScroll: () => void;
};
