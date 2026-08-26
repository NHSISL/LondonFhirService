import { expect, it } from "vitest";
import { buildMedicationStatementMatchKey } from "./medicationStatementMatchKey";
import type { FhirResource, FhirResourceIndex } from "../../models/foundations/fhir/FhirResource";

const snomedSystem = "http://snomed.info/sct";

const emptyIndex: FhirResourceIndex = new Map();

// The key has to be the one MedicationStatementMatcherService built, or a difference is shown
// against the wrong medication - or against every one of them.
it("should build the key from an inline SNOMED code and the asserted date", () => {
    const medicationStatement: FhirResource = {
        resourceType: "MedicationStatement",
        id: "med-1",
        dateAsserted: "2026-01-02",
        medicationCodeableConcept: {
            coding: [{ system: snomedSystem, code: "322236009", display: "Paracetamol" }]
        }
    };

    expect(buildMedicationStatementMatchKey(medicationStatement, emptyIndex))
        .toBe("322236009|2026-01-02");
});

it("should prefer the code on a referenced Medication resource", () => {
    const resourceIndex: FhirResourceIndex = new Map([
        [
            "Medication/medication-1",
            {
                resourceType: "Medication",
                id: "medication-1",
                code: { coding: [{ system: snomedSystem, code: "referenced-code" }] }
            }
        ]
    ]);

    const medicationStatement: FhirResource = {
        resourceType: "MedicationStatement",
        id: "med-1",
        dateAsserted: "2026-01-02",
        medicationReference: { reference: "Medication/medication-1" },
        medicationCodeableConcept: {
            coding: [{ system: snomedSystem, code: "inline-code" }]
        }
    };

    expect(buildMedicationStatementMatchKey(medicationStatement, resourceIndex))
        .toBe("referenced-code|2026-01-02");
});

// A concept can carry several codings. Matching on whichever happens to be first would pair
// statements the server did not pair.
it("should ignore codings from other systems", () => {
    const medicationStatement: FhirResource = {
        resourceType: "MedicationStatement",
        id: "med-1",
        dateAsserted: "2026-01-02",
        medicationCodeableConcept: {
            coding: [
                { system: "http://example.org/local", code: "local-code" },
                { system: snomedSystem, code: "322236009" }
            ]
        }
    };

    expect(buildMedicationStatementMatchKey(medicationStatement, emptyIndex))
        .toBe("322236009|2026-01-02");
});

// The server returns no key in these cases either, so the statement carries no identifier for a
// difference to be matched against.
it("should have no key without both a SNOMED code and an asserted date", () => {
    const withoutDate: FhirResource = {
        resourceType: "MedicationStatement",
        medicationCodeableConcept: { coding: [{ system: snomedSystem, code: "322236009" }] }
    };

    const withoutCode: FhirResource = {
        resourceType: "MedicationStatement",
        dateAsserted: "2026-01-02"
    };

    expect(buildMedicationStatementMatchKey(withoutDate, emptyIndex)).toBeNull();
    expect(buildMedicationStatementMatchKey(withoutCode, emptyIndex)).toBeNull();
});
