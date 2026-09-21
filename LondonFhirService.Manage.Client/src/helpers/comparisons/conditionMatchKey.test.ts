import { expect, it } from "vitest";
import { buildConditionMatchKey } from "./conditionMatchKey";
import type { FhirResource } from "../../models/foundations/fhir/FhirResource";

const snomedSystem = "http://snomed.info/sct";

// The key has to be the one ConditionMatcherService built, or a difference is shown against the
// wrong condition - or against every one of them.
it("should build the key from the SNOMED code", () => {
    const condition: FhirResource = {
        resourceType: "Condition",
        id: "cond-1",
        code: { coding: [{ system: snomedSystem, code: "44054006", display: "Diabetes" }] }
    };

    expect(buildConditionMatchKey(condition)).toBe("44054006");
});

// A concept can carry several codings. Matching on whichever happens to be first would pair
// conditions the server did not pair.
it("should ignore codings from other systems", () => {
    const condition: FhirResource = {
        resourceType: "Condition",
        id: "cond-1",
        code: {
            coding: [
                { system: "http://example.org/local", code: "local-code" },
                { system: snomedSystem, code: "44054006" }
            ]
        }
    };

    expect(buildConditionMatchKey(condition)).toBe("44054006");
});

// The server returns no key in this case either, so the condition carries no identifier for a
// difference to be matched against.
it("should have no key without a SNOMED coding", () => {
    const withoutSnomed: FhirResource = {
        resourceType: "Condition",
        code: { coding: [{ system: "http://example.org/local", code: "local-code" }] }
    };

    expect(buildConditionMatchKey(withoutSnomed)).toBeNull();
    expect(buildConditionMatchKey({ resourceType: "Condition", id: "cond-2" })).toBeNull();
});
