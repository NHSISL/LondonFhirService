import { expect, it } from "vitest";
import { parseFamilyMemberHistory, parseImmunization } from "./fhirResourceParsers";
import type { FhirResource } from "../../models/foundations/fhir/FhirResource";

const snomedSystem = "http://snomed.info/sct";

// The core vaccineCode element is what the FHIR spec expects, so it wins when present.
it("should read the vaccine from vaccineCode when present", () => {
    const immunization: FhirResource = {
        resourceType: "Immunization",
        id: "imm-1",
        vaccineCode: {
            coding: [{ system: snomedSystem, code: "871873002", display: "Influenza vaccine" }]
        }
    };

    const result = parseImmunization(immunization);

    expect(result.display).toBe("Influenza vaccine");
    expect(result.code).toBe("871873002");
});

// Some feeds carry the vaccine given as an extension's CodeableConcept instead of the core
// vaccineCode element - vaccineCode is absent altogether.
it("should fall back to a codeable concept extension when vaccineCode is absent", () => {
    const immunization: FhirResource = {
        resourceType: "Immunization",
        id: "imm-1",
        extension: [
            {
                url: "https://fhir.nhs.uk/STU3/StructureDefinition/Extension-CareConnect-GPC-DateRecorded-1",
                valueDateTime: "2020-01-20T00:00:00+00:00"
            },
            {
                url: null,
                valueCodeableConcept: {
                    coding: [
                        {
                            system: "http://read.info/readv2",
                            code: "65ED.",
                            display: "Seasonal influenza vaccination"
                        }
                    ]
                }
            }
        ]
    };

    const result = parseImmunization(immunization);

    expect(result.display).toBe("Seasonal influenza vaccination");
    expect(result.code).toBe("65ED.");
});

it("should read a relative's relationship and name when present", () => {
    const familyMemberHistory: FhirResource = {
        resourceType: "FamilyMemberHistory",
        id: "fmh-1",
        name: "John Smith Senior",
        relationship: {
            coding: [{ system: "http://hl7.org/fhir/v3/v3-RoleCode", code: "FTH", display: "Father" }]
        }
    };

    const result = parseFamilyMemberHistory(familyMemberHistory);

    expect(result.name).toBe("John Smith Senior");
    expect(result.relationshipDisplay).toBe("Father");
});

// Some feeds record this as a screening statement about the whole family - "no family history of
// malignancy" - with no relative or relationship at all, only a condition code.
it("should read the condition code when there is no relationship or name", () => {
    const familyMemberHistory: FhirResource = {
        resourceType: "FamilyMemberHistory",
        id: "fmh-1",
        condition: [
            {
                code: {
                    coding: [
                        {
                            system: snomedSystem,
                            code: "160250007",
                            display: "No family history of malignancy (situation)"
                        }
                    ]
                }
            }
        ]
    };

    const result = parseFamilyMemberHistory(familyMemberHistory);

    expect(result.name).toBeNull();
    expect(result.relationshipCode).toBeNull();
    expect(result.conditionDisplay).toBe("No family history of malignancy (situation)");
    expect(result.conditionCode).toBe("160250007");
});
