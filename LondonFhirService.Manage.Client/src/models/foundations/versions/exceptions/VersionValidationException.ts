export class VersionValidationException extends Error {
    public readonly fieldName: string;

    constructor(fieldName: string, reason: string) {
        super(`${fieldName}: ${reason}`);
        this.name = "VersionValidationException";
        this.fieldName = fieldName;
    }
}
