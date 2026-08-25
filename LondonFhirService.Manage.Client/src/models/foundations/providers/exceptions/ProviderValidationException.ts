export class ProviderValidationException extends Error {
    public readonly fieldName: string;

    constructor(fieldName: string, reason: string) {
        super(`${fieldName}: ${reason}`);
        this.name = "ProviderValidationException";
        this.fieldName = fieldName;
    }
}
