import { extractReference } from "../fhir/fhirResourceParsers";
import { readObjectArray, readString } from "../fhir/fhirJson";
import type { FhirResource, FhirResourceIndex } from "../../models/foundations/fhir/FhirResource";

const snomedSystem = "http://snomed.info/sct";

// Rebuilds the key MedicationStatementMatcherService matches on, so a difference the engine
// recorded against a medication can be shown on that medication's own row.
//
// A medication statement has no stable id across providers - the two sides mint their own - so
// the server pairs them on SNOMED code and asserted date, and writes that pair as the
// difference's identifier. Matching a row to its differences means computing the same key here;
// anything else either mislabels every medication or none. Returns null when the statement has no
// key, which is also when the engine recorded no identifier for it.
export function buildMedicationStatementMatchKey(
    resource: FhirResource,
    resourceIndex: FhirResourceIndex)
    : string | null {
    const snomedCode = readMedicationSnomedCode(resource, resourceIndex);
    const dateAsserted = readString(resource.dateAsserted);

    if (snomedCode === null || dateAsserted === null) {
        return null;
    }

    return `${snomedCode}|${dateAsserted}`;
}

// medicationReference first, matching the server: a statement that points at a contained
// Medication carries its code there rather than inline.
function readMedicationSnomedCode(
    resource: FhirResource,
    resourceIndex: FhirResourceIndex)
    : string | null {
    const medicationReference = extractReference(resource.medicationReference);

    if (medicationReference !== null) {
        const medication = resourceIndex.get(medicationReference);

        if (medication !== undefined) {
            const referencedCode = readSnomedCodeFromCodeableConcept(medication.code);

            if (referencedCode !== null) {
                return referencedCode;
            }
        }
    }

    return readSnomedCodeFromCodeableConcept(resource.medicationCodeableConcept);
}

// The first SNOMED coding specifically - a concept can carry several codings, and matching on
// whichever happens to be first would pair statements the server did not pair.
function readSnomedCodeFromCodeableConcept(codeableConcept: unknown): string | null {
    const snomedCoding = readObjectArray(
        typeof codeableConcept === "object" && codeableConcept !== null
            ? (codeableConcept as FhirResource).coding
            : undefined)
        .find(coding => readString(coding.system) === snomedSystem);

    return snomedCoding === undefined ? null : readString(snomedCoding.code);
}
