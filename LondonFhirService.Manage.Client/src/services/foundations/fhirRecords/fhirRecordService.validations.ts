import { FhirRecordValidationException } from "../../../models/foundations/fhirRecords/exceptions/FhirRecordValidationException";
import type { FhirRecordQuery } from "../../../models/foundations/fhirRecords/FhirRecordQuery";

export function validateFhirRecordId(fhirRecordId: string): void {
    if (fhirRecordId === null || fhirRecordId === undefined) {
        throw new FhirRecordValidationException("fhirRecordId", "A FHIR record id is required.");
    }

    if (fhirRecordId.trim().length === 0) {
        throw new FhirRecordValidationException(
            "fhirRecordId",
            "A FHIR record id cannot be blank.");
    }
}

export function validateFhirRecordQuery(fhirRecordQuery: FhirRecordQuery): void {
    if (fhirRecordQuery === null || fhirRecordQuery === undefined) {
        throw new FhirRecordValidationException(
            "fhirRecordQuery",
            "A FHIR record query is required.");
    }

    // A take of zero would ask the endpoint for nothing and read the empty answer as an empty
    // queue, which is the one wrong conclusion this list can draw.
    if (Number.isInteger(fhirRecordQuery.take) === false || fhirRecordQuery.take <= 0) {
        throw new FhirRecordValidationException(
            "take",
            "A FHIR record query take must be a whole number greater than zero.");
    }

    // The filter builder calls .trim() on this. Unchecked, a caller that omitted it produced a
    // TypeError, which the layer's catch reports as a service fault - telling the operator to
    // contact support about input the caller got wrong.
    if (typeof fhirRecordQuery.searchTerm !== "string") {
        throw new FhirRecordValidationException(
            "searchTerm",
            "A FHIR record query search term is required, and may be blank.");
    }
}

