export class FhirRecordDifferenceValidationException extends Error {
    public readonly fieldName: string;

    constructor(fieldName: string, reason: string) {
        super(`${fieldName}: ${reason}`);
        this.name = "FhirRecordDifferenceValidationException";
        this.fieldName = fieldName;
    }
}
