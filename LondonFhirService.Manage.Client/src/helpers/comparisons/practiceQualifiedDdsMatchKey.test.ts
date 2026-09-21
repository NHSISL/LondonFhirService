import { describe, expect, it } from "vitest";
import { buildPracticeQualifiedDdsMatchKey } from "./practiceQualifiedDdsMatchKey";
import type { FhirResource } from "../../models/foundations/fhir/FhirResource";

const resource = (ddsValue: string | null, odsCode: string | null): FhirResource => ({
    resourceType: "Observation",
    ...(odsCode === null
        ? {}
        : { meta: { tag: [{ system: "https://fhir.nhs.uk/Id/ODS-Code", code: odsCode }] } }),
    ...(ddsValue === null
        ? {}
        : { identifier: [{ system: "https://fhir.hl7.org.uk/Id/dds", value: ddsValue }] })
} as FhirResource);

describe("buildPracticeQualifiedDdsMatchKey", () => {
    it("qualifies the dds identifier with the issuing practice", () => {
        expect(buildPracticeQualifiedDdsMatchKey(resource("54", "F84642"))).toBe("F84642|54");
    });

    // The whole point: the same dds id from two practices must not produce the same key, or the
    // server cannot pair them and the portal cannot attach their differences to the right row.
    it("separates the same dds identifier issued by different practices", () => {
        const first = buildPracticeQualifiedDdsMatchKey(resource("54", "F84642"));
        const second = buildPracticeQualifiedDdsMatchKey(resource("54", "E84015"));

        expect(first).not.toBe(second);
    });

    it("falls back to an empty practice when the resource carries no ods tag", () => {
        expect(buildPracticeQualifiedDdsMatchKey(resource("54", null))).toBe("|54");
    });

    it("returns null when the resource carries no dds identifier", () => {
        expect(buildPracticeQualifiedDdsMatchKey(resource(null, "F84642"))).toBeNull();
    });

    it("ignores tags from other systems", () => {
        const withOtherTag = {
            resourceType: "Observation",
            meta: { tag: [{ system: "http://example.invalid/other", code: "XXX" }] },
            identifier: [{ system: "https://fhir.hl7.org.uk/Id/dds", value: "54" }]
        } as FhirResource;

        expect(buildPracticeQualifiedDdsMatchKey(withOtherTag)).toBe("|54");
    });

    it("reads as absent rather than throwing when meta.tag is malformed", () => {
        const malformed = {
            resourceType: "Observation",
            meta: { tag: "not-an-array" },
            identifier: [{ system: "https://fhir.hl7.org.uk/Id/dds", value: "54" }]
        } as unknown as FhirResource;

        expect(buildPracticeQualifiedDdsMatchKey(malformed)).toBe("|54");
    });
});
