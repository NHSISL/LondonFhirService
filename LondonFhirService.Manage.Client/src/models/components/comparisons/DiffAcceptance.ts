import type { ComparisonSide } from "../../../helpers/comparisons/diffHighlighting";

// Everything the card tree needs to show a difference's acceptance and to change it. It travels
// as one value because it reaches all the way down to a medication's dosage row, and threading
// four separate props through that tree would bury what each component actually decides.
export type DiffAcceptance = {
    side: ComparisonSide;

    // True while a tick is being written. Every tick rewrites the whole stored result, so they are
    // taken one at a time and the boxes are disabled in between.
    saving: boolean;

    onToggleAcceptance: (diffIndexes: number[], acceptable: boolean) => void;
};
