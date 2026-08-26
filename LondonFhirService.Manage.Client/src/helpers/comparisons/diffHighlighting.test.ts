import { expect, it } from "vitest";
import { getDiffState, getDiffsForField, getHighlightStyle } from "./diffHighlighting";
import type { DiffItemView } from "../../models/views/comparisons/DiffItemView";

const createDiff = (overrides: Partial<DiffItemView>): DiffItemView => ({
    key: "0-path",
    index: 0,
    type: "modified",
    typeText: "Modified",
    typeClassName: "badge bg-warning text-dark",
    path: "gender",
    oldValueText: null,
    newValueText: null,
    resourceTypeText: "Patient",
    identifierText: null,
    reasonText: null,
    acceptableDiff: false,
    ...overrides
});

it("should map the tail of a path onto the card field it belongs to", () => {
    const diffs = [createDiff({ path: "$.Patient[1].name[0].\"family\"" })];

    expect(getDiffsForField(diffs, "nameFamily", "primary")).toHaveLength(1);
    expect(getDiffsForField(diffs, "nameGiven", "primary")).toHaveLength(0);
});

// The engine compares the primary as source1 and the secondary as source2, so a removal is
// something only the primary has and an addition only the secondary.
it("should show a removal on the primary side and an addition on the secondary", () => {
    const removed = [createDiff({ type: "removed", path: "telecom" })];
    const added = [createDiff({ type: "added", path: "telecom" })];

    expect(getDiffsForField(removed, "telecom", "primary")).toHaveLength(1);
    expect(getDiffsForField(removed, "telecom", "secondary")).toHaveLength(0);
    expect(getDiffsForField(added, "telecom", "secondary")).toHaveLength(1);
    expect(getDiffsForField(added, "telecom", "primary")).toHaveLength(0);
});

it("should show a modification on both sides", () => {
    const diffs = [createDiff({ type: "modified", path: "birthDate" })];

    expect(getDiffsForField(diffs, "birthDate", "primary")).toHaveLength(1);
    expect(getDiffsForField(diffs, "birthDate", "secondary")).toHaveLength(1);
});

it("should return every difference recorded against one field", () => {
    const diffs = [
        createDiff({ index: 0, path: "address[0].city" }),
        createDiff({ index: 1, path: "$.Patient[1].address[0].\"city\"" })
    ];

    expect(getDiffsForField(diffs, "addressCity", "primary").map(diff => diff.index))
        .toEqual([0, 1]);
});

it("should ignore a path with no field it recognises", () => {
    const diffs = [createDiff({ path: "$.Patient[1].meta.versionId" })];

    expect(getDiffsForField(diffs, "birthDate", "primary")).toHaveLength(0);
});

// A field reads as accepted only once every difference in it has been ticked - one box can cover
// more than one difference, and a half accepted field is still outstanding.
it("should read a field as accepted only when all of its differences are", () => {
    expect(getDiffState([])).toBe("none");
    expect(getDiffState([createDiff({ acceptableDiff: false })])).toBe("outstanding");
    expect(getDiffState([createDiff({ acceptableDiff: true })])).toBe("accepted");

    expect(getDiffState([
        createDiff({ index: 0, acceptableDiff: true }),
        createDiff({ index: 1, acceptableDiff: false })
    ])).toBe("outstanding");
});

it("should outline an outstanding field in red and an accepted one in green", () => {
    expect(getHighlightStyle("none")).toEqual({});
    expect(getHighlightStyle("outstanding").border).toBe("2px solid #dc3545");
    expect(getHighlightStyle("accepted").border).toBe("2px solid #198754");
});
