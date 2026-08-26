import { expect, it } from "vitest";
import {
    getDiffState,
    getDiffsForField,
    getDiffsForFields,
    getHighlightStyle,
    getMappableFields,
    getOtherDiffs,
    getUnhighlightedPatientDiffs,
    patientHighlightFields
} from "./diffHighlighting";
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

// The differences list counted a generalPractitioner change that the card had no box for, so it
// showed one difference where the list showed two. The mapping is what closes that gap.
it("should map a general practitioner reference onto its card field", () => {
    const diffs = [createDiff({ path: "$.Patient[123].generalPractitioner[0].reference" })];

    expect(getDiffsForField(diffs, "generalPractitionerRefs", "secondary")).toHaveLength(1);
    expect(getUnhighlightedPatientDiffs(diffs, "secondary")).toEqual([]);
});

// The header renders the whole formatted name, so a change to any part of it belongs in the one
// outline rather than falling through to Other differences.
it("should gather every part of the name into one box", () => {
    const diffs = [
        createDiff({ key: "0", index: 0, path: "$.Patient[123].name[0].\"family\"" }),
        createDiff({ key: "1", index: 1, path: "$.Patient[123].name[0].given[0]" })
    ];

    const nameDiffs = getDiffsForFields(
        diffs,
        ["nameFamily", "nameGiven", "namePrefix", "nameSuffix"],
        "secondary");

    expect(nameDiffs).toHaveLength(2);
    expect(getUnhighlightedPatientDiffs(diffs, "secondary")).toEqual([]);
});

// Anything the card has no field for still has to be shown, or the card and the differences list
// disagree about how many differences there are.
it("should surface a difference no card field claims", () => {
    const diffs = [createDiff({ path: "$.Patient[123].maritalStatus.coding[0].code" })];

    expect(getUnhighlightedPatientDiffs(diffs, "secondary")).toHaveLength(1);
});

it("should keep an unclaimed difference on the side it belongs to", () => {
    const diffs = [
        createDiff({ key: "0", index: 0, type: "removed", path: "$.Patient[123].maritalStatus" }),
        createDiff({ key: "1", index: 1, type: "added", path: "$.Patient[123].deceasedBoolean" })
    ];

    expect(getUnhighlightedPatientDiffs(diffs, "primary").map(diff => diff.key)).toEqual(["0"]);
    expect(getUnhighlightedPatientDiffs(diffs, "secondary").map(diff => diff.key)).toEqual(["1"]);
});

// The two lists have to agree. A path that maps to a field the card never draws would drop its
// difference out of the card without landing in Other differences either, which is exactly the
// silent loss this pairing exists to prevent.
it("should render a box for every field a path can map to", () => {
    expect([...getMappableFields()].sort()).toEqual([...patientHighlightFields].sort());
});

// The card only lays out Patient, EpisodeOfCare, List and MedicationStatement. A difference in
// anything else had nowhere to appear, so the card showed fewer differences than the list and
// those it omitted could not be ticked from it.
it("should surface a difference against a resource the card has no section for", () => {
    const diffs = [
        createDiff({ key: "0", index: 0, resourceTypeText: "Organization", path: "name" }),
        createDiff({ key: "1", index: 1, resourceTypeText: "Practitioner", path: "name" })
    ];

    expect(getOtherDiffs(diffs, "secondary").map(diff => diff.key)).toEqual(["0", "1"]);
});

it("should leave a difference its own section already shows out of the other list", () => {
    const diffs = [
        createDiff({ key: "0", index: 0, resourceTypeText: "List", path: "entry" }),
        createDiff({ key: "1", index: 1, resourceTypeText: "EpisodeOfCare", path: "status" }),
        createDiff({ key: "2", index: 2, resourceTypeText: "MedicationStatement", path: "dosage" }),
        createDiff({ key: "3", index: 3, resourceTypeText: "Patient", path: "gender" })
    ];

    expect(getOtherDiffs(diffs, "secondary")).toEqual([]);
});
