import { expect, it } from "vitest";
import { buildDdsIdentifierMatchKey } from "./ddsIdentifierMatchKey";
import type { FhirResource } from "../../models/foundations/fhir/FhirResource";

const ddsIdentifierSystem = "https://fhir.hl7.org.uk/Id/dds";

// The key has to be the one the server's DDS-identifier matchers built, or a difference is shown
// against the wrong resource - or against every one of them.
it("should read the identifier whose system is the DDS feed", () => {
    const observation: FhirResource = {
        resourceType: "Observation",
        id: "obs-1",
        identifier: [{ system: ddsIdentifierSystem, value: "58" }]
    };

    expect(buildDdsIdentifierMatchKey(observation)).toBe("58");
});

// A resource can carry more than one identifier - the official NHS one and the DDS feed's own -
// so picking whichever happens to be first would pair on the wrong one.
it("should ignore identifiers from other systems", () => {
    const immunization: FhirResource = {
        resourceType: "Immunization",
        id: "imm-1",
        identifier: [
            { system: "https://fhir.nhs.uk/Id/immunisation-id", value: "official-id" },
            { system: ddsIdentifierSystem, value: "IMM-1234" }
        ]
    };

    expect(buildDdsIdentifierMatchKey(immunization)).toBe("IMM-1234");
});

// The server returns no key in this case either, so the resource carries no identifier for a
// difference to be matched against.
it("should have no key without a DDS identifier", () => {
    const encounter: FhirResource = {
        resourceType: "Encounter",
        id: "enc-1",
        identifier: [{ system: "http://example.org/system", value: "ENC-1" }]
    };

    expect(buildDdsIdentifierMatchKey(encounter)).toBeNull();
    expect(buildDdsIdentifierMatchKey({ resourceType: "Encounter", id: "enc-2" })).toBeNull();
});
