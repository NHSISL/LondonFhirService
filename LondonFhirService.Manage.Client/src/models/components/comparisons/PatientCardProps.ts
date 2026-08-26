import type { CardExpansion } from "./CardExpansion";
import type { ComparisonSourceView } from "../../views/comparisons/ComparisonSourceView";
import type { DiffAcceptance } from "./DiffAcceptance";
import type { DiffItemView } from "../../views/comparisons/DiffItemView";

export type PatientCardProps = {
    source: ComparisonSourceView;
    diffs: DiffItemView[];
    acceptance: DiffAcceptance;

    // Expansion is owned by the page rather than by each card, so that with syncing on the two
    // sides open and close together and a difference stays opposite its counterpart.
    expansion: CardExpansion;
};
