import type { ComparisonSourceView } from "../../views/comparisons/ComparisonSourceView";
import type { DiffAcceptance } from "./DiffAcceptance";
import type { DiffItemView } from "../../views/comparisons/DiffItemView";

export type PatientCardProps = {
    source: ComparisonSourceView;
    diffs: DiffItemView[];
    acceptance: DiffAcceptance;

    // Expansion is owned by the page rather than by each card, so the two sides open and close
    // together and a difference stays opposite its counterpart.
    showPatientDetails: boolean;
    onShowPatientDetails: (showPatientDetails: boolean) => void;
    expandedLists: Set<string>;
    setExpandedLists: (expandedLists: Set<string>) => void;
    expandedItems: Set<string>;
    setExpandedItems: (expandedItems: Set<string>) => void;
};
