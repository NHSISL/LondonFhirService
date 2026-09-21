import { expect, it } from "vitest";
import { buildAllergyIntoleranceMatchKey } from "./allergyIntoleranceMatchKey";
import type { FhirResource } from "../../models/foundations/fhir/FhirResource";

const snomedSystem = "http://snomed.info/sct";

// The key has to be the one AllergyIntoleranceMatcherService built, or a difference is shown
// against the wrong allergy - or against every one of them.
it("should build the key from the SNOMED code and onset date", () => {
    const allergy: FhirResource = {
        resourceType: "AllergyIntolerance",
        id: "allergy-1",
        onsetDateTime: "2020-05-01",
        code: { coding: [{ system: snomedSystem, code: "91936005", display: "Penicillin allergy" }] }
    };

    expect(buildAllergyIntoleranceMatchKey(allergy)).toBe("91936005|2020-05-01");
});

it("should ignore codings from other systems", () => {
    const allergy: FhirResource = {
        resourceType: "AllergyIntolerance",
        id: "allergy-1",
        onsetDateTime: "2020-05-01",
        code: {
            coding: [
                { system: "http://example.org/local", code: "local-code" },
                { system: snomedSystem, code: "91936005" }
            ]
        }
    };

    expect(buildAllergyIntoleranceMatchKey(allergy)).toBe("91936005|2020-05-01");
});

it("should have no key without both a SNOMED code and an onset date", () => {
    const withoutDate: FhirResource = {
        resourceType: "AllergyIntolerance",
        code: { coding: [{ system: snomedSystem, code: "91936005" }] }
    };

    const withoutCode: FhirResource = {
        resourceType: "AllergyIntolerance",
        onsetDateTime: "2020-05-01"
    };

    expect(buildAllergyIntoleranceMatchKey(withoutDate)).toBeNull();
    expect(buildAllergyIntoleranceMatchKey(withoutCode)).toBeNull();
});
