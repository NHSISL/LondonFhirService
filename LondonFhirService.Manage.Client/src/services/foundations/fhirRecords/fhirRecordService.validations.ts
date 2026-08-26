import { FhirRecordValidationException } from "../../../models/foundations/fhirRecords/exceptions/FhirRecordValidationException";

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

