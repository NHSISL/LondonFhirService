export class FhirRecordDifferenceApiBrokerException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "FhirRecordDifferenceApiBrokerException";
        this.innerException = innerException;
    }
}
