import { readCodingBySystem, readString } from "../fhir/fhirJson";
import type { FhirResource } from "../../models/foundations/fhir/FhirResource";

const snomedSystem = "http://snomed.info/sct";

// Mirrors the match key ConditionMatcherService computes server side, so a difference the engine
// recorded against a condition can be shown on that condition's own row.
//
// A condition has no stable id across providers - the two sides mint their own - so the server
// matches on its SNOMED code alone, and writes that code as the difference's identifier. Returns
// null when the condition carries no SNOMED coding, which is also when the engine recorded no
// identifier for it.
export function buildConditionMatchKey(resource: FhirResource): string | null {
    return readString(readCodingBySystem(resource.code, snomedSystem)?.code);
}
