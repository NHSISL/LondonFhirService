import { describe, expect, it } from "vitest";
import { buildPractitionerMatchKey } from "./practitionerMatchKey";
import type { FhirResource } from "../../models/foundations/fhir/FhirResource";

const practitioner = (identifiers: unknown[]): FhirResource =>
    ({ resourceType: "Practitioner", identifier: identifiers } as FhirResource);

const sds = (value?: string) =>
    value === undefined
        ? { system: "https://fhir.nhs.uk/Id/sds-user-id" }
        : { system: "https://fhir.nhs.uk/Id/sds-user-id", value };

const dds = (value: string) => ({ system: "https://fhir.hl7.org.uk/Id/dds", value });

describe("buildPractitionerMatchKey", () => {
    it("prefers the sds user id, which means the same to both providers", () => {
        expect(buildPractitionerMatchKey(practitioner([sds("G123456"), dds("22338")])))
            .toBe("G123456");
    });

    // The case that was dropping every practitioner from the comparison: the identifier entry is
    // present but carries no value, so the sds lookup reads as absent.
    it("falls back to the dds identifier when the sds entry carries no value", () => {
        expect(buildPractitionerMatchKey(practitioner([sds(), dds("22338")])))
            .toBe("|22338");
    });

    it("falls back when there is no sds entry at all", () => {
        expect(buildPractitionerMatchKey(practitioner([dds("22338")]))).toBe("|22338");
    });

    it("returns null when neither identifier is usable", () => {
        expect(buildPractitionerMatchKey(practitioner([sds()]))).toBeNull();
        expect(buildPractitionerMatchKey(practitioner([]))).toBeNull();
    });

    // The two key spaces have to stay disjoint or an sds id could be read as a dds one.
    it("keeps the sds and dds key spaces apart", () => {
        const bySds = buildPractitionerMatchKey(practitioner([sds("22338")]));
        const byDds = buildPractitionerMatchKey(practitioner([dds("22338")]));

        expect(bySds).toBe("22338");
        expect(byDds).toBe("|22338");
        expect(bySds).not.toBe(byDds);
    });
});
