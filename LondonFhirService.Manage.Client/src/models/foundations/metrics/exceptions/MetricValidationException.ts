export class MetricValidationException extends Error {
    public readonly fieldName: string;

    constructor(fieldName: string, reason: string) {
        super(`${fieldName}: ${reason}`);
        this.name = "MetricValidationException";
        this.fieldName = fieldName;
    }
}
