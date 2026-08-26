import { expect, it } from "vitest";
import { getDiffTypeForField, getHighlightStyle } from "./diffHighlighting";
import type { DiffItemView } from "../../models/views/comparisons/DiffItemView";

const createDiff = (overrides: Partial<DiffItemView>): DiffItemView => ({
    key: "0-path",
    type: "modified",
    typeText: "Modified",
    typeClassName: "badge bg-warning text-dark",
    path: "gender",
    oldValueText: null,
    newValueText: null,
    resourceTypeText: "Patient",
    identifierText: null,
    reasonText: null,
    ...overrides
});

it("should map the tail of a path onto the card field it belongs to", () => {
    const diffs = [createDiff({ path: "$.Patient[1].name[0].\"family\"" })];

    expect(getDiffTypeForField(diffs, "nameFamily", "primary")).toBe("modified");
    expect(getDiffTypeForField(diffs, "nameGiven", "primary")).toBeNull();
});

// The engine compares the primary as source1 and the secondary as source2, so a removal is
// something only the primary has and an addition only the secondary.
it("should show a removal on the primary side and an addition on the secondary", () => {
    const removed = [createDiff({ type: "removed", path: "telecom" })];
    const added = [createDiff({ type: "added", path: "telecom" })];

    expect(getDiffTypeForField(removed, "telecom", "primary")).toBe("removed");
    expect(getDiffTypeForField(removed, "telecom", "secondary")).toBeNull();
    expect(getDiffTypeForField(added, "telecom", "secondary")).toBe("added");
    expect(getDiffTypeForField(added, "telecom", "primary")).toBeNull();
});

it("should show a modification on both sides", () => {
    const diffs = [createDiff({ type: "modified", path: "birthDate" })];

    expect(getDiffTypeForField(diffs, "birthDate", "primary")).toBe("modified");
    expect(getDiffTypeForField(diffs, "birthDate", "secondary")).toBe("modified");
});

it("should ignore a path with no field it recognises", () => {
    const diffs = [createDiff({ path: "$.Patient[1].meta.versionId" })];

    expect(getDiffTypeForField(diffs, "birthDate", "primary")).toBeNull();
});

it("should style only a field that differs", () => {
    expect(getHighlightStyle(null)).toEqual({});
    expect(getHighlightStyle("modified").border).toBe("2px solid #dc3545");
});
