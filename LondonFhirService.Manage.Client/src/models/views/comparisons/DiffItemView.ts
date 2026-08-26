import type { DiffItemType } from "../../foundations/comparisons/DiffItem";

// One difference, ready to render. The raw values are kept as the comparison engine wrote them -
// they are JSON fragments, and rewriting them would hide the very thing being compared - but
// everything the markup needs to choose a colour or a wording is decided here.
export type DiffItemView = {
    key: string;

    // Position in the stored result's diffs array. Acceptance is written back by index, because a
    // difference has no id of its own and the engine can write the same path more than once.
    index: number;

    type: DiffItemType;
    typeText: string;
    typeClassName: string;
    path: string;
    oldValueText: string | null;
    newValueText: string | null;
    resourceTypeText: string | null;
    identifierText: string | null;
    reasonText: string | null;
    acceptableDiff: boolean;
};
