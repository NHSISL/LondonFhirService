export class PatientValidationException extends Error {
    public readonly fieldName: string;

    constructor(fieldName: string, reason: string) {
        super(`${fieldName}: ${reason}`);
        this.name = "PatientValidationException";
        this.fieldName = fieldName;
    }
}
