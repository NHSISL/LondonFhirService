import type { DiffItemView } from "../../views/comparisons/DiffItemView";

export type DiffSidebarProps = {
    show: boolean;
    onHide: () => void;
    diffs: DiffItemView[];
    correlationId: string;
};

export type DiffItemProps = {
    diff: DiffItemView;
};
