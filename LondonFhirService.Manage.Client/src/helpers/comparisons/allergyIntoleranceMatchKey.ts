import { readCodingBySystem, readString } from "../fhir/fhirJson";
import type { FhirResource } from "../../models/foundations/fhir/FhirResource";

const snomedSystem = "http://snomed.info/sct";

// Mirrors the match key AllergyIntoleranceMatcherService computes server side, so a difference
// the engine recorded against an allergy can be shown on that allergy's own row.
//
// An allergy has no stable id across providers - the two sides mint their own - so the server
// pairs them on SNOMED code and onset date, and writes that pair as the difference's identifier.
// Returns null when the allergy has no key, which is also when the engine recorded no identifier
// for it.
export function buildAllergyIntoleranceMatchKey(resource: FhirResource): string | null {
    const snomedCode = readString(readCodingBySystem(resource.code, snomedSystem)?.code);
    const onsetDateTime = readString(resource.onsetDateTime);

    if (snomedCode === null || onsetDateTime === null) {
        return null;
    }

    return `${snomedCode}|${onsetDateTime}`;
}
