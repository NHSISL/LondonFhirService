export class FhirRecordValidationException extends Error {
    public readonly fieldName: string;

    constructor(fieldName: string, reason: string) {
        super(`${fieldName}: ${reason}`);
        this.name = "FhirRecordValidationException";
        this.fieldName = fieldName;
    }
}
