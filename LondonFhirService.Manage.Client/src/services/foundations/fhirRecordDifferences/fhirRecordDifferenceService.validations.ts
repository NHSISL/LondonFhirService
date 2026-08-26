import { FhirRecordDifferenceValidationException } from "../../../models/foundations/fhirRecordDifferences/exceptions/FhirRecordDifferenceValidationException";
import type { FhirRecordDifference } from "../../../models/foundations/fhirRecordDifferences/FhirRecordDifference";
import type { FhirRecordDifferenceQuery } from "../../../models/foundations/fhirRecordDifferences/FhirRecordDifferenceQuery";

// Mirrors the server's own cap - FhirRecordDifferencesController is decorated with EnableQuery,
// and asking for more than a page than it will serve silently truncates instead of failing.
const maximumTake = 50;

export function validateFhirRecordDifferenceId(fhirRecordDifferenceId: string): void {
    if (fhirRecordDifferenceId === null || fhirRecordDifferenceId === undefined) {
        throw new FhirRecordDifferenceValidationException(
            "fhirRecordDifferenceId",
            "A comparison id is required.");
    }

    if (fhirRecordDifferenceId.trim().length === 0) {
        throw new FhirRecordDifferenceValidationException(
            "fhirRecordDifferenceId",
            "A comparison id cannot be blank.");
    }
}

export function validateFhirRecordDifferenceQuery(
    fhirRecordDifferenceQuery: FhirRecordDifferenceQuery)
    : void {
    if (fhirRecordDifferenceQuery === null || fhirRecordDifferenceQuery === undefined) {
        throw new FhirRecordDifferenceValidationException("query", "A query is required.");
    }

    if (fhirRecordDifferenceQuery.skip < 0) {
        throw new FhirRecordDifferenceValidationException("skip", "Skip cannot be negative.");
    }

    if (fhirRecordDifferenceQuery.take <= 0) {
        throw new FhirRecordDifferenceValidationException("take", "Take must be greater than 0.");
    }

    if (fhirRecordDifferenceQuery.take > maximumTake) {
        throw new FhirRecordDifferenceValidationException(
            "take",
            `Take cannot be greater than ${maximumTake}.`);
    }
}

// The server rejects a modify whose audit and comparison values are blank, and compares the
// created values against storage, so an edit has to carry back exactly what it was given.
export function validateFhirRecordDifferenceModification(
    fhirRecordDifference: FhirRecordDifference)
    : void {
    if (fhirRecordDifference === null || fhirRecordDifference === undefined) {
        throw new FhirRecordDifferenceValidationException(
            "fhirRecordDifference",
            "A comparison is required.");
    }

    validateFhirRecordDifferenceId(fhirRecordDifference.id);
    validateRequiredText(fhirRecordDifference.primaryId, "primaryId", "A primary record id");
    validateRequiredText(fhirRecordDifference.secondaryId, "secondaryId", "A secondary record id");
    validateRequiredText(fhirRecordDifference.correlationId, "correlationId", "A correlation id");
    validateRequiredText(fhirRecordDifference.diffJson, "diffJson", "The comparison result");
    validateRequiredText(fhirRecordDifference.createdBy, "createdBy", "The original created by");

    validateRequiredText(
        fhirRecordDifference.createdDate,
        "createdDate",
        "The original created date");
}

function validateRequiredText(value: string, fieldName: string, description: string): void {
    if (!value || value.trim().length === 0) {
        throw new FhirRecordDifferenceValidationException(
            fieldName,
            `${description} is required.`);
    }
}
