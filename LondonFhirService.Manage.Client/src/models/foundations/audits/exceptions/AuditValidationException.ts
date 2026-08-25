export class AuditValidationException extends Error {
    public readonly fieldName: string;

    constructor(fieldName: string, reason: string) {
        super(`${fieldName}: ${reason}`);
        this.name = "AuditValidationException";
        this.fieldName = fieldName;
    }
}
