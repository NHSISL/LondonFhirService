export class FhirRecordServiceException extends Error {
    public readonly innerException: unknown;

    constructor(message: string, innerException: unknown) {
        super(message);
        this.name = "FhirRecordServiceException";
        this.innerException = innerException;
    }
}
