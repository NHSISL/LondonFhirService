import { expect, it } from "vitest";
import { findAllReferences, findUnlistedReferences } from "./unlistedResources";
import type { FhirResourceIndex } from "../../models/foundations/fhir/FhirResource";
import type { ListData } from "../../models/foundations/fhir/ListData";

const emptyList = (itemRefs: string[]): ListData => ({
    id: "list-1",
    title: "Some list",
    status: null,
    itemCount: itemRefs.length,
    itemRefs: itemRefs
});

// A card that only walked List entries would never show these, despite the comparison reporting
// differences against them.
it("should return a resource of the given type that no list points at", () => {
    const resourceIndex: FhirResourceIndex = new Map([
        ["Immunization/imm-1", { resourceType: "Immunization", id: "imm-1" }],
        ["Observation/obs-1", { resourceType: "Observation", id: "obs-1" }]
    ]);

    expect(findUnlistedReferences(resourceIndex, [], "Immunization"))
        .toEqual(["Immunization/imm-1"]);
});

it("should exclude a resource a list already points at", () => {
    const resourceIndex: FhirResourceIndex = new Map([
        ["Immunization/imm-1", { resourceType: "Immunization", id: "imm-1" }],
        ["Immunization/imm-2", { resourceType: "Immunization", id: "imm-2" }]
    ]);

    const lists = [emptyList(["Immunization/imm-1"])];

    expect(findUnlistedReferences(resourceIndex, lists, "Immunization"))
        .toEqual(["Immunization/imm-2"]);
});

it("should not match a different resource type sharing a prefix", () => {
    const resourceIndex: FhirResourceIndex = new Map([
        ["Encounter/enc-1", { resourceType: "Encounter", id: "enc-1" }],
        ["EncounterHistory/eh-1", { resourceType: "EncounterHistory", id: "eh-1" }]
    ]);

    expect(findUnlistedReferences(resourceIndex, [], "Encounter")).toEqual(["Encounter/enc-1"]);
});

it("should return nothing when every resource of that type is listed", () => {
    const resourceIndex: FhirResourceIndex = new Map([
        ["Condition/cond-1", { resourceType: "Condition", id: "cond-1" }]
    ]);

    const lists = [emptyList(["Condition/cond-1"])];

    expect(findUnlistedReferences(resourceIndex, lists, "Condition")).toEqual([]);
});

// A dedicated section groups every Observation regardless of whether a provider's List already
// references it, so a feed that folds them into a list titled "Miscellaneous record" still gets
// them all under a name that says what they are.
it("should return every resource of the given type, listed or not", () => {
    const resourceIndex: FhirResourceIndex = new Map([
        ["Observation/obs-1", { resourceType: "Observation", id: "obs-1" }],
        ["Observation/obs-2", { resourceType: "Observation", id: "obs-2" }],
        ["Condition/cond-1", { resourceType: "Condition", id: "cond-1" }]
    ]);

    expect(findAllReferences(resourceIndex, "Observation"))
        .toEqual(["Observation/obs-1", "Observation/obs-2"]);
});
