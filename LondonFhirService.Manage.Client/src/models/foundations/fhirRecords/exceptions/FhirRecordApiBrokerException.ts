export class FhirRecordApiBrokerException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "FhirRecordApiBrokerException";
        this.innerException = innerException;
    }
}
