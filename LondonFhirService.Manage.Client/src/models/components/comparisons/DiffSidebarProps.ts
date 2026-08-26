import type { DiffItemView } from "../../views/comparisons/DiffItemView";

export type DiffSidebarProps = {
    show: boolean;
    onHide: () => void;
    diffs: DiffItemView[];
    correlationId: string;
    acceptanceSaving: boolean;
    acceptanceError: Error | null;
    onToggleDiffAcceptance: (diffIndexes: number[], acceptable: boolean) => void;
};

export type DiffItemProps = {
    diff: DiffItemView;
};
