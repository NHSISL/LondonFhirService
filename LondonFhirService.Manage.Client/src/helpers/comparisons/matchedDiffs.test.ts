import { expect, it } from "vitest";
import { getAmbiguousDiffs, getMatchedDiffs } from "./matchedDiffs";
import type { DiffItemView } from "../../models/views/comparisons/DiffItemView";

const createDiff = (overrides: Partial<DiffItemView>): DiffItemView => ({
    key: "0-path",
    index: 0,
    type: "modified",
    typeText: "Modified",
    typeClassName: "badge bg-warning text-dark",
    path: "$.Observation[58].status",
    oldValueText: null,
    newValueText: null,
    resourceTypeText: "Observation",
    identifierText: "58",
    reasonText: null,
    acceptableDiff: false,
    ...overrides
});

it("should return every difference recorded against the matched key", () => {
    const diffs = [createDiff({ key: "0", index: 0 })];

    expect(getMatchedDiffs(diffs, "58")).toEqual(diffs);
});

it("should return nothing without a match key", () => {
    const diffs = [createDiff({ key: "0", index: 0 })];

    expect(getMatchedDiffs(diffs, null)).toEqual([]);
});

// A provider can reuse the same identifier - the same DDS id - across more than one resource of
// a type. The engine then cannot pair either of those resources with a counterpart, and records
// that as manual-review-required against the shared identifier - not as a difference on whatever
// resource happens to compute that identifier as its own match key. Folding one of those into an
// item's badge would let that item's checkbox accept a difference that belongs to an unrelated
// resource the card never shows at all.
it("should exclude a manual-review-required entry even when its identifier matches", () => {
    const diffs = [
        createDiff({ key: "0", index: 0, type: "modified" }),

        createDiff({
            key: "1",
            index: 1,
            type: "manual-review-required",
            reasonText: "Match key '58' not found in Source2"
        })
    ];

    expect(getMatchedDiffs(diffs, "58").map(diff => diff.key)).toEqual(["0"]);
});

// The counterpart to getMatchedDiffs: a caller that wants to show the manual-review-required
// entries a resource's own key produced, rather than silently drop them, reads them from here
// instead - kept out of getMatchedDiffs so a plain difference count never mixes an unresolved
// ambiguity in with a confirmed one.
it("should return only the manual-review-required entries for the matched key", () => {
    const diffs = [
        createDiff({ key: "0", index: 0, type: "modified" }),

        createDiff({
            key: "1",
            index: 1,
            type: "manual-review-required",
            reasonText: "Match key '58' not found in Source2"
        })
    ];

    expect(getAmbiguousDiffs(diffs, "58").map(diff => diff.key)).toEqual(["1"]);
});

it("should return nothing from getAmbiguousDiffs without a match key", () => {
    const diffs = [createDiff({ key: "0", index: 0, type: "manual-review-required" })];

    expect(getAmbiguousDiffs(diffs, null)).toEqual([]);
});
